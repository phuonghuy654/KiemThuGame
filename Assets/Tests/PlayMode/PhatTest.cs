using System.Collections;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

[TestFixture]
public class PhatTest
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

    // Test 1: SpeedReset restores default speed
    [UnityTest]
    public IEnumerator SpeedReset_RestoresMoveSpeedToDefault()
    {
        yield return null;
        SetPrivateField("defaultSpeed", 8f);
        SetPrivateField("moveSpeed", 25f);
        SetPrivateField("defaultMilestoneIncrease", 5f);
        SetPrivateField("isSliding", false);

        InvokePrivateMethod("SpeedReset");

        float currentSpeed = GetPrivateField<float>("moveSpeed");
        Assert.AreEqual(8f, currentSpeed);
    }

    // Test 2: SpeedReset does nothing while sliding
    [UnityTest]
    public IEnumerator SpeedReset_DoesNothing_WhenPlayerIsSliding()
    {
        yield return null;
        SetPrivateField("defaultSpeed", 8f);
        SetPrivateField("moveSpeed", 25f);
        SetPrivateField("isSliding", true);

        InvokePrivateMethod("SpeedReset");

        float currentSpeed = GetPrivateField<float>("moveSpeed");
        Assert.AreEqual(25f, currentSpeed);
    }

    // Test 3: SpeedReset also restores milestoneIncreaser
    [UnityTest]
    public IEnumerator SpeedReset_RestoresMilestoneIncreaser()
    {
        yield return null;
        SetPrivateField("defaultMilestoneIncrease", 5f);
        SetPrivateField("milestoneIncreaser", 20f);
        SetPrivateField("isSliding", false);
        SetPrivateField("defaultSpeed", 8f);

        InvokePrivateMethod("SpeedReset");

        float milestone = GetPrivateField<float>("milestoneIncreaser");
        Assert.AreEqual(5f, milestone);
    }

    // Test 4: SpeedController caps speed at maxSpeed
    [UnityTest]
    public IEnumerator SpeedController_CapsSpeed_AtMaxSpeed()
    {
        yield return null;
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

    // Test 5: SpeedController skips when already at maxSpeed
    [UnityTest]
    public IEnumerator SpeedController_DoesNothing_WhenAlreadyAtMaxSpeed()
    {
        yield return null;
        SetPrivateField("moveSpeed", 50f);
        SetPrivateField("maxSpeed", 50f);

        InvokePrivateMethod("SpeedController");

        float resultSpeed = GetPrivateField<float>("moveSpeed");
        Assert.AreEqual(50f, resultSpeed);
    }

    // Test 6: SpeedController updates speedMilestone after passing
    [UnityTest]
    public IEnumerator SpeedController_UpdatesSpeedMilestone_WhenPassed()
    {
        yield return null;
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

    // Test 7: SpeedController does not change speed before milestone
    [UnityTest]
    public IEnumerator SpeedController_DoesNothing_WhenBeforeMilestone()
    {
        yield return null;
        SetPrivateField("moveSpeed", 10f);
        SetPrivateField("maxSpeed", 50f);
        SetPrivateField("speedMilestone", 100f);
        playerObject.transform.position = new Vector3(1f, 0, 0);

        InvokePrivateMethod("SpeedController");

        float resultSpeed = GetPrivateField<float>("moveSpeed");
        Assert.AreEqual(10f, resultSpeed);
    }

    // Test 8: SpeedController multiplies milestoneIncreaser
    [UnityTest]
    public IEnumerator SpeedController_MultipliesMilestoneIncreaser()
    {
        yield return null;
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

    // Test 9: SpeedController applies exact multiplier
    [UnityTest]
    public IEnumerator SpeedController_AppliesExactMultiplier()
    {
        yield return null;
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

    // Test 10: Knockback does nothing when canBeKnocked is false
    [UnityTest]
    public IEnumerator Knockback_DoesNothing_WhenCannotBeKnocked()
    {
        yield return null;
        SetPrivateField("canBeKnocked", false);
        SetPrivateField("moveSpeed", 20f);
        SetPrivateField("defaultSpeed", 8f);

        InvokePrivateMethod("Knockback");

        float currentSpeed = GetPrivateField<float>("moveSpeed");
        Assert.AreEqual(20f, currentSpeed);
    }
}
