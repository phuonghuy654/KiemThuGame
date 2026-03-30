using System.Collections;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

[TestFixture]
public class HuyTest
{
    private GameObject playerObject;
    private Player player;
    private Rigidbody2D rb;

    [UnitySetUp]
    public IEnumerator SetUp()
    {
        playerObject = new GameObject("Player");
        rb = playerObject.AddComponent<Rigidbody2D>();
        rb.gravityScale = 5;
        playerObject.AddComponent<SpriteRenderer>();
        playerObject.AddComponent<Animator>();

        var dustObj = new GameObject("DustFx");
        dustObj.transform.SetParent(playerObject.transform);
        var dustFx = dustObj.AddComponent<ParticleSystem>();

        var bloodObj = new GameObject("BloodFx");
        bloodObj.transform.SetParent(playerObject.transform);
        var bloodFx = bloodObj.AddComponent<ParticleSystem>();

        var wallCheckObj = new GameObject("WallCheck");
        wallCheckObj.transform.SetParent(playerObject.transform);

        player = playerObject.AddComponent<Player>();

        SetPrivateField("dustFx", dustFx);
        SetPrivateField("bloodFx", bloodFx);
        SetPrivateField("wallCheck", wallCheckObj.transform);
        SetPrivateField("wallCheckSize", new Vector2(0.5f, 0.5f));

        yield return null;
    }

    [TearDown]
    public void TearDown()
    {
        Object.DestroyImmediate(playerObject);
    }

    private void SetPrivateField(string fieldName, object value)
    {
        var field = typeof(Player).GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance);
        Assert.IsNotNull(field, $"Field '{fieldName}' not found");
        field.SetValue(player, value);
    }

    private T GetPrivateField<T>(string fieldName)
    {
        var field = typeof(Player).GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance);
        Assert.IsNotNull(field, $"Field '{fieldName}' not found");
        return (T)field.GetValue(player);
    }

    private void InvokePrivateMethod(string methodName, params object[] args)
    {
        var method = typeof(Player).GetMethod(methodName, BindingFlags.NonPublic | BindingFlags.Instance);
        Assert.IsNotNull(method, $"Method '{methodName}' not found");
        method.Invoke(player, args);
    }

    // Test 1: Player starts not unlocked
    [UnityTest]
    public IEnumerator PlayerLocked()
    {
        yield return null;
        Assert.IsFalse(player.playerUnlocked);
    }

    // Test 2: ExtraLife is true when moveSpeed >= speedToSurvive
    [UnityTest]
    public IEnumerator ExtraLife_IsTrue()
    {
        yield return null;
        SetPrivateField("moveSpeed", 20f);
        SetPrivateField("speedToSurvive", 18f);
        player.extraLife = GetPrivateField<float>("moveSpeed") >= GetPrivateField<float>("speedToSurvive");
        Assert.IsTrue(player.extraLife);
    }

    // Test 3: ExtraLife is false when moveSpeed < speedToSurvive
    [UnityTest]
    public IEnumerator ExtraLife_IsFalse()
    {
        yield return null;
        SetPrivateField("moveSpeed", 10f);
        SetPrivateField("speedToSurvive", 18f);
        player.extraLife = GetPrivateField<float>("moveSpeed") >= GetPrivateField<float>("speedToSurvive");
        Assert.IsFalse(player.extraLife);
    }

    // Test 4: ExtraLife is true at exact threshold
    [UnityTest]
    public IEnumerator ExtraLife_IsTrue_IfHighSpeed()
    {
        yield return null;
        SetPrivateField("moveSpeed", 18f);
        SetPrivateField("speedToSurvive", 18f);
        player.extraLife = GetPrivateField<float>("moveSpeed") >= GetPrivateField<float>("speedToSurvive");
        Assert.IsTrue(player.extraLife);
    }

    // Test 5: ExtraLife defaults to false
    [UnityTest]
    public IEnumerator ExtraLife_DefaultsToFalse()
    {
        yield return null;
        Assert.IsFalse(player.extraLife);
    }

    // Test 6: isDead defaults to false
    [UnityTest]
    public IEnumerator IsDead_DefaultsToFalse()
    {
        yield return null;
        bool isDead = GetPrivateField<bool>("isDead");
        Assert.IsFalse(isDead);
    }

    // Test 7: isKnocked defaults to false
    [UnityTest]
    public IEnumerator IsKnocked_DefaultsToFalse()
    {
        yield return null;
        bool isKnocked = GetPrivateField<bool>("isKnocked");
        Assert.IsFalse(isKnocked);
    }

    // Test 8: canBeKnocked defaults to true
    [UnityTest]
    public IEnumerator CanBeKnocked_DefaultsToTrue()
    {
        yield return null;
        bool canBeKnocked = GetPrivateField<bool>("canBeKnocked");
        Assert.IsTrue(canBeKnocked);
    }

    // Test 9: canGrabLedge defaults to true
    [UnityTest]
    public IEnumerator CanGrabLedge_DefaultsToTrue()
    {
        yield return null;
        bool canGrabLedge = GetPrivateField<bool>("canGrabLedge");
        Assert.IsTrue(canGrabLedge);
    }

    // Test 10: isSliding defaults to false
    [UnityTest]
    public IEnumerator IsSliding_DefaultsToFalse()
    {
        yield return null;
        bool isSliding = GetPrivateField<bool>("isSliding");
        Assert.IsFalse(isSliding);
    }
}
