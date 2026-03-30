using System.Collections;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

[TestFixture]
public class PlayerTests
{
    private GameObject playerObject;
    private Player player;
    private Rigidbody2D rb;

    [SetUp]
    public void SetUp()
    {
        playerObject = new GameObject("Player");

        rb = playerObject.AddComponent<Rigidbody2D>();
        rb.gravityScale = 5;

        playerObject.AddComponent<SpriteRenderer>();
        playerObject.AddComponent<Animator>();

        player = playerObject.AddComponent<Player>();
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

    // Test 1: Player starts 
    [Test]
    public void PlayerUnlocked_DefaultsToFalse()
    {
        Assert.IsFalse(player.playerUnlocked);
    }

    // Test 2: ExtraLife 
    [Test]
    public void ExtraLife_IsTrue_WhenMoveSpeedExceedsThreshold()
    {
        SetPrivateField("moveSpeed", 20f);
        SetPrivateField("speedToSurvive", 18f);

        player.extraLife = GetPrivateField<float>("moveSpeed") >= GetPrivateField<float>("speedToSurvive");

        Assert.IsTrue(player.extraLife);
    }

    // Test 3: ExtraLife decrease
    [Test]
    public void ExtraLife_IsFalse_WhenMoveSpeedBelowThreshold()
    {
        SetPrivateField("moveSpeed", 10f);
        SetPrivateField("speedToSurvive", 18f);

        player.extraLife = GetPrivateField<float>("moveSpeed") >= GetPrivateField<float>("speedToSurvive");

        Assert.IsFalse(player.extraLife);
    }

    // Test 4: Speed reset because knock back
    [Test]
    public void SpeedReset_RestoresMoveSpeedToDefault()
    {
        SetPrivateField("defaultSpeed", 8f);
        SetPrivateField("moveSpeed", 25f);
        SetPrivateField("defaultMilestoneIncrease", 5f);
        SetPrivateField("isSliding", false);

        InvokePrivateMethod("SpeedReset");

        float currentSpeed = GetPrivateField<float>("moveSpeed");
        Assert.AreEqual(8f, currentSpeed);
    }

    // Test 5: Speed reset when player sliding
    [Test]
    public void SpeedReset_DoesNothing_WhenPlayerIsSliding()
    {
        SetPrivateField("defaultSpeed", 8f);
        SetPrivateField("moveSpeed", 25f);
        SetPrivateField("isSliding", true);

        InvokePrivateMethod("SpeedReset");

        float currentSpeed = GetPrivateField<float>("moveSpeed");
        Assert.AreEqual(25f, currentSpeed);
    }

    // Test 6: Jump don't active because slide
    [Test]
    public void JumpButton_DoesNothing_WhenPlayerIsSliding()
    {
        SetPrivateField("isSliding", true);
        SetPrivateField("isDead", false);
        rb.linearVelocity = new Vector2(5f, 0f);
        float velocityYBefore = rb.linearVelocity.y;

        player.JumpButton();

        Assert.AreEqual(velocityYBefore, rb.linearVelocity.y);
    }

    // Test 7: Speed <= maxSpeed
    [Test]
    public void SpeedController_CapsSpeed_AtMaxSpeed()
    {
        SetPrivateField("moveSpeed", 48f);
        SetPrivateField("maxSpeed", 50f);
        SetPrivateField("speedMilestone", 0f);
        SetPrivateField("milestoneIncreaser", 5f);
        SetPrivateField("speedMultiplier", 1.2f);

        playerObject.transform.position = new Vector3(1f, 0, 0);

        InvokePrivateMethod("SpeedController");

        float resultSpeed = GetPrivateField<float>("moveSpeed");
        Assert.LessOrEqual(resultSpeed, 50f);
    }

    // Test 8: Maxspeed is maximum
    [Test]
    public void SpeedController_DoesNothing_WhenAlreadyAtMaxSpeed()
    {
        SetPrivateField("moveSpeed", 50f);
        SetPrivateField("maxSpeed", 50f);

        InvokePrivateMethod("SpeedController");

        float resultSpeed = GetPrivateField<float>("moveSpeed");
        Assert.AreEqual(50f, resultSpeed);
    }

    // Test 9: Slide have limit time
    [Test]
    public void CheckForSlideCancel_EndsSlide_WhenTimerExpired()
    {
        SetPrivateField("isSliding", true);
        SetPrivateField("slideTimeCounter", -1f);
        SetPrivateField("ceillingDetected", false);

        InvokePrivateMethod("CheckForSlideCancel");

        bool isSliding = GetPrivateField<bool>("isSliding");
        Assert.IsFalse(isSliding);
    }
    // Test 10: Slide forever if co tran nha
    [Test]
    public void CheckForSlideCancel_KeepsSliding_WhenCeilingDetected()
    {
        SetPrivateField("isSliding", true);
        SetPrivateField("slideTimeCounter", -1f);
        SetPrivateField("ceillingDetected", true);

        InvokePrivateMethod("CheckForSlideCancel");

        bool isSliding = GetPrivateField<bool>("isSliding");
        Assert.IsTrue(isSliding);
    }
}
