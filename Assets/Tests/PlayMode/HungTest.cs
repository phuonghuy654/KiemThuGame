using System.Collections;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

[TestFixture]
public class HungTest
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

    // Test 1: JumpButton does nothing when player is sliding
    [UnityTest]
    public IEnumerator JumpButton_DoesNothing_WhenPlayerIsSliding()
    {
        yield return null;
        SetPrivateField("isSliding", true);
        SetPrivateField("isDead", false);
        rb.linearVelocity = new Vector2(5f, 0f);
        float velocityYBefore = rb.linearVelocity.y;

        player.JumpButton();

        Assert.AreEqual(velocityYBefore, rb.linearVelocity.y);
    }

    // Test 2: JumpButton does nothing when player is dead
    [UnityTest]
    public IEnumerator JumpButton_DoesNothing_WhenPlayerIsDead()
    {
        yield return null;
        SetPrivateField("isDead", true);
        SetPrivateField("isSliding", false);
        rb.linearVelocity = new Vector2(5f, 0f);
        float velocityYBefore = rb.linearVelocity.y;

        player.JumpButton();

        Assert.AreEqual(velocityYBefore, rb.linearVelocity.y);
    }

    // Test 3: SlideButton does nothing when player is dead
    [UnityTest]
    public IEnumerator SlideButton_DoesNothing_WhenPlayerIsDead()
    {
        yield return null;
        SetPrivateField("isDead", true);
        bool slidingBefore = GetPrivateField<bool>("isSliding");

        player.SlideButton();

        bool slidingAfter = GetPrivateField<bool>("isSliding");
        Assert.AreEqual(slidingBefore, slidingAfter);
    }

    // Test 4: SlideButton does nothing when cooldown is active
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

    // Test 5: SlideButton does nothing when velocity is zero
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

    // Test 6: SlideButton activates slide with valid conditions
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

    // Test 7: SlideButton sets slideTimeCounter
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

    // Test 8: SlideButton sets slideCooldownCounter
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

    // Test 9: CancelKnockback sets isKnocked to false
    [UnityTest]
    public IEnumerator CancelKnockback_SetsIsKnockedToFalse()
    {
        yield return null;
        SetPrivateField("isKnocked", true);

        InvokePrivateMethod("CancelKnockback");

        bool isKnocked = GetPrivateField<bool>("isKnocked");
        Assert.IsFalse(isKnocked);
    }

    // Test 10: AllowLedgeGrab sets canGrabLedge to true
    [UnityTest]
    public IEnumerator AllowLedgeGrab_SetsCanGrabLedgeToTrue()
    {
        yield return null;
        SetPrivateField("canGrabLedge", false);

        InvokePrivateMethod("AllowLedgeGrab");

        bool canGrabLedge = GetPrivateField<bool>("canGrabLedge");
        Assert.IsTrue(canGrabLedge);
    }
}
