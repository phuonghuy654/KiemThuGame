using System.Collections;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class PlayModeExtended
{
    private GameObject tempEnemyObj;
    private GameObject tempTrapObj;
    private GameObject tempLedgeObj;
    private GameObject tempGroundObj;

    [TearDown]
    public void Teardown()
    {
        if (tempEnemyObj != null) Object.DestroyImmediate(tempEnemyObj);
        if (tempTrapObj != null) Object.DestroyImmediate(tempTrapObj);
        if (tempLedgeObj != null) Object.DestroyImmediate(tempLedgeObj);
        if (tempGroundObj != null) Object.DestroyImmediate(tempGroundObj);
    }

    [UnityTest]
    public IEnumerator Trap_Initialization_KeepsObjectIfChanceSucceeds()
    {
        tempTrapObj = new GameObject();
        Trap trap = tempTrapObj.AddComponent<Trap>();

        FieldInfo chanceField = typeof(Trap).GetField("chanceToSpawn", BindingFlags.NonPublic | BindingFlags.Instance);
        chanceField.SetValue(trap, 200f);

        MethodInfo startMethod = typeof(Trap).GetMethod("Start", BindingFlags.NonPublic | BindingFlags.Instance);
        startMethod.Invoke(trap, null);

        yield return null;

        Assert.IsFalse(tempTrapObj == null);
    }

    [Test]
    public void Enemy_AnimationTrigger_ResetsStateAndAllowsMovement()
    {
        tempEnemyObj = new GameObject();
        tempEnemyObj.SetActive(false);

        Enemy enemy = tempEnemyObj.AddComponent<Enemy>();
        Rigidbody2D rb = tempEnemyObj.AddComponent<Rigidbody2D>();

        FieldInfo rbField = typeof(Enemy).GetField("rb", BindingFlags.NonPublic | BindingFlags.Instance);
        rbField.SetValue(enemy, rb);

        rb.gravityScale = 0f;
        enemy.canMove = false;

        FieldInfo defaultGravityField = typeof(Enemy).GetField("defaultGravityScale", BindingFlags.NonPublic | BindingFlags.Instance);
        defaultGravityField.SetValue(enemy, 5f);

        FieldInfo justRespawnedField = typeof(Enemy).GetField("justRespawned", BindingFlags.NonPublic | BindingFlags.Instance);
        justRespawnedField.SetValue(enemy, true);

        MethodInfo animTriggerMethod = typeof(Enemy).GetMethod("AnimatatinTrigger", BindingFlags.NonPublic | BindingFlags.Instance);
        animTriggerMethod.Invoke(enemy, null);

        bool justRespawned = (bool)justRespawnedField.GetValue(enemy);

        Assert.AreEqual(5f, rb.gravityScale);
        Assert.IsTrue(enemy.canMove);
        Assert.IsFalse(justRespawned);
    }

    [Test]
    public void LedgeDetection_OnTriggerEnter2D_GroundLayer_DisablesDetection()
    {
        tempLedgeObj = new GameObject();
        LedgeDetection ledge = tempLedgeObj.AddComponent<LedgeDetection>();
        ledge.canDetect = true;

        tempGroundObj = new GameObject();
        tempGroundObj.layer = LayerMask.NameToLayer("Ground");
        BoxCollider2D groundCollider = tempGroundObj.AddComponent<BoxCollider2D>();

        MethodInfo triggerMethod = typeof(LedgeDetection).GetMethod("OnTriggerEnter2D", BindingFlags.NonPublic | BindingFlags.Instance);
        triggerMethod.Invoke(ledge, new object[] { groundCollider });

        Assert.IsFalse(ledge.canDetect);
    }
}