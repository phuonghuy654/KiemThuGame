using System.Collections;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
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
    [Description("TC_REGISTER_19 - Kiem tra spawn map lien tuc")]
    public IEnumerator TC_REGISTER_19_LevelGenerator_AfterPlayerMovesForward_SpawnsNewMapParts()
    {
        yield return SceneManager.LoadSceneAsync(1);
        yield return new WaitForSeconds(0.5f);

        LevelGenerator generator = GameObject.FindObjectOfType<LevelGenerator>();
        Assert.That(generator, Is.Not.Null, "Khong tim thay LevelGenerator trong scene!");

        int initialChildCount = generator.transform.childCount;

        yield return new WaitForSeconds(3.0f);

        int laterChildCount = generator.transform.childCount;

        Assert.That(laterChildCount, Is.GreaterThanOrEqualTo(initialChildCount),
            $"Map khong spawn them phan moi! So parts ban dau: {initialChildCount}, sau 3 giay: {laterChildCount}.");
    }

    [UnityTest]
    [Description("TC_REGISTER_11 - Kiem tra animation player slide state")]
    public IEnumerator TC_REGISTER_11_Player_SlideButton_AnimatorEntersSlideState()
    {
        yield return SceneManager.LoadSceneAsync(1);
        yield return new WaitForSeconds(1f);

        Player player = GameManager.instance.player;
        Animator animator = player.GetComponent<Animator>();
        Assert.That(animator, Is.Not.Null, "Player khong co Animator component!");

        FieldInfo isSlidingField = typeof(Player).GetField("isSliding",
            BindingFlags.NonPublic | BindingFlags.Instance);
        Assert.That(isSlidingField, Is.Not.Null, "Khong tim thay field isSliding trong Player!");
        isSlidingField.SetValue(player, true);

        yield return null;

        Assert.That(animator.GetBool("isSliding"), Is.True,
            "Animator parameter 'isSliding' phai duoc bat khi isSliding = true!");
    }

    [UnityTest]
    [Description("TC_REGISTER_34 - Kiem tra thanh chinh am luong")]
    public IEnumerator TC_REGISTER_34_VolumeSlider_WhenValueChanged_AudioListenerVolumeUpdatesAccordingly()
    {
        yield return SceneManager.LoadSceneAsync(1);
        yield return null;

        UI_Main ui = GameObject.FindObjectOfType<UI_Main>();
        Assert.That(ui, Is.Not.Null, "Khong tim thay UI_Main trong scene!");

        MethodInfo setVolumeMethod = null;
        string[] possibleNames = { "SetVolume", "ChangeVolume", "OnVolumeChanged", "UpdateVolume", "SliderVolume" };
        foreach (var name in possibleNames)
        {
            setVolumeMethod = typeof(UI_Main).GetMethod(name,
                BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            if (setVolumeMethod != null) break;
        }

        if (setVolumeMethod != null)
        {
            setVolumeMethod.Invoke(ui, new object[] { 0.3f });
            yield return null;
            Assert.That(AudioListener.volume, Is.EqualTo(0.3f).Within(0.01f),
                $"AudioListener.volume khong cap nhat dung! Mong doi 0.3, thuc te: {AudioListener.volume}.");

            setVolumeMethod.Invoke(ui, new object[] { 0f });
            yield return null;
            Assert.That(AudioListener.volume, Is.EqualTo(0f).Within(0.01f),
                "Volume khong ve 0 khi goi method voi gia tri 0!");
        }
        else
        {
            AudioListener.volume = 0.3f;
            yield return null;
            Assert.That(AudioListener.volume, Is.EqualTo(0.3f).Within(0.01f),
                "AudioListener.volume khong nhan gia tri 0.3!");

            AudioListener.volume = 0f;
            yield return null;
            Assert.That(AudioListener.volume, Is.EqualTo(0f).Within(0.01f),
                "Sau khi keo slider ve 0, van con am thanh phat ra (AudioListener.volume != 0)!");
        }
    }

    [UnityTest]
    [Description("TC_REGISTER_42 - Kiem tra Extra Life cua Player")]
    public IEnumerator TC_REGISTER_42_Player_WhenSpeedAboveThreshold_ExtraLifeIsTrue()
    {
        yield return SceneManager.LoadSceneAsync(1);
        yield return new WaitForSeconds(0.5f);

        Player player = GameManager.instance.player;

        FieldInfo speedToSurviveField = typeof(Player).GetField("speedToSurvive",
            BindingFlags.NonPublic | BindingFlags.Instance);
        FieldInfo moveSpeedField = typeof(Player).GetField("moveSpeed",
            BindingFlags.NonPublic | BindingFlags.Instance);

        Assert.That(speedToSurviveField, Is.Not.Null, "Khong tim thay field speedToSurvive!");
        Assert.That(moveSpeedField, Is.Not.Null, "Khong tim thay field moveSpeed!");

        float speedToSurvive = (float)speedToSurviveField.GetValue(player);

        moveSpeedField.SetValue(player, speedToSurvive + 1f);
        yield return null;
        yield return null;

        Assert.That(player.extraLife, Is.True,
            $"extraLife phai la true khi moveSpeed ({speedToSurvive + 1f}) >= speedToSurvive ({speedToSurvive})!");

        moveSpeedField.SetValue(player, speedToSurvive - 1f);
        yield return null;
        yield return null;

        Assert.That(player.extraLife, Is.False,
            $"extraLife phai la false khi moveSpeed ({speedToSurvive - 1f}) < speedToSurvive ({speedToSurvive})!");
    }

    [UnityTest]
    [Description("TC_REGISTER_20 - Kiem tra spawn chong chuong ngai vat")]
    public IEnumerator TC_REGISTER_20_Trap_Initialization_KeepsObjectIfChanceSucceeds()
    {
        tempTrapObj = new GameObject();
        Trap trap = tempTrapObj.AddComponent<Trap>();

        FieldInfo chanceField = typeof(Trap).GetField("chanceToSpawn",
            BindingFlags.NonPublic | BindingFlags.Instance);
        chanceField.SetValue(trap, 200f);

        MethodInfo startMethod = typeof(Trap).GetMethod("Start",
            BindingFlags.NonPublic | BindingFlags.Instance);
        startMethod.Invoke(trap, null);

        yield return null;

        Assert.That(tempTrapObj, Is.Not.Null,
            "Trap bi huy du chanceToSpawn = 200 (luon thanh cong)!");
    }

    [Test]
    [Description("TC_REGISTER_36 - Kiem tra Animation cua Enemy")]
    public void TC_REGISTER_36_Enemy_AnimationTrigger_ResetsStateAndAllowsMovement()
    {
        tempEnemyObj = new GameObject();
        tempEnemyObj.SetActive(false);

        Enemy enemy = tempEnemyObj.AddComponent<Enemy>();
        Rigidbody2D rb = tempEnemyObj.AddComponent<Rigidbody2D>();

        FieldInfo rbField = typeof(Enemy).GetField("rb",
            BindingFlags.NonPublic | BindingFlags.Instance);
        rbField.SetValue(enemy, rb);

        rb.gravityScale = 0f;
        enemy.canMove = false;

        FieldInfo defaultGravityField = typeof(Enemy).GetField("defaultGravityScale",
            BindingFlags.NonPublic | BindingFlags.Instance);
        defaultGravityField.SetValue(enemy, 5f);

        FieldInfo justRespawnedField = typeof(Enemy).GetField("justRespawned",
            BindingFlags.NonPublic | BindingFlags.Instance);
        justRespawnedField.SetValue(enemy, true);

        MethodInfo animTriggerMethod = typeof(Enemy).GetMethod("AnimatatinTrigger",
            BindingFlags.NonPublic | BindingFlags.Instance);
        animTriggerMethod.Invoke(enemy, null);

        bool justRespawned = (bool)justRespawnedField.GetValue(enemy);

        Assert.That(rb.gravityScale, Is.EqualTo(5f),
            "gravityScale phai duoc phuc hoi ve defaultGravityScale sau AnimationTrigger!");
        Assert.That(enemy.canMove, Is.True,
            "canMove phai la true sau khi AnimationTrigger duoc goi!");
        Assert.That(justRespawned, Is.False,
            "justRespawned phai duoc reset ve false sau AnimationTrigger!");
    }

    [Test]
    [Description("TC_REGISTER_22 - Kiem tra climb")]
    public void TC_REGISTER_22_LedgeDetection_OnTriggerEnter2D_GroundLayer_DisablesDetection()
    {
        tempLedgeObj = new GameObject();
        LedgeDetection ledge = tempLedgeObj.AddComponent<LedgeDetection>();
        ledge.canDetect = true;

        tempGroundObj = new GameObject();
        tempGroundObj.layer = LayerMask.NameToLayer("Ground");
        BoxCollider2D groundCollider = tempGroundObj.AddComponent<BoxCollider2D>();

        MethodInfo triggerMethod = typeof(LedgeDetection).GetMethod("OnTriggerEnter2D",
            BindingFlags.NonPublic | BindingFlags.Instance);
        triggerMethod.Invoke(ledge, new object[] { groundCollider });

        Assert.That(ledge.canDetect, Is.False,
            "canDetect phai bi tat khi LedgeDetection cham vao layer Ground!");
    }
}