using System.Collections;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

[TestFixture]
public class PlayerEditModeTests
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

    // Test 1: Player starts not unlocked
    [Test]
    public void PlayerUnlocked_DefaultsToFalse()
    {
        Assert.IsFalse(player.playerUnlocked);
    }

    // Test 2: ExtraLife is true when moveSpeed >= speedToSurvive
    [Test]
    public void ExtraLife_IsTrue_WhenMoveSpeedExceedsThreshold()
    {
        SetPrivateField("moveSpeed", 20f);
        SetPrivateField("speedToSurvive", 18f);

        player.extraLife = GetPrivateField<float>("moveSpeed") >= GetPrivateField<float>("speedToSurvive");

        Assert.IsTrue(player.extraLife);
    }

    // Test 3: ExtraLife is false when moveSpeed < speedToSurvive
    [Test]
    public void ExtraLife_IsFalse_WhenMoveSpeedBelowThreshold()
    {
        SetPrivateField("moveSpeed", 10f);
        SetPrivateField("speedToSurvive", 18f);

        player.extraLife = GetPrivateField<float>("moveSpeed") >= GetPrivateField<float>("speedToSurvive");

        Assert.IsFalse(player.extraLife);
    }

    // Test 4: SpeedReset restores default speed
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

    // Test 5: SpeedReset does nothing while sliding
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

    // Test 6: JumpButton does nothing when player is sliding
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

    // Test 7: SpeedController caps speed at maxSpeed
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

    // Test 8: SpeedController skips when already at maxSpeed
    [Test]
    public void SpeedController_DoesNothing_WhenAlreadyAtMaxSpeed()
    {
        SetPrivateField("moveSpeed", 50f);
        SetPrivateField("maxSpeed", 50f);

        InvokePrivateMethod("SpeedController");

        float resultSpeed = GetPrivateField<float>("moveSpeed");
        Assert.AreEqual(50f, resultSpeed);
    }

    // Test 9: CheckForSlideCancel ends slide when timer expires
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

    // Test 10: CheckForSlideCancel keeps sliding when ceiling detected
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
    // Test 11: JumpButton does nothing when player is dead
    [Test]
    public void JumpButton_DoesNothing_WhenPlayerIsDead()
    {
        SetPrivateField("isDead", true);
        SetPrivateField("isSliding", false);
        rb.linearVelocity = new Vector2(5f, 0f);
        float velocityYBefore = rb.linearVelocity.y;

        player.JumpButton();

        Assert.AreEqual(velocityYBefore, rb.linearVelocity.y);
    }

    // Test 12: SlideButton does nothing when player is dead
    [Test]
    public void SlideButton_DoesNothing_WhenPlayerIsDead()
    {
        SetPrivateField("isDead", true);
        bool slidingBefore = GetPrivateField<bool>("isSliding");

        player.SlideButton();

        bool slidingAfter = GetPrivateField<bool>("isSliding");
        Assert.AreEqual(slidingBefore, slidingAfter);
    }

    // Test 13: ExtraLife is true at exact threshold
    [Test]
    public void ExtraLife_IsTrue_AtExactThreshold()
    {
        SetPrivateField("moveSpeed", 18f);
        SetPrivateField("speedToSurvive", 18f);

        player.extraLife = GetPrivateField<float>("moveSpeed") >= GetPrivateField<float>("speedToSurvive");

        Assert.IsTrue(player.extraLife);
    }
    // Test 14: ExtraLife defaults to false
    [Test]
    public void ExtraLife_DefaultsToFalse()
    {
        Assert.IsFalse(player.extraLife);
    }

    // Test 15: isDead defaults to false
    [Test]
    public void IsDead_DefaultsToFalse()
    {
        bool isDead = GetPrivateField<bool>("isDead");
        Assert.IsFalse(isDead);
    }

    // Test 16: isKnocked defaults to false
    [Test]
    public void IsKnocked_DefaultsToFalse()
    {
        bool isKnocked = GetPrivateField<bool>("isKnocked");
        Assert.IsFalse(isKnocked);
    }

    // Test 17: canBeKnocked defaults to true
    [Test]
    public void CanBeKnocked_DefaultsToTrue()
    {
        bool canBeKnocked = GetPrivateField<bool>("canBeKnocked");
        Assert.IsTrue(canBeKnocked);
    }

    // Test 18: canGrabLedge defaults to true
    [Test]
    public void CanGrabLedge_DefaultsToTrue()
    {
        bool canGrabLedge = GetPrivateField<bool>("canGrabLedge");
        Assert.IsTrue(canGrabLedge);
    }

    // Test 19: isSliding defaults to false
    [Test]
    public void IsSliding_DefaultsToFalse()
    {
        bool isSliding = GetPrivateField<bool>("isSliding");
        Assert.IsFalse(isSliding);
    }

    // Test 20: ledgeDetected defaults to false
    [Test]
    public void LedgeDetected_DefaultsToFalse()
    {
        Assert.IsFalse(player.ledgeDetected);
    }

    // Test 21: readyToLand defaults to false
    [Test]
    public void ReadyToLand_DefaultsToFalse()
    {
        bool readyToLand = GetPrivateField<bool>("readyToLand");
        Assert.IsFalse(readyToLand);
    }

    // Test 22: canDoubleJump defaults to false
    [Test]
    public void CanDoubleJump_DefaultsToFalse()
    {
        bool canDoubleJump = GetPrivateField<bool>("canDoubleJump");
        Assert.IsFalse(canDoubleJump);
    }

    // Test 23: canClimb defaults to false
    [Test]
    public void CanClimb_DefaultsToFalse()
    {
        bool canClimb = GetPrivateField<bool>("canClimb");
        Assert.IsFalse(canClimb);
    }

    // Test 24: SpeedReset also restores milestoneIncreaser
    [Test]
    public void SpeedReset_RestoresMilestoneIncreaser()
    {
        SetPrivateField("defaultMilestoneIncrease", 5f);
        SetPrivateField("milestoneIncreaser", 20f);
        SetPrivateField("isSliding", false);
        SetPrivateField("defaultSpeed", 8f);

        InvokePrivateMethod("SpeedReset");

        float milestone = GetPrivateField<float>("milestoneIncreaser");
        Assert.AreEqual(5f, milestone);
    }

    // Test 25: SpeedController updates speedMilestone after passing
    [Test]
    public void SpeedController_UpdatesSpeedMilestone_WhenPassed()
    {
        SetPrivateField("moveSpeed", 10f);
        SetPrivateField("maxSpeed", 50f);
        SetPrivateField("speedMilestone", 0f);
        SetPrivateField("milestoneIncreaser", 5f);
        SetPrivateField("speedMultiplier", 1.2f);
        playerObject.transform.position = new Vector3(1f, 0, 0);

        InvokePrivateMethod("SpeedController");

        float newMilestone = GetPrivateField<float>("speedMilestone");
        Assert.Greater(newMilestone, 0f);
    }

    // Test 26: SpeedController does not change speed before milestone
    [Test]
    public void SpeedController_DoesNothing_WhenBeforeMilestone()
    {
        SetPrivateField("moveSpeed", 10f);
        SetPrivateField("maxSpeed", 50f);
        SetPrivateField("speedMilestone", 100f);
        playerObject.transform.position = new Vector3(1f, 0, 0);

        InvokePrivateMethod("SpeedController");

        float resultSpeed = GetPrivateField<float>("moveSpeed");
        Assert.AreEqual(10f, resultSpeed);
    }

    // Test 27: SpeedController multiplies milestoneIncreaser
    [Test]
    public void SpeedController_MultipliesMilestoneIncreaser()
    {
        SetPrivateField("moveSpeed", 10f);
        SetPrivateField("maxSpeed", 50f);
        SetPrivateField("speedMilestone", 0f);
        SetPrivateField("milestoneIncreaser", 5f);
        SetPrivateField("speedMultiplier", 1.2f);
        playerObject.transform.position = new Vector3(1f, 0, 0);

        InvokePrivateMethod("SpeedController");

        float newIncreaser = GetPrivateField<float>("milestoneIncreaser");
        Assert.AreEqual(5f * 1.2f, newIncreaser, 0.01f);
    }
    // Test 28: SpeedController applies exact multiplier
    [Test]
    public void SpeedController_AppliesExactMultiplier()
    {
        SetPrivateField("moveSpeed", 10f);
        SetPrivateField("maxSpeed", 50f);
        SetPrivateField("speedMilestone", 0f);
        SetPrivateField("milestoneIncreaser", 5f);
        SetPrivateField("speedMultiplier", 1.5f);
        playerObject.transform.position = new Vector3(1f, 0, 0);

        InvokePrivateMethod("SpeedController");

        float resultSpeed = GetPrivateField<float>("moveSpeed");
        Assert.AreEqual(15f, resultSpeed, 0.01f);
    }

    // Test 29: Knockback does nothing when canBeKnocked is false
    [Test]
    public void Knockback_DoesNothing_WhenCannotBeKnocked()
    {
        SetPrivateField("canBeKnocked", false);
        SetPrivateField("moveSpeed", 20f);
        SetPrivateField("defaultSpeed", 8f);

        InvokePrivateMethod("Knockback");

        float currentSpeed = GetPrivateField<float>("moveSpeed");
        Assert.AreEqual(20f, currentSpeed);
    }

    // Test 30: CancelKnockback sets isKnocked to false
    [Test]
    public void CancelKnockback_SetsIsKnockedToFalse()
    {
        SetPrivateField("isKnocked", true);

        InvokePrivateMethod("CancelKnockback");

        bool isKnocked = GetPrivateField<bool>("isKnocked");
        Assert.IsFalse(isKnocked);
    }

    // Test 31: CheckForSlideCancel does nothing when timer active
    [Test]
    public void CheckForSlideCancel_DoesNothing_WhenTimerStillActive()
    {
        SetPrivateField("isSliding", true);
        SetPrivateField("slideTimeCounter", 1f);
        SetPrivateField("ceillingDetected", false);

        InvokePrivateMethod("CheckForSlideCancel");

        bool isSliding = GetPrivateField<bool>("isSliding");
        Assert.IsTrue(isSliding);
    }

    // Test 32: AllowLedgeGrab sets canGrabLedge to true
    [Test]
    public void AllowLedgeGrab_SetsCanGrabLedgeToTrue()
    {
        SetPrivateField("canGrabLedge", false);

        InvokePrivateMethod("AllowLedgeGrab");

        bool canGrabLedge = GetPrivateField<bool>("canGrabLedge");
        Assert.IsTrue(canGrabLedge);
    }
}
