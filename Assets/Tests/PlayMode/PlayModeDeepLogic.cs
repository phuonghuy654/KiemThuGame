using System.Collections;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class PlayModeDeepLogic
{
    private GameObject tempCoinObj;
    private GameObject tempEnemyObj;
    private GameObject tempPartPrefab;
    private GameObject tempGeneratorObj;
    private GameObject tempPlayerObj;

    [TearDown]
    public void Teardown()
    {
        if (tempCoinObj != null) Object.DestroyImmediate(tempCoinObj);
        if (tempEnemyObj != null) Object.DestroyImmediate(tempEnemyObj);
        if (tempPartPrefab != null) Object.DestroyImmediate(tempPartPrefab);
        if (tempGeneratorObj != null) Object.DestroyImmediate(tempGeneratorObj);
        if (tempPlayerObj != null) Object.DestroyImmediate(tempPlayerObj);
    }

    [UnityTest]
    public IEnumerator Coin_OnTriggerEnter2D_Enemy_DestroysCoin()
    {
        tempCoinObj = new GameObject();
        Coin coin = tempCoinObj.AddComponent<Coin>();

        tempEnemyObj = new GameObject();
        tempEnemyObj.SetActive(false);
        tempEnemyObj.AddComponent<Enemy>();
        BoxCollider2D enemyCollider = tempEnemyObj.AddComponent<BoxCollider2D>();

        MethodInfo triggerMethod = typeof(Coin).GetMethod("OnTriggerEnter2D", BindingFlags.NonPublic | BindingFlags.Instance);
        triggerMethod.Invoke(coin, new object[] { enemyCollider });

        yield return null;

        Assert.IsTrue(tempCoinObj == null);
    }

    [Test]
    public void LevelGenerator_GeneratePlatform_SpawnsNewPartWhenPlayerIsNear()
    {
        tempPartPrefab = new GameObject("Part");
        tempPartPrefab.SetActive(false);

        GameObject startPoint = new GameObject("StartPoint");
        GameObject endPoint = new GameObject("EndPoint");
        startPoint.transform.SetParent(tempPartPrefab.transform);
        endPoint.transform.SetParent(tempPartPrefab.transform);
        startPoint.transform.localPosition = new Vector3(-5f, 0f, 0f);
        endPoint.transform.localPosition = new Vector3(5f, 0f, 0f);

        tempGeneratorObj = new GameObject();
        tempGeneratorObj.SetActive(false);
        LevelGenerator generator = tempGeneratorObj.AddComponent<LevelGenerator>();

        tempPlayerObj = new GameObject();
        tempPlayerObj.transform.position = new Vector3(0f, 0f, 0f);

        FieldInfo levelPartField = typeof(LevelGenerator).GetField("levelPart", BindingFlags.NonPublic | BindingFlags.Instance);
        levelPartField.SetValue(generator, new Transform[] { tempPartPrefab.transform });

        FieldInfo playerField = typeof(LevelGenerator).GetField("player", BindingFlags.NonPublic | BindingFlags.Instance);
        playerField.SetValue(generator, tempPlayerObj.transform);

        FieldInfo distanceToSpawnField = typeof(LevelGenerator).GetField("distanceToSpawn", BindingFlags.NonPublic | BindingFlags.Instance);
        distanceToSpawnField.SetValue(generator, 20f);

        FieldInfo nextPartPositionField = typeof(LevelGenerator).GetField("nextPartPosition", BindingFlags.NonPublic | BindingFlags.Instance);
        nextPartPositionField.SetValue(generator, new Vector3(10f, 0f, 0f));

        MethodInfo generateMethod = typeof(LevelGenerator).GetMethod("GeneratePlatform", BindingFlags.NonPublic | BindingFlags.Instance);
        generateMethod.Invoke(generator, null);

        Assert.IsTrue(tempGeneratorObj.transform.childCount > 0);
    }
}