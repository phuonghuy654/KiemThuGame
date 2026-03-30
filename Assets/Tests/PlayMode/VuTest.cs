using System.Collections;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

[TestFixture]
public class VuTest
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

    // Test 1: CheckForLanding sets readyToLand when falling fast
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

    // Test 2: CheckForLanding does not set readyToLand when grounded
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

    // Test 3: CheckForLanding does not trigger when falling slowly
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

    // Test 4: CheckForLanding resets readyToLand when landing
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

    // Test 5: LedgeClimbOver resets canClimb
    [UnityTest]
    public IEnumerator LedgeClimbOver_SetsCanClimbToFalse()
    {
        yield return null;
        SetPrivateField("canClimb", true);

        InvokePrivateMethod("LedgeClimbOver");

        bool canClimb = GetPrivateField<bool>("canClimb");
        Assert.IsFalse(canClimb);
    }

    // Test 6: LedgeClimbOver restores gravity
    [UnityTest]
    public IEnumerator LedgeClimbOver_RestoresGravity()
    {
        yield return null;
        rb.gravityScale = 0;
        SetPrivateField("canClimb", true);

        InvokePrivateMethod("LedgeClimbOver");

        Assert.AreEqual(5f, rb.gravityScale);
    }

    // Test 7: LedgeClimbOver moves player to climbOverPosition
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

    // Test 8: readyToLand defaults to false
    [UnityTest]
    public IEnumerator ReadyToLand_DefaultsToFalse()
    {
        yield return null;
        bool readyToLand = GetPrivateField<bool>("readyToLand");
        Assert.IsFalse(readyToLand);
    }

    // Test 9: canDoubleJump defaults to false
    [UnityTest]
    public IEnumerator CanDoubleJump_DefaultsToFalse()
    {
        yield return null;
        bool canDoubleJump = GetPrivateField<bool>("canDoubleJump");
        Assert.IsFalse(canDoubleJump);
    }

    // Test 10: canClimb defaults to false
    [UnityTest]
    public IEnumerator CanClimb_DefaultsToFalse()
    {
        yield return null;
        bool canClimb = GetPrivateField<bool>("canClimb");
        Assert.IsFalse(canClimb);
    }
}
