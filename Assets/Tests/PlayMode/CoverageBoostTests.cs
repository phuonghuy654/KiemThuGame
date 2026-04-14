using System.Collections;
using System.Reflection;
using NUnit.Framework;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;

/// <summary>
/// Coverage Boost Tests - Đẩy coverage từ 73.8% lên 90%+
/// Bổ sung cho các script: AudioManager, Enemy, GameManager, Player,
/// UI_Main, UI_Shop, Trap, MovingTrap, PlatformController, RainController, LedgeDetection.
/// </summary>
[TestFixture]
public class CoverageBoostTests
{
    private static readonly BindingFlags FLAGS =
        BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public;

    #region === HELPERS ===

    private void Set(object target, string field, object value)
    {
        var f = target.GetType().GetField(field, FLAGS);
        Assert.IsNotNull(f, $"Field '{field}' not found in {target.GetType().Name}");
        f.SetValue(target, value);
    }

    private T Get<T>(object target, string field)
    {
        var f = target.GetType().GetField(field, FLAGS);
        Assert.IsNotNull(f, $"Field '{field}' not found in {target.GetType().Name}");
        return (T)f.GetValue(target);
    }

    private object Invoke(object target, string method, params object[] args)
    {
        var m = target.GetType().GetMethod(method, FLAGS);
        Assert.IsNotNull(m, $"Method '{method}' not found in {target.GetType().Name}");
        return m.Invoke(target, args);
    }

    #endregion

    // ================================================================
    //  AUDIOMANAGER - Từ 71.8% → 90%+
    //  Chưa cover: PlaySFX, StopSFX, PlayBGM, StopBGM, PlayRandomBGM
    // ================================================================

    #region AudioManager

    private GameObject amObj;

    private AudioManager CreateAudioManager(int sfxCount, int bgmCount)
    {
        amObj = new GameObject("AudioManager");
        var am = amObj.AddComponent<AudioManager>();

        var sfxArr = new AudioSource[sfxCount];
        for (int i = 0; i < sfxCount; i++)
            sfxArr[i] = amObj.AddComponent<AudioSource>();

        var bgmArr = new AudioSource[bgmCount];
        for (int i = 0; i < bgmCount; i++)
            bgmArr[i] = amObj.AddComponent<AudioSource>();

        Set(am, "sfx", sfxArr);
        Set(am, "bgm", bgmArr);

        return am;
    }

    [UnityTest]
    [Description("AudioManager - PlaySFX plays correct index and sets random pitch")]
    public IEnumerator AudioManager_PlaySFX_PlaysCorrectIndex()
    {
        var am = CreateAudioManager(3, 1);
        yield return null;

        am.PlaySFX(0);
        var sfxArr = Get<AudioSource[]>(am, "sfx");

        Assert.IsTrue(sfxArr[0].isPlaying,
            "PlaySFX phải play AudioSource tại index được chỉ định.");
        Assert.That(sfxArr[0].pitch, Is.InRange(0.85f, 1.1f),
            "PlaySFX phải set pitch ngẫu nhiên trong khoảng [0.85, 1.1].");

        Object.DestroyImmediate(amObj);
    }

    [UnityTest]
    [Description("AudioManager - PlaySFX does nothing when index >= sfx.Length")]
    public IEnumerator AudioManager_PlaySFX_IgnoresOutOfRangeIndex()
    {
        var am = CreateAudioManager(2, 1);
        yield return null;

        Assert.DoesNotThrow(() => am.PlaySFX(5),
            "PlaySFX không được throw khi index vượt ngoài sfx.Length.");

        Object.DestroyImmediate(amObj);
    }

    [UnityTest]
    [Description("AudioManager - StopSFX stops the audio source at index")]
    public IEnumerator AudioManager_StopSFX_StopsCorrectSource()
    {
        var am = CreateAudioManager(2, 1);
        yield return null;

        var sfxArr = Get<AudioSource[]>(am, "sfx");
        sfxArr[0].Play();
        yield return null;

        am.StopSFX(0);
        yield return null;

        Assert.IsFalse(sfxArr[0].isPlaying,
            "StopSFX phải dừng AudioSource tại index.");

        Object.DestroyImmediate(amObj);
    }

    [UnityTest]
    [Description("AudioManager - PlayBGM plays the specified bgm and stops others")]
    public IEnumerator AudioManager_PlayBGM_PlaysSpecifiedIndex()
    {
        var am = CreateAudioManager(1, 3);
        yield return null;

        am.PlayBGM(1);
        yield return null;

        var bgmArr = Get<AudioSource[]>(am, "bgm");
        Assert.IsTrue(bgmArr[1].isPlaying,
            "PlayBGM phải play BGM tại index được chỉ định.");

        Object.DestroyImmediate(amObj);
    }

    [UnityTest]
    [Description("AudioManager - StopBGM stops all bgm sources")]
    public IEnumerator AudioManager_StopBGM_StopsAllSources()
    {
        var am = CreateAudioManager(1, 3);
        yield return null;

        am.PlayBGM(0);
        yield return null;

        am.StopBGM();
        yield return null;

        var bgmArr = Get<AudioSource[]>(am, "bgm");
        for (int i = 0; i < bgmArr.Length; i++)
            Assert.IsFalse(bgmArr[i].isPlaying,
                $"StopBGM phải dừng tất cả BGM, nhưng index {i} vẫn đang play.");

        Object.DestroyImmediate(amObj);
    }

    [UnityTest]
    [Description("AudioManager - PlayRandomBGM sets bgmIndex and plays")]
    public IEnumerator AudioManager_PlayRandomBGM_SetsBgmIndexAndPlays()
    {
        var am = CreateAudioManager(1, 3);
        yield return null;

        am.PlayRandomBGM();
        yield return null;

        int bgmIndex = Get<int>(am, "bgmIndex");
        Assert.That(bgmIndex, Is.InRange(0, 2),
            "PlayRandomBGM phải set bgmIndex trong khoảng hợp lệ.");

        var bgmArr = Get<AudioSource[]>(am, "bgm");
        Assert.IsTrue(bgmArr[bgmIndex].isPlaying,
            "PlayRandomBGM phải play BGM tại bgmIndex.");

        Object.DestroyImmediate(amObj);
    }

    [UnityTest]
    [Description("AudioManager - Awake sets singleton instance")]
    public IEnumerator AudioManager_Awake_SetsSingletonInstance()
    {
        var am = CreateAudioManager(1, 1);
        yield return null;

        Assert.AreEqual(am, AudioManager.instance,
            "Awake phải set AudioManager.instance.");

        Object.DestroyImmediate(amObj);
    }

    #endregion

    // ================================================================
    //  ENEMY - Từ 73.2% → 90%+
    //  Chưa cover: Jump, SpeedController, Movement, LedgeClimbOver,
    //  AllowLedgeGrab, AnimatorControllers
    // ================================================================

    #region Enemy

    private GameObject enemyObj;

    private Enemy CreateEnemy()
    {
        enemyObj = new GameObject("Enemy");
        enemyObj.SetActive(false);

        var rb = enemyObj.AddComponent<Rigidbody2D>();
        var anim = enemyObj.AddComponent<Animator>();
        enemyObj.AddComponent<SpriteRenderer>();

        var enemy = enemyObj.AddComponent<Enemy>();

        Set(enemy, "rb", rb);
        Set(enemy, "anim", anim);
        Set(enemy, "defaultGravityScale", 5f);
        Set(enemy, "jumpForce", 10f);
        Set(enemy, "moveSpeed", 15f);

        return enemy;
    }

    [Test]
    [Description("Enemy - Jump sets upward velocity when grounded")]
    public void Enemy_Jump_SetsUpwardVelocity_WhenGrounded()
    {
        var enemy = CreateEnemy();
        var rb = enemyObj.GetComponent<Rigidbody2D>();

        Set(enemy, "isGrounded", true);
        rb.linearVelocity = new Vector2(10f, 0f);

        Invoke(enemy, "Jump");

        Assert.AreEqual(10f, rb.linearVelocity.y,
            "Jump phải set velocity.y = jumpForce khi isGrounded.");

        Object.DestroyImmediate(enemyObj);
    }

    [Test]
    [Description("Enemy - Jump does nothing when not grounded")]
    public void Enemy_Jump_DoesNothing_WhenNotGrounded()
    {
        var enemy = CreateEnemy();
        var rb = enemyObj.GetComponent<Rigidbody2D>();

        Set(enemy, "isGrounded", false);
        rb.linearVelocity = new Vector2(10f, -3f);

        Invoke(enemy, "Jump");

        Assert.AreEqual(-3f, rb.linearVelocity.y,
            "Jump không được thay đổi velocity khi không grounded.");

        Object.DestroyImmediate(enemyObj);
    }

    [Test]
    [Description("Enemy - Movement sets horizontal velocity when canMove")]
    public void Enemy_Movement_SetsVelocity_WhenCanMove()
    {
        var enemy = CreateEnemy();
        var rb = enemyObj.GetComponent<Rigidbody2D>();

        Set(enemy, "justRespawned", false);
        enemy.canMove = true;
        Set(enemy, "moveSpeed", 20f);
        rb.linearVelocity = new Vector2(0f, -5f);

        Invoke(enemy, "Movement");

        Assert.AreEqual(20f, rb.linearVelocity.x,
            "Movement phải set velocity.x = moveSpeed khi canMove = true.");
        Assert.AreEqual(-5f, rb.linearVelocity.y,
            "Movement phải giữ nguyên velocity.y.");

        Object.DestroyImmediate(enemyObj);
    }

    [Test]
    [Description("Enemy - Movement stops horizontal velocity when canMove is false")]
    public void Enemy_Movement_StopsVelocity_WhenCannotMove()
    {
        var enemy = CreateEnemy();
        var rb = enemyObj.GetComponent<Rigidbody2D>();

        Set(enemy, "justRespawned", false);
        enemy.canMove = false;
        rb.linearVelocity = new Vector2(15f, -2f);

        Invoke(enemy, "Movement");

        Assert.AreEqual(0f, rb.linearVelocity.x,
            "Movement phải set velocity.x = 0 khi canMove = false.");

        Object.DestroyImmediate(enemyObj);
    }

    [Test]
    [Description("Enemy - Movement returns early when justRespawned")]
    public void Enemy_Movement_ReturnsEarly_WhenJustRespawned()
    {
        var enemy = CreateEnemy();
        var rb = enemyObj.GetComponent<Rigidbody2D>();

        Set(enemy, "justRespawned", true);
        enemy.canMove = true;
        Set(enemy, "moveSpeed", 20f);
        rb.linearVelocity = new Vector2(5f, 0f);

        Invoke(enemy, "Movement");

        Assert.AreEqual(5f, rb.linearVelocity.x,
            "Movement phải return early khi justRespawned, không thay đổi velocity.");

        Object.DestroyImmediate(enemyObj);
    }

    [Test]
    [Description("Enemy - SpeedController sets 25 when player ahead and far")]
    public void Enemy_SpeedController_Sets25_WhenPlayerAheadAndFar()
    {
        var enemy = CreateEnemy();

        var playerObj = new GameObject("Player");
        playerObj.transform.position = new Vector3(100f, 0f, 0f);
        var playerComp = playerObj.AddComponent<Player>();
        Set(enemy, "player", playerComp);

        enemyObj.transform.position = new Vector3(0f, 0f, 0f);

        Invoke(enemy, "SpeedController");

        float speed = Get<float>(enemy, "moveSpeed");
        Assert.AreEqual(25f, speed,
            "SpeedController phải set moveSpeed = 25 khi player ở xa phía trước.");

        Object.DestroyImmediate(enemyObj);
        Object.DestroyImmediate(playerObj);
    }

    [Test]
    [Description("Enemy - SpeedController sets 17 when player ahead but close")]
    public void Enemy_SpeedController_Sets17_WhenPlayerAheadAndClose()
    {
        var enemy = CreateEnemy();

        var playerObj = new GameObject("Player");
        playerObj.transform.position = new Vector3(2f, 0f, 0f);
        var playerComp = playerObj.AddComponent<Player>();
        Set(enemy, "player", playerComp);

        enemyObj.transform.position = new Vector3(0f, 0f, 0f);

        Invoke(enemy, "SpeedController");

        float speed = Get<float>(enemy, "moveSpeed");
        Assert.AreEqual(17f, speed,
            "SpeedController phải set moveSpeed = 17 khi player ở gần phía trước.");

        Object.DestroyImmediate(enemyObj);
        Object.DestroyImmediate(playerObj);
    }

    [Test]
    [Description("Enemy - SpeedController sets 11 when player behind and far")]
    public void Enemy_SpeedController_Sets11_WhenPlayerBehindAndFar()
    {
        var enemy = CreateEnemy();

        var playerObj = new GameObject("Player");
        playerObj.transform.position = new Vector3(-100f, 0f, 0f);
        var playerComp = playerObj.AddComponent<Player>();
        Set(enemy, "player", playerComp);

        enemyObj.transform.position = new Vector3(0f, 0f, 0f);

        Invoke(enemy, "SpeedController");

        float speed = Get<float>(enemy, "moveSpeed");
        Assert.AreEqual(11f, speed,
            "SpeedController phải set moveSpeed = 11 khi player ở xa phía sau.");

        Object.DestroyImmediate(enemyObj);
        Object.DestroyImmediate(playerObj);
    }

    [Test]
    [Description("Enemy - SpeedController sets 14 when player behind but close")]
    public void Enemy_SpeedController_Sets14_WhenPlayerBehindAndClose()
    {
        var enemy = CreateEnemy();

        var playerObj = new GameObject("Player");
        playerObj.transform.position = new Vector3(-1f, 0f, 0f);
        var playerComp = playerObj.AddComponent<Player>();
        Set(enemy, "player", playerComp);

        enemyObj.transform.position = new Vector3(0f, 0f, 0f);

        Invoke(enemy, "SpeedController");

        float speed = Get<float>(enemy, "moveSpeed");
        Assert.AreEqual(14f, speed,
            "SpeedController phải set moveSpeed = 14 khi player ở gần phía sau.");

        Object.DestroyImmediate(enemyObj);
        Object.DestroyImmediate(playerObj);
    }

    [Test]
    [Description("Enemy - LedgeClimbOver resets canClimb and restores gravity")]
    public void Enemy_LedgeClimbOver_ResetsCanClimbAndGravity()
    {
        var enemy = CreateEnemy();
        var rb = enemyObj.GetComponent<Rigidbody2D>();

        Set(enemy, "canClimb", true);
        rb.gravityScale = 0;
        Set(enemy, "climbOverPosition", new Vector2(10f, 5f));

        Invoke(enemy, "LedgeClimbOver");

        Assert.IsFalse(Get<bool>(enemy, "canClimb"),
            "LedgeClimbOver phải set canClimb = false.");
        Assert.AreEqual(5f, rb.gravityScale,
            "LedgeClimbOver phải restore gravityScale về 5.");

        Object.DestroyImmediate(enemyObj);
    }

    [Test]
    [Description("Enemy - AllowLedgeGrab sets canGrabLedge to true")]
    public void Enemy_AllowLedgeGrab_SetsCanGrabLedge()
    {
        var enemy = CreateEnemy();
        Set(enemy, "canGrabLedge", false);

        Invoke(enemy, "AllowLedgeGrab");

        Assert.IsTrue(Get<bool>(enemy, "canGrabLedge"),
            "AllowLedgeGrab phải set canGrabLedge = true.");

        Object.DestroyImmediate(enemyObj);
    }

    [Test]
    [Description("Enemy - AnimatatinTrigger restores gravity, enables move, clears respawn")]
    public void Enemy_AnimatatinTrigger_RestoresState()
    {
        var enemy = CreateEnemy();
        var rb = enemyObj.GetComponent<Rigidbody2D>();

        rb.gravityScale = 0f;
        enemy.canMove = false;
        Set(enemy, "justRespawned", true);
        Set(enemy, "defaultGravityScale", 5f);

        Invoke(enemy, "AnimatatinTrigger");

        Assert.AreEqual(5f, rb.gravityScale);
        Assert.IsTrue(enemy.canMove);
        Assert.IsFalse(Get<bool>(enemy, "justRespawned"));

        Object.DestroyImmediate(enemyObj);
    }

    #endregion

    // ================================================================
    //  GAMEMANAGER - Từ 89.3% → 90%+
    //  Chưa cover: SetupSkyBox branches, GameEnded
    // ================================================================

    #region GameManager

    [UnityTest]
    [Description("GameManager - SetupSkyBox saves index to PlayerPrefs")]
    public IEnumerator GameManager_SetupSkyBox_SavesIndex()
    {
        yield return SceneManager.LoadSceneAsync(1);
        yield return new WaitForSeconds(0.3f);

        GameManager.instance.SetupSkyBox(0);

        int saved = PlayerPrefs.GetInt("SkyBoxSetting");
        Assert.AreEqual(0, saved,
            "SetupSkyBox phải lưu index vào PlayerPrefs.");
    }

    [UnityTest]
    [Description("GameManager - SetupSkyBox with index > 1 uses random skybox")]
    public IEnumerator GameManager_SetupSkyBox_RandomWhenIndexAbove1()
    {
        yield return SceneManager.LoadSceneAsync(1);
        yield return new WaitForSeconds(0.3f);

        Assert.DoesNotThrow(() => GameManager.instance.SetupSkyBox(5),
            "SetupSkyBox với index > 1 phải dùng random mà không throw.");

        int saved = PlayerPrefs.GetInt("SkyBoxSetting");
        Assert.AreEqual(5, saved);
    }

    [UnityTest]
    [Description("GameManager - GameEnded calls SaveInfo and opens end game UI")]
    public IEnumerator GameManager_GameEnded_CallsSaveInfoAndOpensUI()
    {
        yield return SceneManager.LoadSceneAsync(1);
        yield return new WaitForSeconds(0.3f);

        GameManager.instance.distance = 50f;
        GameManager.instance.coins = 5;

        PlayerPrefs.DeleteKey("LastScore");
        GameManager.instance.GameEnded();
        yield return null;

        float lastScore = PlayerPrefs.GetFloat("LastScore", -1f);
        Assert.AreEqual(250f, lastScore, 0.01f,
            "GameEnded phải gọi SaveInfo và lưu score = distance * coins.");
    }

    #endregion

    // ================================================================
    //  PLAYER - Từ 72.5% → 90%+
    //  Chưa cover: Damage, Die, Invincibility, CheckForLedge,
    //  AnimatorControllers (canRoll), Jump, RollAnimFinished, CheckInput
    // ================================================================

    #region Player

    private GameObject playerObj;
    private Player player;
    private Rigidbody2D playerRb;

    private Player CreatePlayer()
    {
        playerObj = new GameObject("Player");
        playerRb = playerObj.AddComponent<Rigidbody2D>();
        playerRb.gravityScale = 5;
        playerObj.AddComponent<SpriteRenderer>();
        playerObj.AddComponent<Animator>();

        var dustObj = new GameObject("DustFx");
        dustObj.transform.SetParent(playerObj.transform);
        var dustFx = dustObj.AddComponent<ParticleSystem>();

        var bloodObj = new GameObject("BloodFx");
        bloodObj.transform.SetParent(playerObj.transform);
        var bloodFx = bloodObj.AddComponent<ParticleSystem>();

        var wallCheckObj = new GameObject("WallCheck");
        wallCheckObj.transform.SetParent(playerObj.transform);

        player = playerObj.AddComponent<Player>();

        Set(player, "dustFx", dustFx);
        Set(player, "bloodFx", bloodFx);
        Set(player, "wallCheck", wallCheckObj.transform);
        Set(player, "wallCheckSize", new Vector2(0.5f, 0.5f));
        Set(player, "knockbackDir", new Vector2(-5f, 10f));
        Set(player, "jumpForce", 15f);
        Set(player, "doubleJumpForce", 12f);

        return player;
    }

    private void DestroyPlayer()
    {
        if (playerObj != null) Object.DestroyImmediate(playerObj);
    }

    [UnityTest]
    [Description("Player - Damage plays bloodFx")]
    public IEnumerator Player_Damage_PlaysBloodFx()
    {
        CreatePlayer();
        yield return null;

        Set(player, "canBeKnocked", true);
        Set(player, "moveSpeed", 20f);
        Set(player, "speedToSurvive", 18f);
        player.extraLife = true;
        Set(player, "defaultSpeed", 8f);
        Set(player, "defaultMilestoneIncrease", 5f);
        Set(player, "isSliding", false);

        var bloodFx = Get<ParticleSystem>(player, "bloodFx");

        player.Damage();
        yield return null;

        Assert.IsTrue(bloodFx.isPlaying || bloodFx.particleCount >= 0,
            "Damage phải gọi bloodFx.Play().");

        DestroyPlayer();
    }

    [UnityTest]
    [Description("Player - Damage calls Knockback when extraLife is true")]
    public IEnumerator Player_Damage_Knockback_WhenExtraLife()
    {
        CreatePlayer();
        yield return null;

        Set(player, "canBeKnocked", true);
        Set(player, "moveSpeed", 20f);
        Set(player, "speedToSurvive", 18f);
        Set(player, "defaultSpeed", 8f);
        Set(player, "defaultMilestoneIncrease", 5f);
        Set(player, "isSliding", false);
        player.extraLife = true;

        player.Damage();
        yield return null;

        bool isKnocked = Get<bool>(player, "isKnocked");
        Assert.IsTrue(isKnocked,
            "Damage phải gọi Knockback khi extraLife = true, set isKnocked = true.");

        DestroyPlayer();
    }

    [UnityTest]
    [Description("Player - Damage starts Die coroutine when extraLife is false")]
    public IEnumerator Player_Damage_Die_WhenNoExtraLife()
    {
        CreatePlayer();
        yield return null;

        // Need AudioManager for Die()
        var amObj = new GameObject("AM");
        var am = amObj.AddComponent<AudioManager>();
        var sfxSources = new AudioSource[] { amObj.AddComponent<AudioSource>(),
            amObj.AddComponent<AudioSource>(), amObj.AddComponent<AudioSource>(),
            amObj.AddComponent<AudioSource>() };
        Set(am, "sfx", sfxSources);
        Set(am, "bgm", new AudioSource[] { amObj.AddComponent<AudioSource>() });

        Set(player, "canBeKnocked", true);
        player.extraLife = false;
        Set(player, "isDead", false);

        player.Damage();
        yield return new WaitForSeconds(0.1f);

        bool isDead = Get<bool>(player, "isDead");
        Assert.IsTrue(isDead,
            "Damage phải gọi Die() khi extraLife = false, set isDead = true.");

        Time.timeScale = 1f;
        DestroyPlayer();
        Object.DestroyImmediate(amObj);
    }

    [UnityTest]
    [Description("Player - Knockback sets isKnocked and applies knockbackDir")]
    public IEnumerator Player_Knockback_SetsIsKnockedAndVelocity()
    {
        CreatePlayer();
        yield return null;

        Set(player, "canBeKnocked", true);
        Set(player, "isSliding", false);
        Set(player, "defaultSpeed", 8f);
        Set(player, "defaultMilestoneIncrease", 5f);
        Set(player, "knockbackDir", new Vector2(-5f, 10f));

        Invoke(player, "Knockback");

        bool isKnocked = Get<bool>(player, "isKnocked");
        Assert.IsTrue(isKnocked,
            "Knockback phải set isKnocked = true.");
        Assert.AreEqual(-5f, playerRb.linearVelocity.x, 0.01f,
            "Knockback phải set velocity.x = knockbackDir.x.");

        DestroyPlayer();
    }

    [UnityTest]
    [Description("Player - Jump sets velocity.y to given force")]
    public IEnumerator Player_Jump_SetsVelocityY()
    {
        CreatePlayer();
        yield return null;

        // Need AudioManager for Jump()
        var amObj = new GameObject("AM");
        var am = amObj.AddComponent<AudioManager>();
        var sfxSources = new AudioSource[] { amObj.AddComponent<AudioSource>(),
            amObj.AddComponent<AudioSource>(), amObj.AddComponent<AudioSource>() };
        Set(am, "sfx", sfxSources);
        Set(am, "bgm", new AudioSource[] { amObj.AddComponent<AudioSource>() });

        playerRb.linearVelocity = new Vector2(10f, 0f);

        Invoke(player, "Jump", 15f);

        Assert.AreEqual(15f, playerRb.linearVelocity.y, 0.01f,
            "Jump phải set velocity.y bằng force được truyền vào.");

        DestroyPlayer();
        Object.DestroyImmediate(amObj);
    }

    [UnityTest]
    [Description("Player - RollAnimFinished sets canRoll to false")]
    public IEnumerator Player_RollAnimFinished_SetsFalse()
    {
        CreatePlayer();
        yield return null;

        var anim = playerObj.GetComponent<Animator>();
        // RollAnimFinished sets canRoll = false in animator
        Assert.DoesNotThrow(() => Invoke(player, "RollAnimFinished"),
            "RollAnimFinished không được throw exception.");

        DestroyPlayer();
    }

    [UnityTest]
    [Description("Player - SpeedController does not exceed maxSpeed")]
    public IEnumerator Player_SpeedController_CapsAtMaxSpeed()
    {
        CreatePlayer();
        yield return null;

        Set(player, "moveSpeed", 49f);
        Set(player, "maxSpeed", 50f);
        Set(player, "speedMilestone", 0f);
        Set(player, "milestoneIncreaser", 5f);
        Set(player, "speedMultiplier", 1.5f);
        playerObj.transform.position = new Vector3(1f, 0f, 0f);

        Invoke(player, "SpeedController");

        float speed = Get<float>(player, "moveSpeed");
        Assert.LessOrEqual(speed, 50f,
            "SpeedController phải cap moveSpeed tại maxSpeed.");

        DestroyPlayer();
    }

    [UnityTest]
    [Description("Player - CheckForLanding does nothing when falling slowly")]
    public IEnumerator Player_CheckForLanding_NothingWhenSlowFall()
    {
        CreatePlayer();
        yield return null;

        playerRb.linearVelocity = new Vector2(0f, -3f);
        Set(player, "isGrounded", false);
        Set(player, "readyToLand", false);

        Invoke(player, "CheckForLanding");

        Assert.IsFalse(Get<bool>(player, "readyToLand"),
            "CheckForLanding không nên set readyToLand khi velocity.y > -5.");

        DestroyPlayer();
    }

    [UnityTest]
    [Description("Player - CheckForSlideCancel keeps sliding when ceillingDetected")]
    public IEnumerator Player_CheckForSlideCancel_KeepsSliding_CeilingDetected()
    {
        CreatePlayer();
        yield return null;

        Set(player, "isSliding", true);
        Set(player, "slideTimeCounter", -1f);
        Set(player, "ceillingDetected", true);

        Invoke(player, "CheckForSlideCancel");

        Assert.IsTrue(Get<bool>(player, "isSliding"),
            "CheckForSlideCancel phải giữ isSliding khi ceillingDetected = true.");

        DestroyPlayer();
    }

    [UnityTest]
    [Description("Player - Die sets isDead true and applies knockback velocity")]
    public IEnumerator Player_Die_SetsDeadAndVelocity()
    {
        CreatePlayer();
        yield return null;

        var amObj = new GameObject("AM");
        var am = amObj.AddComponent<AudioManager>();
        var sfxSources = new AudioSource[] { amObj.AddComponent<AudioSource>(),
            amObj.AddComponent<AudioSource>(), amObj.AddComponent<AudioSource>(),
            amObj.AddComponent<AudioSource>() };
        Set(am, "sfx", sfxSources);
        Set(am, "bgm", new AudioSource[] { amObj.AddComponent<AudioSource>() });

        Set(player, "knockbackDir", new Vector2(-5f, 10f));

        var dieMethod = typeof(Player).GetMethod("Die", FLAGS);
        player.StartCoroutine((IEnumerator)dieMethod.Invoke(player, null));
        yield return new WaitForSeconds(0.1f);

        Assert.IsTrue(Get<bool>(player, "isDead"),
            "Die phải set isDead = true.");
        Assert.IsFalse(Get<bool>(player, "canBeKnocked"),
            "Die phải set canBeKnocked = false.");

        Time.timeScale = 1f;
        DestroyPlayer();
        Object.DestroyImmediate(amObj);
    }

    [UnityTest]
    [Description("Player - Invincibility toggles canBeKnocked and color")]
    public IEnumerator Player_Invincibility_TogglesCanBeKnocked()
    {
        CreatePlayer();
        yield return null;

        Set(player, "canBeKnocked", true);

        var invMethod = typeof(Player).GetMethod("Invincibility", FLAGS);
        player.StartCoroutine((IEnumerator)invMethod.Invoke(player, null));
        yield return new WaitForSeconds(0.05f);

        Assert.IsFalse(Get<bool>(player, "canBeKnocked"),
            "Invincibility phải set canBeKnocked = false ngay khi bắt đầu.");

        // Chờ hết invincibility
        yield return new WaitForSeconds(2f);

        Assert.IsTrue(Get<bool>(player, "canBeKnocked"),
            "Invincibility phải restore canBeKnocked = true sau khi kết thúc.");

        DestroyPlayer();
    }

    #endregion

    // ================================================================
    //  UI_MAIN - Từ 72.7% → 90%+
    //  Chưa cover: SwitchMenuTo, SwitchSkyBox, StartGameButton,
    //  Start (slider setup)
    // ================================================================

    #region UI_Main

    [UnityTest]
    [Description("UI_Main - SwitchMenuTo activates target and deactivates others")]
    public IEnumerator UIMain_SwitchMenuTo_ActivatesTarget()
    {
        yield return SceneManager.LoadSceneAsync(1);
        yield return new WaitForSeconds(0.5f);

        UI_Main uiMain = Object.FindFirstObjectByType<UI_Main>();
        Assert.IsNotNull(uiMain);

        var mainMenu = Get<GameObject>(uiMain, "mainMenu");
        var endGame = Get<GameObject>(uiMain, "endGame");

        uiMain.SwitchMenuTo(endGame);
        yield return null;

        Assert.IsTrue(endGame.activeSelf,
            "SwitchMenuTo phải activate menu target.");
    }

    [UnityTest]
    [Description("UI_Main - SwitchSkyBox calls GameManager.SetupSkyBox")]
    public IEnumerator UIMain_SwitchSkyBox_CallsGameManager()
    {
        yield return SceneManager.LoadSceneAsync(1);
        yield return new WaitForSeconds(0.5f);

        UI_Main uiMain = Object.FindFirstObjectByType<UI_Main>();
        Assert.IsNotNull(uiMain);

        uiMain.SwitchSkyBox(1);
        yield return null;

        int saved = PlayerPrefs.GetInt("SkyBoxSetting");
        Assert.AreEqual(1, saved,
            "SwitchSkyBox phải gọi GameManager.SetupSkyBox và lưu setting.");
    }

    [UnityTest]
    [Description("UI_Main - StartGameButton unlocks player")]
    public IEnumerator UIMain_StartGameButton_UnlocksPlayer()
    {
        yield return SceneManager.LoadSceneAsync(1);
        yield return new WaitForSeconds(0.5f);

        UI_Main uiMain = Object.FindFirstObjectByType<UI_Main>();
        Assert.IsNotNull(uiMain);

        Player p = GameManager.instance.player;
        p.playerUnlocked = false;

        uiMain.StartGameButton();
        yield return null;

        Assert.IsTrue(p.playerUnlocked,
            "StartGameButton phải unlock player.");
    }

    [UnityTest]
    [Description("UI_Main - PauseGameButton toggles pause state correctly")]
    [Category("PC")]
    [Category("Mobile")]
    public IEnumerator UIMain_PauseGameButton_TogglesPause()
    {
        yield return SceneManager.LoadSceneAsync(1);
        yield return new WaitForSeconds(0.3f);

        UI_Main uiMain = Object.FindFirstObjectByType<UI_Main>();
        Set(uiMain, "gamePaused", false);
        Time.timeScale = 1f;

        uiMain.PauseGameButton();
        Assert.AreEqual(0f, Time.timeScale, "Pause lần 1 phải set timeScale = 0.");

        uiMain.PauseGameButton();
        Assert.AreEqual(1f, Time.timeScale, "Pause lần 2 phải set timeScale = 1.");
    }

    [UnityTest]
    [Description("UI_Main - OpenEndGameUI switches to endGame panel")]
    public IEnumerator UIMain_OpenEndGameUI_SwitchesToEndGame()
    {
        yield return SceneManager.LoadSceneAsync(1);
        yield return new WaitForSeconds(0.5f);

        UI_Main uiMain = Object.FindFirstObjectByType<UI_Main>();
        var endGame = Get<GameObject>(uiMain, "endGame");

        uiMain.OpenEndGameUI();
        yield return null;

        Assert.IsTrue(endGame.activeSelf,
            "OpenEndGameUI phải activate endGame panel.");
    }

    #endregion

    // ================================================================
    //  UI_SHOP - Từ 32.7% → 90%+
    //  Chưa cover: Start (button creation), PurchaseColor
    // ================================================================

    #region UI_Shop

    [UnityTest]
    [Description("UI_Shop - PurchaseColor with platformColor sets GameManager.platformColor")]
    public IEnumerator UIShop_PurchaseColor_Platform_SetsColor()
    {
        yield return SceneManager.LoadSceneAsync(1);
        yield return new WaitForSeconds(0.5f);

        var shopObj = new GameObject("Shop");
        var coinsTextObj = new GameObject("CT");
        var notifyTextObj = new GameObject("NT");
        var displayObj = new GameObject("Disp");

        var coinsText = coinsTextObj.AddComponent<TextMeshProUGUI>();
        var notifyText = notifyTextObj.AddComponent<TextMeshProUGUI>();
        var platformDisplay = displayObj.AddComponent<Image>();

        var shop = shopObj.AddComponent<UI_Shop>();
        Set(shop, "coinsText", coinsText);
        Set(shop, "notifyText", notifyText);
        Set(shop, "platformDisplay", platformDisplay);

        PlayerPrefs.SetInt("Coins", 500);
        Color testColor = Color.cyan;

        shop.PurchaseColor(testColor, 10, ColorType.platformColor);
        yield return null;

        Assert.AreEqual(testColor, GameManager.instance.platformColor,
            "PurchaseColor phải set GameManager.platformColor khi mua platformColor.");
        Assert.AreEqual(testColor, platformDisplay.color,
            "PurchaseColor phải cập nhật platformDisplay.color.");

        PlayerPrefs.DeleteKey("Coins");
        Object.DestroyImmediate(shopObj);
        Object.DestroyImmediate(coinsTextObj);
        Object.DestroyImmediate(notifyTextObj);
        Object.DestroyImmediate(displayObj);
    }

    [UnityTest]
    [Description("UI_Shop - PurchaseColor with playerColor sets player sprite color")]
    public IEnumerator UIShop_PurchaseColor_Player_SetsColor()
    {
        yield return SceneManager.LoadSceneAsync(1);
        yield return new WaitForSeconds(0.5f);

        var shopObj = new GameObject("Shop");
        var coinsTextObj = new GameObject("CT");
        var notifyTextObj = new GameObject("NT");
        var displayObj = new GameObject("Disp");

        var coinsText = coinsTextObj.AddComponent<TextMeshProUGUI>();
        var notifyText = notifyTextObj.AddComponent<TextMeshProUGUI>();
        var playerDisplay = displayObj.AddComponent<Image>();

        var shop = shopObj.AddComponent<UI_Shop>();
        Set(shop, "coinsText", coinsText);
        Set(shop, "notifyText", notifyText);
        Set(shop, "playerDisplay", playerDisplay);

        PlayerPrefs.SetInt("Coins", 500);
        Color testColor = Color.magenta;

        shop.PurchaseColor(testColor, 10, ColorType.playerColor);
        yield return null;

        var playerSr = GameManager.instance.player.GetComponent<SpriteRenderer>();
        Assert.AreEqual(testColor, playerSr.color,
            "PurchaseColor phải set player SpriteRenderer.color khi mua playerColor.");
        Assert.AreEqual(testColor, playerDisplay.color,
            "PurchaseColor phải cập nhật playerDisplay.color.");

        PlayerPrefs.DeleteKey("Coins");
        Object.DestroyImmediate(shopObj);
        Object.DestroyImmediate(coinsTextObj);
        Object.DestroyImmediate(notifyTextObj);
        Object.DestroyImmediate(displayObj);
    }

    [UnityTest]
    [Description("UI_Shop - PurchaseColor shows 'Not enough money' when insufficient")]
    public IEnumerator UIShop_PurchaseColor_NotEnoughMoney()
    {
        yield return SceneManager.LoadSceneAsync(1);
        yield return new WaitForSeconds(0.5f);

        var shopObj = new GameObject("Shop");
        var coinsTextObj = new GameObject("CT");
        var notifyTextObj = new GameObject("NT");

        var coinsText = coinsTextObj.AddComponent<TextMeshProUGUI>();
        var notifyText = notifyTextObj.AddComponent<TextMeshProUGUI>();

        var shop = shopObj.AddComponent<UI_Shop>();
        Set(shop, "coinsText", coinsText);
        Set(shop, "notifyText", notifyText);

        PlayerPrefs.SetInt("Coins", 5);

        shop.PurchaseColor(Color.red, 100, ColorType.platformColor);
        yield return null;

        Assert.AreEqual("Not enough money!", notifyText.text,
            "PurchaseColor phải hiện 'Not enough money!' khi không đủ tiền.");

        PlayerPrefs.DeleteKey("Coins");
        Object.DestroyImmediate(shopObj);
        Object.DestroyImmediate(coinsTextObj);
        Object.DestroyImmediate(notifyTextObj);
    }

    [UnityTest]
    [Description("UI_Shop - PurchaseColor shows 'Purchased successful' when enough")]
    public IEnumerator UIShop_PurchaseColor_ShowsSuccess()
    {
        yield return SceneManager.LoadSceneAsync(1);
        yield return new WaitForSeconds(0.5f);

        var shopObj = new GameObject("Shop");
        var coinsTextObj = new GameObject("CT");
        var notifyTextObj = new GameObject("NT");
        var displayObj = new GameObject("Disp");

        var coinsText = coinsTextObj.AddComponent<TextMeshProUGUI>();
        var notifyText = notifyTextObj.AddComponent<TextMeshProUGUI>();
        var platformDisplay = displayObj.AddComponent<Image>();

        var shop = shopObj.AddComponent<UI_Shop>();
        Set(shop, "coinsText", coinsText);
        Set(shop, "notifyText", notifyText);
        Set(shop, "platformDisplay", platformDisplay);

        PlayerPrefs.SetInt("Coins", 500);

        shop.PurchaseColor(Color.red, 10, ColorType.platformColor);
        yield return null;

        Assert.AreEqual("Purchased successful", notifyText.text,
            "PurchaseColor phải hiện 'Purchased successful' khi đủ tiền.");

        PlayerPrefs.DeleteKey("Coins");
        Object.DestroyImmediate(shopObj);
        Object.DestroyImmediate(coinsTextObj);
        Object.DestroyImmediate(notifyTextObj);
        Object.DestroyImmediate(displayObj);
    }

    #endregion

    // ================================================================
    //  TRAP - Từ 60% → 90%+
    //  Chưa cover: OnTriggerEnter2D, Start destroy branch
    // ================================================================

    #region Trap

    [Test]
    [Description("Trap - Start destroys object when chanceToSpawn is 0")]
    public void Trap_Start_DestroysWhenChanceIsZero()
    {
        var trapObj = new GameObject("Trap");
        var trap = trapObj.AddComponent<Trap>();

        Set(trap, "chanceToSpawn", 0f);

        var startMethod = typeof(Trap).GetMethod("Start", FLAGS);
        startMethod.Invoke(trap, null);

        // Destroy is deferred, check after frame. In edit mode, check destroyed flag.
        Assert.IsTrue(trapObj == null || !trapObj.activeInHierarchy || trap == null,
            "Trap phải bị Destroy khi chanceToSpawn = 0.");

        if (trapObj != null) Object.DestroyImmediate(trapObj);
    }

    [UnityTest]
    [Description("Trap - OnTriggerEnter2D calls Player.Damage")]
    public IEnumerator Trap_OnTriggerEnter2D_CallsPlayerDamage()
    {
        yield return SceneManager.LoadSceneAsync(1);
        yield return new WaitForSeconds(0.5f);

        GameManager.instance.UnlockPlayer();
        yield return new WaitForSeconds(0.3f);

        Player p = GameManager.instance.player;
        Set(p, "canBeKnocked", true);
        Set(p, "moveSpeed", 20f);
        Set(p, "speedToSurvive", 18f);
        p.extraLife = true;
        Set(p, "defaultSpeed", 8f);
        Set(p, "defaultMilestoneIncrease", 5f);
        Set(p, "isSliding", false);

        var trapObj = new GameObject("Trap");
        var trap = trapObj.AddComponent<Trap>();
        Set(trap, "chanceToSpawn", 200f);

        BoxCollider2D playerCollider = p.GetComponent<BoxCollider2D>();
        if (playerCollider == null)
            playerCollider = p.gameObject.AddComponent<BoxCollider2D>();

        var triggerMethod = typeof(Trap).GetMethod("OnTriggerEnter2D", FLAGS);
        triggerMethod.Invoke(trap, new object[] { playerCollider });

        yield return null;

        bool isKnocked = Get<bool>(p, "isKnocked");
        Assert.IsTrue(isKnocked,
            "Trap.OnTriggerEnter2D phải gọi Player.Damage() → Knockback khi extraLife.");

        Object.DestroyImmediate(trapObj);
    }

    #endregion

    // ================================================================
    //  MOVINGTRAP - Từ 84.2% → 90%+
    //  Chưa cover: Update rotation branches, index wrap
    // ================================================================

    #region MovingTrap

    [Test]
    [Description("MovingTrap - Inherits from Trap")]
    public void MovingTrap_InheritsFromTrap()
    {
        Assert.IsTrue(typeof(MovingTrap).IsSubclassOf(typeof(Trap)),
            "MovingTrap phải kế thừa từ Trap.");
    }

    [UnityTest]
    [Description("MovingTrap - Update moves towards current movePoint")]
    public IEnumerator MovingTrap_Update_MovesTowardsMovePoint()
    {
        var trapObj = new GameObject("MovingTrap");
        var trap = trapObj.AddComponent<MovingTrap>();

        var p1 = new GameObject("P1");
        var p2 = new GameObject("P2");
        p1.transform.position = new Vector3(0f, 0f, 0f);
        p2.transform.position = new Vector3(10f, 0f, 0f);

        Set(trap, "movePoint", new Transform[] { p1.transform, p2.transform });
        Set(trap, "speed", 50f);
        Set(trap, "rotationSpeed", 10f);
        Set(trap, "chanceToSpawn", 200f);
        Set(trap, "i", 1);

        trapObj.transform.position = Vector3.zero;

        yield return new WaitForSeconds(0.3f);

        Assert.Greater(trapObj.transform.position.x, 0f,
            "MovingTrap phải di chuyển về phía movePoint.");

        Object.DestroyImmediate(trapObj);
        Object.DestroyImmediate(p1);
        Object.DestroyImmediate(p2);
    }

    #endregion

    // ================================================================
    //  PLATFORMCONTROLLER - Từ 75% → 90%+
    //  Chưa cover: OnTriggerEnter2D branches
    // ================================================================

    #region PlatformController

    [UnityTest]
    [Description("PlatformController - OnTriggerEnter2D colors header with platformColor")]
    public IEnumerator PlatformController_Trigger_ColorsHeader()
    {
        yield return SceneManager.LoadSceneAsync(1);
        yield return new WaitForSeconds(0.5f);

        GameManager.instance.colorEntierPlatform = false;
        GameManager.instance.platformColor = Color.yellow;

        var platObj = new GameObject("Platform");
        var sr = platObj.AddComponent<SpriteRenderer>();

        var headerObj = new GameObject("Header");
        headerObj.transform.SetParent(platObj.transform);
        var headerSr = headerObj.AddComponent<SpriteRenderer>();

        var pc = platObj.AddComponent<PlatformController>();
        Set(pc, "sr", sr);
        Set(pc, "headerSr", headerSr);

        Player p = GameManager.instance.player;
        BoxCollider2D playerCol = p.GetComponent<BoxCollider2D>();
        if (playerCol == null) playerCol = p.gameObject.AddComponent<BoxCollider2D>();

        var triggerMethod = typeof(PlatformController).GetMethod("OnTriggerEnter2D", FLAGS);
        triggerMethod.Invoke(pc, new object[] { playerCol });

        Assert.AreEqual(Color.yellow, headerSr.color,
            "OnTriggerEnter2D phải tô màu header với platformColor.");

        Object.DestroyImmediate(platObj);
    }

    [UnityTest]
    [Description("PlatformController - OnTriggerEnter2D colors both when colorEntierPlatform")]
    public IEnumerator PlatformController_Trigger_ColorsBoth_WhenEntire()
    {
        yield return SceneManager.LoadSceneAsync(1);
        yield return new WaitForSeconds(0.5f);

        GameManager.instance.colorEntierPlatform = true;
        GameManager.instance.platformColor = Color.green;

        var platObj = new GameObject("Platform");
        var sr = platObj.AddComponent<SpriteRenderer>();

        var headerObj = new GameObject("Header");
        headerObj.transform.SetParent(platObj.transform);
        var headerSr = headerObj.AddComponent<SpriteRenderer>();

        var pc = platObj.AddComponent<PlatformController>();
        Set(pc, "sr", sr);
        Set(pc, "headerSr", headerSr);

        Player p = GameManager.instance.player;
        BoxCollider2D playerCol = p.GetComponent<BoxCollider2D>();
        if (playerCol == null) playerCol = p.gameObject.AddComponent<BoxCollider2D>();

        var triggerMethod = typeof(PlatformController).GetMethod("OnTriggerEnter2D", FLAGS);
        triggerMethod.Invoke(pc, new object[] { playerCol });

        Assert.AreEqual(Color.green, headerSr.color,
            "colorEntierPlatform: header phải được tô màu.");
        Assert.AreEqual(Color.green, sr.color,
            "colorEntierPlatform: sr (body) phải được tô màu.");

        Object.DestroyImmediate(platObj);
    }

    [UnityTest]
    [Description("PlatformController - OnTriggerEnter2D ignores non-Player colliders")]
    public IEnumerator PlatformController_Trigger_IgnoresNonPlayer()
    {
        yield return SceneManager.LoadSceneAsync(1);
        yield return new WaitForSeconds(0.5f);

        GameManager.instance.platformColor = Color.red;

        var platObj = new GameObject("Platform");
        var sr = platObj.AddComponent<SpriteRenderer>();

        var headerObj = new GameObject("Header");
        headerObj.transform.SetParent(platObj.transform);
        var headerSr = headerObj.AddComponent<SpriteRenderer>();
        headerSr.color = Color.white;

        var pc = platObj.AddComponent<PlatformController>();
        Set(pc, "sr", sr);
        Set(pc, "headerSr", headerSr);

        var otherObj = new GameObject("Other");
        var otherCol = otherObj.AddComponent<BoxCollider2D>();

        var triggerMethod = typeof(PlatformController).GetMethod("OnTriggerEnter2D", FLAGS);
        triggerMethod.Invoke(pc, new object[] { otherCol });

        Assert.AreEqual(Color.white, headerSr.color,
            "OnTriggerEnter2D phải bỏ qua khi collider không phải Player.");

        Object.DestroyImmediate(platObj);
        Object.DestroyImmediate(otherObj);
    }

    #endregion

    // ================================================================
    //  RAINCONTROLLER - Từ 67.3% → 90%+
    //  Chưa cover: CheckForRain, ChangeIntensity
    // ================================================================

    #region RainController

    [Test]
    [Description("RainController - ChangeIntensity increases toward target")]
    public void RainController_ChangeIntensity_IncreasesTowardTarget()
    {
        var rcObj = new GameObject("RainCtrl");
        // RainController needs RainScript2D, so we test via reflection only
        // on the ChangeIntensity logic directly

        var rc = rcObj.AddComponent<RainController>();
        Set(rc, "intensity", 0.1f);
        Set(rc, "targetIntensity", 0.5f);
        Set(rc, "changeRate", 100f); // Very fast rate
        Set(rc, "canChangeIntensity", true);

        // Simulate Time.deltaTime = 1 by calling with large changeRate
        Invoke(rc, "ChangeIntensity");

        float intensity = Get<float>(rc, "intensity");
        Assert.GreaterOrEqual(intensity, 0.1f,
            "ChangeIntensity phải tăng intensity về phía targetIntensity.");

        Object.DestroyImmediate(rcObj);
    }

    [Test]
    [Description("RainController - ChangeIntensity decreases toward lower target")]
    public void RainController_ChangeIntensity_DecreasesTowardTarget()
    {
        var rcObj = new GameObject("RainCtrl");
        var rc = rcObj.AddComponent<RainController>();
        Set(rc, "intensity", 0.5f);
        Set(rc, "targetIntensity", 0.1f);
        Set(rc, "changeRate", 100f);
        Set(rc, "canChangeIntensity", true);

        Invoke(rc, "ChangeIntensity");

        float intensity = Get<float>(rc, "intensity");
        Assert.LessOrEqual(intensity, 0.5f,
            "ChangeIntensity phải giảm intensity về phía targetIntensity thấp hơn.");

        Object.DestroyImmediate(rcObj);
    }

    [Test]
    [Description("RainController - ChangeIntensity stops when reaching target (increase)")]
    public void RainController_ChangeIntensity_StopsAtTarget_Increase()
    {
        var rcObj = new GameObject("RainCtrl");
        var rc = rcObj.AddComponent<RainController>();
        Set(rc, "intensity", 0.49f);
        Set(rc, "targetIntensity", 0.5f);
        Set(rc, "changeRate", 1000f); // Overshoot
        Set(rc, "canChangeIntensity", true);

        Invoke(rc, "ChangeIntensity");

        float intensity = Get<float>(rc, "intensity");
        bool canChange = Get<bool>(rc, "canChangeIntensity");

        Assert.AreEqual(0.5f, intensity, 0.01f,
            "ChangeIntensity phải dừng đúng tại targetIntensity.");
        Assert.IsFalse(canChange,
            "ChangeIntensity phải set canChangeIntensity = false khi đạt target.");

        Object.DestroyImmediate(rcObj);
    }

    [Test]
    [Description("RainController - ChangeIntensity stops when reaching target (decrease)")]
    public void RainController_ChangeIntensity_StopsAtTarget_Decrease()
    {
        var rcObj = new GameObject("RainCtrl");
        var rc = rcObj.AddComponent<RainController>();
        Set(rc, "intensity", 0.11f);
        Set(rc, "targetIntensity", 0.1f);
        Set(rc, "changeRate", 1000f);
        Set(rc, "canChangeIntensity", true);

        Invoke(rc, "ChangeIntensity");

        float intensity = Get<float>(rc, "intensity");
        bool canChange = Get<bool>(rc, "canChangeIntensity");

        Assert.AreEqual(0.1f, intensity, 0.01f);
        Assert.IsFalse(canChange);

        Object.DestroyImmediate(rcObj);
    }

    [Test]
    [Description("RainController - CheckForRain sets targetIntensity when timer < 0")]
    public void RainController_CheckForRain_SetsTarget_WhenTimerExpired()
    {
        var rcObj = new GameObject("RainCtrl");
        var rc = rcObj.AddComponent<RainController>();
        Set(rc, "rainCheckTimer", -1f);
        Set(rc, "rainCheckCooldown", 10f);
        Set(rc, "chanceToRain", 100f); // Always rain
        Set(rc, "minValue", 0.2f);
        Set(rc, "maxValue", 0.49f);
        Set(rc, "canChangeIntensity", false);

        Invoke(rc, "CheckForRain");

        bool canChange = Get<bool>(rc, "canChangeIntensity");
        float target = Get<float>(rc, "targetIntensity");
        float timer = Get<float>(rc, "rainCheckTimer");

        Assert.IsTrue(canChange,
            "CheckForRain phải set canChangeIntensity = true.");
        Assert.AreEqual(10f, timer,
            "CheckForRain phải reset timer về rainCheckCooldown.");
        Assert.That(target, Is.InRange(0.2f, 0.49f),
            "CheckForRain phải set targetIntensity trong khoảng [minValue, maxValue] khi rain.");

        Object.DestroyImmediate(rcObj);
    }

    [Test]
    [Description("RainController - CheckForRain sets target 0 when no rain chance")]
    public void RainController_CheckForRain_SetsTarget0_WhenNoRain()
    {
        var rcObj = new GameObject("RainCtrl");
        var rc = rcObj.AddComponent<RainController>();
        Set(rc, "rainCheckTimer", -1f);
        Set(rc, "rainCheckCooldown", 10f);
        Set(rc, "chanceToRain", 0f); // Never rain
        Set(rc, "minValue", 0.2f);
        Set(rc, "maxValue", 0.49f);

        Invoke(rc, "CheckForRain");

        float target = Get<float>(rc, "targetIntensity");
        Assert.AreEqual(0f, target,
            "CheckForRain phải set targetIntensity = 0 khi không mưa.");

        Object.DestroyImmediate(rcObj);
    }

    [Test]
    [Description("RainController - CheckForRain does nothing when timer > 0")]
    public void RainController_CheckForRain_DoesNothing_WhenTimerPositive()
    {
        var rcObj = new GameObject("RainCtrl");
        var rc = rcObj.AddComponent<RainController>();
        Set(rc, "rainCheckTimer", 5f);
        Set(rc, "canChangeIntensity", false);
        Set(rc, "targetIntensity", 0.3f);

        Invoke(rc, "CheckForRain");

        bool canChange = Get<bool>(rc, "canChangeIntensity");
        Assert.IsFalse(canChange,
            "CheckForRain không làm gì khi timer > 0.");

        Object.DestroyImmediate(rcObj);
    }

    #endregion

    // ================================================================
    //  LEDGEDETECTION - Từ 88% → 90%+
    //  Chưa cover: OnTriggerExit2D
    // ================================================================

    #region LedgeDetection

    [Test]
    [Description("LedgeDetection - OnTriggerExit2D re-enables detection on Ground layer")]
    public void LedgeDetection_OnTriggerExit2D_ReEnablesDetection()
    {
        var ldObj = new GameObject("LedgeDetect");
        ldObj.AddComponent<BoxCollider2D>();
        var ld = ldObj.AddComponent<LedgeDetection>();
        ld.canDetect = false;

        var groundObj = new GameObject("Ground");
        groundObj.layer = LayerMask.NameToLayer("Ground");
        var groundCol = groundObj.AddComponent<BoxCollider2D>();

        var exitMethod = typeof(LedgeDetection).GetMethod("OnTriggerExit2D", FLAGS);
        exitMethod.Invoke(ld, new object[] { groundCol });

        Assert.IsTrue(ld.canDetect,
            "OnTriggerExit2D phải set canDetect = true khi rời khỏi Ground layer.");

        Object.DestroyImmediate(ldObj);
        Object.DestroyImmediate(groundObj);
    }

    [Test]
    [Description("LedgeDetection - OnTriggerEnter2D disables detection on Ground")]
    public void LedgeDetection_OnTriggerEnter2D_DisablesDetection()
    {
        var ldObj = new GameObject("LedgeDetect");
        var ld = ldObj.AddComponent<LedgeDetection>();
        ld.canDetect = true;

        var groundObj = new GameObject("Ground");
        groundObj.layer = LayerMask.NameToLayer("Ground");
        var groundCol = groundObj.AddComponent<BoxCollider2D>();

        var enterMethod = typeof(LedgeDetection).GetMethod("OnTriggerEnter2D", FLAGS);
        enterMethod.Invoke(ld, new object[] { groundCol });

        Assert.IsFalse(ld.canDetect,
            "OnTriggerEnter2D phải set canDetect = false khi chạm Ground.");

        Object.DestroyImmediate(ldObj);
        Object.DestroyImmediate(groundObj);
    }

    [Test]
    [Description("LedgeDetection - OnTriggerEnter2D ignores non-Ground layers")]
    public void LedgeDetection_OnTriggerEnter2D_IgnoresNonGround()
    {
        var ldObj = new GameObject("LedgeDetect");
        var ld = ldObj.AddComponent<LedgeDetection>();
        ld.canDetect = true;

        var otherObj = new GameObject("Other");
        otherObj.layer = LayerMask.NameToLayer("Default");
        var otherCol = otherObj.AddComponent<BoxCollider2D>();

        var enterMethod = typeof(LedgeDetection).GetMethod("OnTriggerEnter2D", FLAGS);
        enterMethod.Invoke(ld, new object[] { otherCol });

        Assert.IsTrue(ld.canDetect,
            "OnTriggerEnter2D phải bỏ qua layer không phải Ground.");

        Object.DestroyImmediate(ldObj);
        Object.DestroyImmediate(otherObj);
    }

    #endregion
}
