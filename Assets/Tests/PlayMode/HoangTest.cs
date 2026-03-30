using System.Collections;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

[TestFixture]
public class HoangTest
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

    // Test 1: CheckForSlideCancel ends slide when timer expires
    [UnityTest]
    public IEnumerator CheckForSlideCancel_EndsSlide_WhenTimerExpired()
    {
        yield return null;
        SetPrivateField("isSliding", true);
        SetPrivateField("slideTimeCounter", -1f);
        SetPrivateField("ceillingDetected", false);

        InvokePrivateMethod("CheckForSlideCancel");

        bool isSliding = GetPrivateField<bool>("isSliding");
        Assert.IsFalse(isSliding);
    }

    // Test 2: CheckForSlideCancel keeps sliding when ceiling detected
    [UnityTest]
    public IEnumerator CheckForSlideCancel_KeepsSliding_WhenCeilingDetected()
    {
        yield return null;
        SetPrivateField("isSliding", true);
        SetPrivateField("slideTimeCounter", -1f);
        SetPrivateField("ceillingDetected", true);

        InvokePrivateMethod("CheckForSlideCancel");

        bool isSliding = GetPrivateField<bool>("isSliding");
        Assert.IsTrue(isSliding);
    }

    // Test 3: CheckForSlideCancel does nothing when timer active
    [UnityTest]
    public IEnumerator CheckForSlideCancel_DoesNothing_WhenTimerStillActive()
    {
        yield return null;
        SetPrivateField("isSliding", true);
        SetPrivateField("slideTimeCounter", 1f);
        SetPrivateField("ceillingDetected", false);

        InvokePrivateMethod("CheckForSlideCancel");

        bool isSliding = GetPrivateField<bool>("isSliding");
        Assert.IsTrue(isSliding);
    }

    // Test 4: SetupMovement resets speed when wall detected
    [UnityTest]
    public IEnumerator SetupMovement_ResetsSpeed_WhenWallDetected()
    {
        yield return null;
        SetPrivateField("wallDetected", true);
        SetPrivateField("moveSpeed", 25f);
        SetPrivateField("defaultSpeed", 8f);
        SetPrivateField("defaultMilestoneIncrease", 5f);
        SetPrivateField("isSliding", false);

        InvokePrivateMethod("SetupMovement");

        float currentSpeed = GetPrivateField<float>("moveSpeed");
        Assert.AreEqual(8f, currentSpeed);
    }

    // Test 5: SetupMovement uses moveSpeed when not sliding
    [UnityTest]
    public IEnumerator SetupMovement_UsesMoveSpeed_WhenNotSliding()
    {
        yield return null;
        SetPrivateField("wallDetected", false);
        SetPrivateField("isSliding", false);
        SetPrivateField("moveSpeed", 12f);

        InvokePrivateMethod("SetupMovement");

        Assert.AreEqual(12f, rb.linearVelocity.x);
    }

    // Test 6: SetupMovement uses slideSpeed when sliding
    [UnityTest]
    public IEnumerator SetupMovement_UsesSlideSpeed_WhenSliding()
    {
        yield return null;
        SetPrivateField("wallDetected", false);
        SetPrivateField("isSliding", true);
        SetPrivateField("slideSpeed", 20f);

        InvokePrivateMethod("SetupMovement");

        Assert.AreEqual(20f, rb.linearVelocity.x);
    }

    // Test 7: SetupMovement preserves Y velocity when not sliding
    [UnityTest]
    public IEnumerator SetupMovement_PreservesYVelocity_WhenNotSliding()
    {
        yield return null;
        SetPrivateField("wallDetected", false);
        SetPrivateField("isSliding", false);
        SetPrivateField("moveSpeed", 12f);
        rb.linearVelocity = new Vector2(0f, -5f);

        InvokePrivateMethod("SetupMovement");

        Assert.AreEqual(-5f, rb.linearVelocity.y);
    }

    // Test 8: SetupMovement preserves Y velocity when sliding
    [UnityTest]
    public IEnumerator SetupMovement_PreservesYVelocity_WhenSliding()
    {
        yield return null;
        SetPrivateField("wallDetected", false);
        SetPrivateField("isSliding", true);
        SetPrivateField("slideSpeed", 20f);
        rb.linearVelocity = new Vector2(0f, -3f);

        InvokePrivateMethod("SetupMovement");

        Assert.AreEqual(-3f, rb.linearVelocity.y);
    }

    // Test 9: SetupMovement does not set velocity when wall detected
    [UnityTest]
    public IEnumerator SetupMovement_DoesNotSetVelocity_WhenWallDetected()
    {
        yield return null;
        SetPrivateField("wallDetected", true);
        SetPrivateField("defaultSpeed", 8f);
        SetPrivateField("defaultMilestoneIncrease", 5f);
        SetPrivateField("isSliding", false);
        rb.linearVelocity = new Vector2(0f, -3f);

        InvokePrivateMethod("SetupMovement");

        Assert.AreEqual(0f, rb.linearVelocity.x, 0.01f);
    }

    // Test 10: ledgeDetected defaults to false
    [UnityTest]
    public IEnumerator LedgeDetected_DefaultsToFalse()
    {
        yield return null;
        Assert.IsFalse(player.ledgeDetected);
    }
}
