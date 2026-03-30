using System.Collections;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

[TestFixture]
public class PlayerPlayModeTests
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

    // Test 33: SlideButton does nothing when cooldown is active
    [UnityTest]
    public IEnumerator SlideButton_DoesNothing_WhenCooldownActive()
    {
        yield return null;
        SetPrivateField("isDead", false);
        player.slideCooldownCounter = 2f;
        rb.linearVelocity = new Vector2(5f, 0f);

        player.SlideButton();

        bool isSliding = GetPrivateField<bool>("isSliding");
        Assert.IsFalse(isSliding);
    }

    // Test 34: SlideButton does nothing when velocity is zero
    [UnityTest]
    public IEnumerator SlideButton_DoesNothing_WhenNotMoving()
    {
        yield return null;
        SetPrivateField("isDead", false);
        player.slideCooldownCounter = -1f;
        rb.linearVelocity = new Vector2(0f, 0f);

        player.SlideButton();

        bool isSliding = GetPrivateField<bool>("isSliding");
        Assert.IsFalse(isSliding);
    }

    // Test 35: SlideButton activates slide with valid conditions
    [UnityTest]
    public IEnumerator SlideButton_ActivatesSlide_WhenConditionsMet()
    {
        yield return null;
        SetPrivateField("isDead", false);
        SetPrivateField("slideTime", 1f);
        SetPrivateField("slideCooldown", 2f);
        player.slideCooldownCounter = -1f;
        rb.linearVelocity = new Vector2(5f, 0f);

        player.SlideButton();

        bool isSliding = GetPrivateField<bool>("isSliding");
        Assert.IsTrue(isSliding);
    }

    // Test 36: SlideButton sets slideTimeCounter
    [UnityTest]
    public IEnumerator SlideButton_SetsSlideTimeCounter()
    {
        yield return null;
        SetPrivateField("isDead", false);
        SetPrivateField("slideTime", 1.5f);
        SetPrivateField("slideCooldown", 2f);
        player.slideCooldownCounter = -1f;
        rb.linearVelocity = new Vector2(5f, 0f);

        player.SlideButton();

        float slideTimeCounter = GetPrivateField<float>("slideTimeCounter");
        Assert.AreEqual(1.5f, slideTimeCounter);
    }

    // Test 37: SlideButton sets slideCooldownCounter
    [UnityTest]
    public IEnumerator SlideButton_SetsSlideCooldownCounter()
    {
        yield return null;
        SetPrivateField("isDead", false);
        SetPrivateField("slideTime", 1f);
        SetPrivateField("slideCooldown", 3f);
        player.slideCooldownCounter = -1f;
        rb.linearVelocity = new Vector2(5f, 0f);

        player.SlideButton();

        Assert.AreEqual(3f, player.slideCooldownCounter);
    }

    // Test 38: SetupMovement resets speed when wall detected
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

    // Test 39: SetupMovement uses moveSpeed when not sliding
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

    // Test 40: SetupMovement uses slideSpeed when sliding
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

    // Test 41: SetupMovement preserves Y velocity when not sliding
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

    // Test 42: SetupMovement preserves Y velocity when sliding
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

    // Test 43: SetupMovement does not set velocity when wall detected
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

    // Test 44: CheckForLanding sets readyToLand when falling fast
    [UnityTest]
    public IEnumerator CheckForLanding_SetsReadyToLand_WhenFallingFast()
    {
        yield return null;
        rb.linearVelocity = new Vector2(0f, -10f);
        SetPrivateField("isGrounded", false);
        SetPrivateField("readyToLand", false);

        InvokePrivateMethod("CheckForLanding");

        bool readyToLand = GetPrivateField<bool>("readyToLand");
        Assert.IsTrue(readyToLand);
    }

    // Test 45: CheckForLanding does not set readyToLand when grounded
    [UnityTest]
    public IEnumerator CheckForLanding_DoesNotSetReadyToLand_WhenGrounded()
    {
        yield return null;
        rb.linearVelocity = new Vector2(0f, -10f);
        SetPrivateField("isGrounded", true);
        SetPrivateField("readyToLand", false);

        InvokePrivateMethod("CheckForLanding");

        bool readyToLand = GetPrivateField<bool>("readyToLand");
        Assert.IsFalse(readyToLand);
    }

    // Test 46: CheckForLanding does not trigger when falling slowly
    [UnityTest]
    public IEnumerator CheckForLanding_DoesNotTrigger_WhenFallingSlowly()
    {
        yield return null;
        rb.linearVelocity = new Vector2(0f, -2f);
        SetPrivateField("isGrounded", false);
        SetPrivateField("readyToLand", false);

        InvokePrivateMethod("CheckForLanding");

        bool readyToLand = GetPrivateField<bool>("readyToLand");
        Assert.IsFalse(readyToLand);
    }

    // Test 47: LedgeClimbOver resets canClimb
    [UnityTest]
    public IEnumerator LedgeClimbOver_SetsCanClimbToFalse()
    {
        yield return null;
        SetPrivateField("canClimb", true);

        InvokePrivateMethod("LedgeClimbOver");

        bool canClimb = GetPrivateField<bool>("canClimb");
        Assert.IsFalse(canClimb);
    }

    // Test 48: LedgeClimbOver restores gravity
    [UnityTest]
    public IEnumerator LedgeClimbOver_RestoresGravity()
    {
        yield return null;
        rb.gravityScale = 0;
        SetPrivateField("canClimb", true);

        InvokePrivateMethod("LedgeClimbOver");

        Assert.AreEqual(5f, rb.gravityScale);
    }

    // Test 49: LedgeClimbOver moves player to climbOverPosition
    [UnityTest]
    public IEnumerator LedgeClimbOver_MovesPlayerToClimbOverPosition()
    {
        yield return null;
        Vector2 climbOverPos = new Vector2(10f, 5f);
        SetPrivateField("climbOverPosition", climbOverPos);
        SetPrivateField("canClimb", true);

        InvokePrivateMethod("LedgeClimbOver");

        Assert.AreEqual(climbOverPos.x, playerObject.transform.position.x, 0.01f);
        Assert.AreEqual(climbOverPos.y, playerObject.transform.position.y, 0.01f);
    }

    // Test 50: CheckForLanding resets readyToLand when landing
    [UnityTest]
    public IEnumerator CheckForLanding_ResetsReadyToLand_WhenLanding()
    {
        yield return null;
        SetPrivateField("readyToLand", true);
        SetPrivateField("isGrounded", true);
        rb.linearVelocity = new Vector2(0f, 0f);

        InvokePrivateMethod("CheckForLanding");

        bool readyToLand = GetPrivateField<bool>("readyToLand");
        Assert.IsFalse(readyToLand);
    }
}
