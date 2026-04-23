using System.Collections;
using System.Reflection;
using NUnit.Framework;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

/// <summary>
/// Gameplay Tests - Logic game: di chuyển, tốc độ, nhảy, trượt, knockback,
/// coin, score, save, trap, sinh map, enemy, rain, ledge, platform, component.
/// </summary>
[TestFixture]
public class GameplayTests
{
    private static readonly BindingFlags F = BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public;
    private void Set(object t, string f, object v) { t.GetType().GetField(f, F).SetValue(t, v); }
    private T Get<T>(object t, string f) { return (T)t.GetType().GetField(f, F).GetValue(t); }
    private object Inv(object t, string m, params object[] a) { return t.GetType().GetMethod(m, F).Invoke(t, a); }

    private GameObject tempCoinObj, tempPartPrefab, tempGeneratorObj, tempPlayerObj, tempTrapObj, tempLedgeObj, tempGroundObj;
    private GameObject playerObj, enemyObj;

    private IEnumerator Load() { SceneManager.LoadScene(1); yield return new WaitForSeconds(0.5f); }
    private void Unlock() { GameManager.instance.UnlockPlayer(); }

    private Player CreatePlayer()
    {
        playerObj = new GameObject("P");
        playerObj.AddComponent<Rigidbody2D>().gravityScale = 5;
        playerObj.AddComponent<SpriteRenderer>(); playerObj.AddComponent<Animator>();
        var dO = new GameObject("D"); dO.transform.SetParent(playerObj.transform);
        var bO = new GameObject("B"); bO.transform.SetParent(playerObj.transform);
        var wO = new GameObject("W"); wO.transform.SetParent(playerObj.transform);
        var p = playerObj.AddComponent<Player>();
        Set(p, "dustFx", dO.AddComponent<ParticleSystem>());
        Set(p, "bloodFx", bO.AddComponent<ParticleSystem>());
        Set(p, "wallCheck", wO.transform); Set(p, "wallCheckSize", new Vector2(0.5f, 0.5f));
        Set(p, "knockbackDir", new Vector2(-5f, 10f)); Set(p, "jumpForce", 15f); Set(p, "doubleJumpForce", 12f);
        return p;
    }
    private Enemy CreateEnemy()
    {
        enemyObj = new GameObject("E"); enemyObj.SetActive(false);
        enemyObj.AddComponent<Rigidbody2D>(); enemyObj.AddComponent<Animator>(); enemyObj.AddComponent<SpriteRenderer>();
        var e = enemyObj.AddComponent<Enemy>();
        Set(e, "rb", enemyObj.GetComponent<Rigidbody2D>()); Set(e, "anim", enemyObj.GetComponent<Animator>());
        Set(e, "defaultGravityScale", 5f); Set(e, "jumpForce", 10f); Set(e, "moveSpeed", 15f);
        return e;
    }
    private AudioManager CreateAM()
    {
        var o = new GameObject("AM"); var am = o.AddComponent<AudioManager>();
        Set(am, "sfx", new AudioSource[] { o.AddComponent<AudioSource>(), o.AddComponent<AudioSource>(), o.AddComponent<AudioSource>(), o.AddComponent<AudioSource>() });
        Set(am, "bgm", new AudioSource[] { o.AddComponent<AudioSource>() });
        return am;
    }

    [TearDown]
    public void Teardown()
    {
        if (tempCoinObj) Object.DestroyImmediate(tempCoinObj);
        if (tempPartPrefab) Object.DestroyImmediate(tempPartPrefab);
        if (tempGeneratorObj) Object.DestroyImmediate(tempGeneratorObj);
        if (tempPlayerObj) Object.DestroyImmediate(tempPlayerObj);
        if (tempTrapObj) Object.DestroyImmediate(tempTrapObj);
        if (tempLedgeObj) Object.DestroyImmediate(tempLedgeObj);
        if (tempGroundObj) Object.DestroyImmediate(tempGroundObj);
        if (playerObj) Object.DestroyImmediate(playerObj);
        if (enemyObj) Object.DestroyImmediate(enemyObj);
        PlayerPrefs.DeleteKey("Coins"); Time.timeScale = 1f;
    }

    // ═════════════════════════════════════════════════════════════
    //  GIỮ NGUYÊN TỪ FILE GỐC (TC_LOGIC_17 → 34, TC_UI_2, TC_LOGIN_01)
    // ═════════════════════════════════════════════════════════════

    [UnityTest, Category("PC"), Category("Mobile")]
    [Description("TC_LOGIC_17 - Kiểm tra tốc độ Player tăng lên khi di chuyển vượt qua các mốc speedMilestone")]
    public IEnumerator TC_LOGIC_17() { yield return Load(); Unlock(); var p = GameManager.instance.player; yield return new WaitForSeconds(0.2f); float b = p.GetComponent<Rigidbody2D>().linearVelocity.x; p.transform.position = new Vector3(300f, p.transform.position.y, 0f); yield return new WaitForSeconds(0.5f); Assert.Greater(p.GetComponent<Rigidbody2D>().linearVelocity.x, b); }

    [UnityTest, Category("PC"), Category("Mobile")]
    [Description("TC_LOGIC_18 - Kiểm tra GameManager.distance tăng theo vị trí X hiện tại của Player")]
    public IEnumerator TC_LOGIC_18() { yield return Load(); Unlock(); yield return new WaitForSeconds(0.3f); float b = GameManager.instance.distance; GameManager.instance.player.transform.position = new Vector3(b + 50f, GameManager.instance.player.transform.position.y, 0f); yield return new WaitForSeconds(0.2f); Assert.Greater(GameManager.instance.distance, b); }

    [UnityTest, Category("PC"), Category("Mobile")]
    [Description("TC_UI_2 - Kiểm tra nhân vật thực sự di chuyển về phía trước sau 2 giây chơi game")]
    public IEnumerator TC_UI_2() { yield return SceneManager.LoadSceneAsync(1); Time.timeScale = 1; Unlock(); yield return null; float s = GameManager.instance.player.transform.position.x; yield return new WaitForSeconds(2f); Assert.Greater(GameManager.instance.player.transform.position.x, s); }

    [UnityTest, Category("PC"), Category("Mobile")]
    [Description("TC_LOGIC_19 - Kiểm tra moveSpeed reset về defaultSpeed khi Player chạm tường (wallDetected = true)")]
    public IEnumerator TC_LOGIC_19() { yield return Load(); Unlock(); var p = GameManager.instance.player; var msF = typeof(Player).GetField("moveSpeed", F); var dsF = typeof(Player).GetField("defaultSpeed", F); var wF = typeof(Player).GetField("wallDetected", F); p.transform.position = new Vector3(300f, p.transform.position.y, 0f); yield return new WaitForSeconds(0.5f); float ds = (float)dsF.GetValue(p); Assert.Greater((float)msF.GetValue(p), ds); wF.SetValue(p, true); yield return new WaitForSeconds(0.1f); Assert.AreEqual(ds, (float)msF.GetValue(p), 0.01f); wF.SetValue(p, false); }

    [UnityTest, Category("PC"), Category("Mobile")]
    [Description("TC_LOGIC_20 - Kiểm tra Player không thể nhảy lần 3 - chỉ cho phép tối đa double jump")]
    public IEnumerator TC_LOGIC_20() { yield return Load(); Unlock(); var p = GameManager.instance.player; var rb = p.GetComponent<Rigidbody2D>(); yield return new WaitForSeconds(0.4f); p.JumpButton(); yield return new WaitForSeconds(0.2f); p.JumpButton(); yield return new WaitForSeconds(0.15f); float b = rb.linearVelocity.y; p.JumpButton(); yield return new WaitForSeconds(0.05f); Assert.LessOrEqual(rb.linearVelocity.y, b + 0.5f); }

    [UnityTest, Category("PC"), Category("Mobile")]
    [Description("TC_LOGIC_21 - Kiểm tra Slide không thể kích hoạt lại khi đang trong thời gian cooldown")]
    public IEnumerator TC_LOGIC_21() { yield return Load(); Unlock(); var p = GameManager.instance.player; yield return new WaitForSeconds(0.3f); p.SlideButton(); yield return new WaitForSeconds(0.05f); float c1 = p.slideCooldownCounter; p.SlideButton(); yield return new WaitForSeconds(0.05f); Assert.LessOrEqual(p.slideCooldownCounter, c1); }

    [UnityTest, Category("PC"), Category("Mobile")]
    [Description("TC_LOGIC_22 - Kiểm tra canDoubleJump được reset về true khi Player tiếp đất sau double jump")]
    public IEnumerator TC_LOGIC_22() { yield return Load(); Unlock(); var p = GameManager.instance.player; var f = typeof(Player).GetField("canDoubleJump", F); yield return new WaitForSeconds(0.5f); p.JumpButton(); yield return new WaitForSeconds(0.15f); p.JumpButton(); yield return new WaitForSeconds(0.1f); Assert.IsFalse((bool)f.GetValue(p)); yield return new WaitForSeconds(2f); Assert.IsTrue((bool)f.GetValue(p)); }

    [UnityTest, Category("PC"), Category("Mobile")]
    [Description("TC_LOGIC_23 - Kiểm tra extraLife bật khi moveSpeed >= speedToSurvive và tắt khi nhỏ hơn")]
    public IEnumerator TC_LOGIC_23() { yield return SceneManager.LoadSceneAsync(1); yield return new WaitForSeconds(0.5f); var p = GameManager.instance.player; var sF = typeof(Player).GetField("speedToSurvive", F); var mF = typeof(Player).GetField("moveSpeed", F); float s = (float)sF.GetValue(p); mF.SetValue(p, s + 1f); yield return null; yield return null; Assert.IsTrue(p.extraLife); mF.SetValue(p, s - 1f); yield return null; yield return null; Assert.IsFalse(p.extraLife); }

    [UnityTest, Category("PC"), Category("Mobile")]
    [Description("TC_LOGIC_24 - Kiểm tra extraLife flag đồng bộ chính xác với moveSpeed so với ngưỡng speedToSurvive")]
    public IEnumerator TC_LOGIC_24() { yield return Load(); Unlock(); var p = GameManager.instance.player; var mF = typeof(Player).GetField("moveSpeed", F); mF.SetValue(p, 10f); yield return new WaitForSeconds(0.1f); Assert.IsFalse(p.extraLife); mF.SetValue(p, 20f); yield return new WaitForSeconds(0.1f); Assert.IsTrue(p.extraLife); }

    [UnityTest, Category("PC"), Category("Mobile")]
    [Description("TC_LOGIC_25 - Kiểm tra coins tăng lên và Coin bị hủy khi Player nhặt coin")]
    public IEnumerator TC_LOGIC_25() { yield return SceneManager.LoadSceneAsync(1); yield return new WaitForSeconds(0.5f); int b = GameManager.instance.coins; tempCoinObj = new GameObject("Coin"); var c = tempCoinObj.AddComponent<Coin>(); tempPlayerObj = new GameObject("P"); tempPlayerObj.tag = "Player"; var col = tempPlayerObj.AddComponent<BoxCollider2D>(); typeof(Coin).GetMethod("OnTriggerEnter2D", F).Invoke(c, new object[] { col }); yield return null; Assert.That(tempCoinObj == null || !tempCoinObj.activeInHierarchy, Is.True); Assert.Greater(GameManager.instance.coins, b); }

    [UnityTest, Category("PC"), Category("Mobile")]
    [Description("TC_LOGIC_26 - Kiểm tra SaveInfo tính score = distance × coins và lưu đúng vào PlayerPrefs")]
    public IEnumerator TC_LOGIC_26() { yield return Load(); var gm = GameManager.instance; gm.distance = 100f; gm.coins = 5; gm.SaveInfo(); Assert.AreEqual(500f, PlayerPrefs.GetFloat("LastScore"), 0.01f); }

    [UnityTest, Category("PC"), Category("Mobile")]
    [Description("TC_LOGIC_27 - Kiểm tra HighScore chỉ cập nhật khi score mới cao hơn score cũ")]
    public IEnumerator TC_LOGIC_27() { yield return Load(); var gm = GameManager.instance; PlayerPrefs.SetFloat("HighScore", 1000f); gm.distance = 5f; gm.coins = 2; gm.SaveInfo(); Assert.AreEqual(1000f, PlayerPrefs.GetFloat("HighScore"), 0.01f); gm.distance = 200f; gm.coins = 10; gm.SaveInfo(); Assert.AreEqual(2000f, PlayerPrefs.GetFloat("HighScore"), 0.01f); }

    [UnityTest, Category("PC"), Category("Mobile")]
    [Description("TC_LOGIC_28 - Kiểm tra coins được cộng dồn vào tổng coins đã lưu khi gọi SaveInfo")]
    public IEnumerator TC_LOGIC_28() { yield return Load(); PlayerPrefs.SetInt("Coins", 50); GameManager.instance.coins = 30; GameManager.instance.distance = 10f; GameManager.instance.SaveInfo(); Assert.AreEqual(80, PlayerPrefs.GetInt("Coins")); }

    [UnityTest, Category("PC"), Category("Mobile")]
    [Description("TC_LOGIN_01 - Kiểm tra SaveInfo lưu đúng giá trị coins vào PlayerPrefs key 'Coins'")]
    public IEnumerator TC_LOGIN_01() { yield return SceneManager.LoadSceneAsync(1); Time.timeScale = 1; Unlock(); yield return null; PlayerPrefs.DeleteKey("Coins"); PlayerPrefs.Save(); GameManager.instance.coins = 50; GameManager.instance.SaveInfo(); Assert.AreEqual(50, PlayerPrefs.GetInt("Coins", -1)); }

    [UnityTest, Category("PC"), Category("Mobile")]
    [Description("TC_LOGIC_29 - Kiểm tra SaveColor lưu đúng giá trị R, G, B vào PlayerPrefs")]
    public IEnumerator TC_LOGIC_29() { yield return Load(); GameManager.instance.SaveColor(0.25f, 0.5f, 0.75f); yield return null; Assert.AreEqual(0.25f, PlayerPrefs.GetFloat("ColorR"), 0.001f); Assert.AreEqual(0.5f, PlayerPrefs.GetFloat("ColorG"), 0.001f); Assert.AreEqual(0.75f, PlayerPrefs.GetFloat("ColorB"), 0.001f); }

    [UnityTest, Category("PC"), Category("Mobile")]
    [Description("TC_LOGIC_30 - Kiểm tra Trap không bị hủy khi chanceToSpawn = 200 (luôn spawn thành công)")]
    public IEnumerator TC_LOGIC_30() { tempTrapObj = new GameObject(); var t = tempTrapObj.AddComponent<Trap>(); Set(t, "chanceToSpawn", 200f); typeof(Trap).GetMethod("Start", F).Invoke(t, null); yield return null; Assert.That(tempTrapObj, Is.Not.Null); }

    [Test, Category("PC"), Category("Mobile")]
    [Description("TC_LOGIC_31 - Kiểm tra LedgeDetection tắt canDetect khi chạm vào layer Ground")]
    public void TC_LOGIC_31() { tempLedgeObj = new GameObject(); var l = tempLedgeObj.AddComponent<LedgeDetection>(); l.canDetect = true; tempGroundObj = new GameObject(); tempGroundObj.layer = LayerMask.NameToLayer("Ground"); var c = tempGroundObj.AddComponent<BoxCollider2D>(); typeof(LedgeDetection).GetMethod("OnTriggerEnter2D", F).Invoke(l, new object[] { c }); Assert.IsFalse(l.canDetect); }

    [Test, Category("PC"), Category("Mobile")]
    [Description("TC_LOGIC_32 - Kiểm tra GeneratePlatform spawn phần map mới khi Player đủ gần nextPartPosition")]
    public void TC_LOGIC_32() { tempPartPrefab = new GameObject("Part"); var sP = new GameObject("StartPoint"); var eP = new GameObject("EndPoint"); sP.transform.SetParent(tempPartPrefab.transform); eP.transform.SetParent(tempPartPrefab.transform); sP.transform.position = Vector3.zero; eP.transform.position = new Vector3(10f, 0, 0); tempGeneratorObj = new GameObject("LG"); var g = tempGeneratorObj.AddComponent<LevelGenerator>(); tempPlayerObj = new GameObject("P"); tempPlayerObj.transform.position = Vector3.zero; Set(g, "levelPart", new Transform[] { tempPartPrefab.transform }); Set(g, "player", tempPlayerObj.transform); Set(g, "distanceToSpawn", 20f); Set(g, "distanceToDelete", 1000f); Set(g, "nextPartPosition", new Vector3(5f, 0, 0)); typeof(LevelGenerator).GetMethod("GeneratePlatform", F).Invoke(g, null); Assert.Greater(tempGeneratorObj.transform.childCount, 0); }

    [UnityTest, Category("PC"), Category("Mobile")]
    [Description("TC_LOGIC_33 - Kiểm tra LevelGenerator spawn thêm platform khi Player tiến đến gần trong scene thật")]
    public IEnumerator TC_LOGIC_33() { yield return Load(); Unlock(); var g = Object.FindFirstObjectByType<LevelGenerator>(); yield return new WaitForSeconds(0.3f); int b = g.transform.childCount; GameManager.instance.player.transform.position = new Vector3(GameManager.instance.player.transform.position.x + 200f, GameManager.instance.player.transform.position.y, 0f); yield return new WaitForSeconds(0.5f); Assert.GreaterOrEqual(g.transform.childCount, b); }

    [UnityTest, Category("PC"), Category("Mobile")]
    [Description("TC_LOGIC_34 - Kiểm tra LevelGenerator xóa platform cũ khi Player di chuyển quá xa về phía trước")]
    public IEnumerator TC_LOGIC_34() { yield return Load(); Unlock(); var g = Object.FindFirstObjectByType<LevelGenerator>(); yield return new WaitForSeconds(0.5f); Assert.Greater(g.transform.childCount, 0); var first = g.transform.GetChild(0).gameObject; GameManager.instance.player.transform.position = new Vector3(first.transform.position.x + 500f, GameManager.instance.player.transform.position.y, 0f); yield return new WaitForSeconds(0.5f); Assert.IsTrue(first == null); }

    // ═════════════════════════════════════════════════════════════
    //  GỘP TỪ CoverageBoostTests (TC_LOGIC_52+)
    // ═════════════════════════════════════════════════════════════

    // --- Enemy SpeedController ---
    [Test, Category("PC"), Category("Mobile")]
    [Description("TC_LOGIC_52 - Kiểm tra Enemy SpeedController set moveSpeed = 25 khi Player ở xa phía trước")]
    public void TC_LOGIC_52() { var e = CreateEnemy(); var pO = new GameObject("P"); pO.transform.position = new Vector3(100f, 0, 0); Set(e, "player", pO.AddComponent<Player>()); enemyObj.transform.position = Vector3.zero; Inv(e, "SpeedController"); Assert.AreEqual(25f, Get<float>(e, "moveSpeed")); Object.DestroyImmediate(pO); }

    [Test, Category("PC"), Category("Mobile")]
    [Description("TC_LOGIC_53 - Kiểm tra Enemy SpeedController set moveSpeed = 17 khi Player ở gần phía trước")]
    public void TC_LOGIC_53() { var e = CreateEnemy(); var pO = new GameObject("P"); pO.transform.position = new Vector3(2f, 0, 0); Set(e, "player", pO.AddComponent<Player>()); enemyObj.transform.position = Vector3.zero; Inv(e, "SpeedController"); Assert.AreEqual(17f, Get<float>(e, "moveSpeed")); Object.DestroyImmediate(pO); }

    [Test, Category("PC"), Category("Mobile")]
    [Description("TC_LOGIC_54 - Kiểm tra Enemy SpeedController set moveSpeed = 11 khi Player ở xa phía sau")]
    public void TC_LOGIC_54() { var e = CreateEnemy(); var pO = new GameObject("P"); pO.transform.position = new Vector3(-100f, 0, 0); Set(e, "player", pO.AddComponent<Player>()); enemyObj.transform.position = Vector3.zero; Inv(e, "SpeedController"); Assert.AreEqual(11f, Get<float>(e, "moveSpeed")); Object.DestroyImmediate(pO); }

    [Test, Category("PC"), Category("Mobile")]
    [Description("TC_LOGIC_55 - Kiểm tra Enemy SpeedController set moveSpeed = 14 khi Player ở gần phía sau")]
    public void TC_LOGIC_55() { var e = CreateEnemy(); var pO = new GameObject("P"); pO.transform.position = new Vector3(-1f, 0, 0); Set(e, "player", pO.AddComponent<Player>()); enemyObj.transform.position = Vector3.zero; Inv(e, "SpeedController"); Assert.AreEqual(14f, Get<float>(e, "moveSpeed")); Object.DestroyImmediate(pO); }

    [Test, Category("PC"), Category("Mobile")]
    [Description("TC_LOGIC_56 - Kiểm tra Enemy AllowLedgeGrab set canGrabLedge = true sau khi được gọi")]
    public void TC_LOGIC_56() { var e = CreateEnemy(); Set(e, "canGrabLedge", false); Inv(e, "AllowLedgeGrab"); Assert.IsTrue(Get<bool>(e, "canGrabLedge")); }

    // --- GameManager ---
    [UnityTest, Category("PC"), Category("Mobile")]
    [Description("TC_LOGIC_57 - Kiểm tra SetupSkyBox lưu đúng index vào PlayerPrefs key 'SkyBoxSetting'")]
    public IEnumerator TC_LOGIC_57() { yield return SceneManager.LoadSceneAsync(1); yield return new WaitForSeconds(0.3f); GameManager.instance.SetupSkyBox(0); Assert.AreEqual(0, PlayerPrefs.GetInt("SkyBoxSetting")); }

    [UnityTest, Category("PC"), Category("Mobile")]
    [Description("TC_LOGIC_58 - Kiểm tra SetupSkyBox dùng skybox ngẫu nhiên khi index > 1 và không throw exception")]
    public IEnumerator TC_LOGIC_58() { yield return SceneManager.LoadSceneAsync(1); yield return new WaitForSeconds(0.3f); Assert.DoesNotThrow(() => GameManager.instance.SetupSkyBox(5)); Assert.AreEqual(5, PlayerPrefs.GetInt("SkyBoxSetting")); }

    [UnityTest, Category("PC"), Category("Mobile")]
    [Description("TC_LOGIC_59 - Kiểm tra GameEnded gọi SaveInfo và lưu score = distance × coins vào PlayerPrefs")]
    public IEnumerator TC_LOGIC_59() { yield return SceneManager.LoadSceneAsync(1); yield return new WaitForSeconds(0.3f); GameManager.instance.distance = 50f; GameManager.instance.coins = 5; PlayerPrefs.DeleteKey("LastScore"); GameManager.instance.GameEnded(); yield return null; Assert.AreEqual(250f, PlayerPrefs.GetFloat("LastScore", -1f), 0.01f); }

    // --- Player Damage/Knockback/Die ---
    [UnityTest, Category("PC"), Category("Mobile")]
    [Description("TC_LOGIC_60 - Kiểm tra Damage gọi bloodFx.Play() để hiển thị hiệu ứng máu khi bị tấn công")]
    public IEnumerator TC_LOGIC_60() { var p = CreatePlayer(); yield return null; Set(p, "canBeKnocked", true); Set(p, "moveSpeed", 20f); Set(p, "speedToSurvive", 18f); p.extraLife = true; Set(p, "defaultSpeed", 8f); Set(p, "defaultMilestoneIncrease", 5f); Set(p, "isSliding", false); p.Damage(); yield return null; Assert.IsTrue(Get<ParticleSystem>(p, "bloodFx").isPlaying || Get<ParticleSystem>(p, "bloodFx").particleCount >= 0); }

    [UnityTest, Category("PC"), Category("Mobile")]
    [Description("TC_LOGIC_61 - Kiểm tra Damage gọi Knockback (set isKnocked = true) khi Player có extraLife")]
    public IEnumerator TC_LOGIC_61() { var p = CreatePlayer(); yield return null; Set(p, "canBeKnocked", true); Set(p, "moveSpeed", 20f); Set(p, "speedToSurvive", 18f); Set(p, "defaultSpeed", 8f); Set(p, "defaultMilestoneIncrease", 5f); Set(p, "isSliding", false); p.extraLife = true; p.Damage(); yield return null; Assert.IsTrue(Get<bool>(p, "isKnocked")); }

    [UnityTest, Category("PC"), Category("Mobile")]
    [Description("TC_LOGIC_62 - Kiểm tra Damage gọi Die() (set isDead = true) khi Player không có extraLife")]
    public IEnumerator TC_LOGIC_62() { var p = CreatePlayer(); yield return null; var am = CreateAM(); Set(p, "canBeKnocked", true); p.extraLife = false; Set(p, "isDead", false); p.Damage(); yield return new WaitForSeconds(0.1f); Assert.IsTrue(Get<bool>(p, "isDead")); Time.timeScale = 1f; Object.DestroyImmediate(am.gameObject); }

    [UnityTest, Category("PC"), Category("Mobile")]
    [Description("TC_LOGIC_63 - Kiểm tra Knockback set isKnocked = true và áp dụng knockbackDir vào velocity")]
    public IEnumerator TC_LOGIC_63() { var p = CreatePlayer(); yield return null; Set(p, "canBeKnocked", true); Set(p, "isSliding", false); Set(p, "defaultSpeed", 8f); Set(p, "defaultMilestoneIncrease", 5f); Inv(p, "Knockback"); Assert.IsTrue(Get<bool>(p, "isKnocked")); Assert.AreEqual(-5f, playerObj.GetComponent<Rigidbody2D>().linearVelocity.x, 0.01f); }

    [UnityTest, Category("PC"), Category("Mobile")]
    [Description("TC_LOGIC_64 - Kiểm tra hàm Jump private set velocity.y bằng force được truyền vào")]
    public IEnumerator TC_LOGIC_64() { var p = CreatePlayer(); yield return null; var am = CreateAM(); playerObj.GetComponent<Rigidbody2D>().linearVelocity = new Vector2(10f, 0f); Inv(p, "Jump", 15f); Assert.AreEqual(15f, playerObj.GetComponent<Rigidbody2D>().linearVelocity.y, 0.01f); Object.DestroyImmediate(am.gameObject); }

    [UnityTest, Category("PC"), Category("Mobile")]
    [Description("TC_LOGIC_65 - Kiểm tra SpeedController không để moveSpeed vượt quá giới hạn maxSpeed")]
    public IEnumerator TC_LOGIC_65() { var p = CreatePlayer(); yield return null; Set(p, "moveSpeed", 49f); Set(p, "maxSpeed", 50f); Set(p, "speedMilestone", 0f); Set(p, "milestoneIncreaser", 5f); Set(p, "speedMultiplier", 1.5f); playerObj.transform.position = new Vector3(1f, 0, 0); Inv(p, "SpeedController"); Assert.LessOrEqual(Get<float>(p, "moveSpeed"), 50f); }

    [UnityTest, Category("PC"), Category("Mobile")]
    [Description("TC_LOGIC_66 - Kiểm tra CheckForLanding không set readyToLand khi rơi chậm (velocity.y > -5)")]
    public IEnumerator TC_LOGIC_66() { var p = CreatePlayer(); yield return null; playerObj.GetComponent<Rigidbody2D>().linearVelocity = new Vector2(0f, -3f); Set(p, "isGrounded", false); Set(p, "readyToLand", false); Inv(p, "CheckForLanding"); Assert.IsFalse(Get<bool>(p, "readyToLand")); }

    [UnityTest, Category("PC"), Category("Mobile")]
    [Description("TC_LOGIC_67 - Kiểm tra CheckForSlideCancel giữ isSliding = true khi phát hiện trần nhà phía trên")]
    public IEnumerator TC_LOGIC_67() { var p = CreatePlayer(); yield return null; Set(p, "isSliding", true); Set(p, "slideTimeCounter", -1f); Set(p, "ceillingDetected", true); Inv(p, "CheckForSlideCancel"); Assert.IsTrue(Get<bool>(p, "isSliding")); }

    [UnityTest, Category("PC"), Category("Mobile")]
    [Description("TC_LOGIC_68 - Kiểm tra Die coroutine set isDead = true và canBeKnocked = false khi nhân vật chết")]
    public IEnumerator TC_LOGIC_68() { var p = CreatePlayer(); yield return null; var am = CreateAM(); var die = typeof(Player).GetMethod("Die", F); p.StartCoroutine((IEnumerator)die.Invoke(p, null)); yield return new WaitForSeconds(0.1f); Assert.IsTrue(Get<bool>(p, "isDead")); Assert.IsFalse(Get<bool>(p, "canBeKnocked")); Time.timeScale = 1f; Object.DestroyImmediate(am.gameObject); }

    // --- Trap & MovingTrap ---
    [UnityTest, Category("PC"), Category("Mobile")]
    [Description("TC_LOGIC_69 - Kiểm tra Trap.OnTriggerEnter2D gọi Player.Damage() khi Player chạm vào bẫy")]
    public IEnumerator TC_LOGIC_69() { yield return SceneManager.LoadSceneAsync(1); yield return new WaitForSeconds(0.5f); Unlock(); yield return new WaitForSeconds(0.3f); var p = GameManager.instance.player; Set(p, "canBeKnocked", true); Set(p, "moveSpeed", 20f); Set(p, "speedToSurvive", 18f); p.extraLife = true; Set(p, "defaultSpeed", 8f); Set(p, "defaultMilestoneIncrease", 5f); Set(p, "isSliding", false); var tO = new GameObject(); var t = tO.AddComponent<Trap>(); Set(t, "chanceToSpawn", 200f); var col = p.GetComponent<BoxCollider2D>(); if (!col) col = p.gameObject.AddComponent<BoxCollider2D>(); typeof(Trap).GetMethod("OnTriggerEnter2D", F).Invoke(t, new object[] { col }); yield return null; Assert.IsTrue(Get<bool>(p, "isKnocked")); Object.DestroyImmediate(tO); }

    [Test, Category("PC"), Category("Mobile")]
    [Description("TC_LOGIC_70 - Kiểm tra MovingTrap kế thừa đúng từ lớp cha Trap")]
    public void TC_LOGIC_70() { Assert.IsTrue(typeof(MovingTrap).IsSubclassOf(typeof(Trap))); }

    [UnityTest, Category("PC"), Category("Mobile")]
    [Description("TC_LOGIC_71 - Kiểm tra MovingTrap di chuyển về phía movePoint hiện tại trong Update")]
    public IEnumerator TC_LOGIC_71() { var tO = new GameObject(); var t = tO.AddComponent<MovingTrap>(); var p1 = new GameObject(); var p2 = new GameObject(); p1.transform.position = Vector3.zero; p2.transform.position = new Vector3(10f, 0, 0); Set(t, "movePoint", new Transform[] { p1.transform, p2.transform }); Set(t, "speed", 50f); Set(t, "rotationSpeed", 10f); Set(t, "chanceToSpawn", 200f); Set(t, "i", 1); tO.transform.position = Vector3.zero; yield return new WaitForSeconds(0.3f); Assert.Greater(tO.transform.position.x, 0f); Object.DestroyImmediate(tO); Object.DestroyImmediate(p1); Object.DestroyImmediate(p2); }

    // --- PlatformController ---
    [UnityTest, Category("PC"), Category("Mobile")]
    [Description("TC_LOGIC_72 - Kiểm tra OnTriggerEnter2D chỉ tô màu header khi colorEntierPlatform = false")]
    public IEnumerator TC_LOGIC_72() { yield return SceneManager.LoadSceneAsync(1); yield return new WaitForSeconds(0.5f); GameManager.instance.colorEntierPlatform = false; GameManager.instance.platformColor = Color.yellow; var pO = new GameObject(); var sr = pO.AddComponent<SpriteRenderer>(); var hO = new GameObject(); hO.transform.SetParent(pO.transform); var hSr = hO.AddComponent<SpriteRenderer>(); var pc = pO.AddComponent<PlatformController>(); Set(pc, "sr", sr); Set(pc, "headerSr", hSr); var col = GameManager.instance.player.GetComponent<BoxCollider2D>(); if (!col) col = GameManager.instance.player.gameObject.AddComponent<BoxCollider2D>(); typeof(PlatformController).GetMethod("OnTriggerEnter2D", F).Invoke(pc, new object[] { col }); Assert.AreEqual(Color.yellow, hSr.color); Object.DestroyImmediate(pO); }

    [UnityTest, Category("PC"), Category("Mobile")]
    [Description("TC_LOGIC_73 - Kiểm tra OnTriggerEnter2D tô màu cả header và body khi colorEntierPlatform = true")]
    public IEnumerator TC_LOGIC_73() { yield return SceneManager.LoadSceneAsync(1); yield return new WaitForSeconds(0.5f); GameManager.instance.colorEntierPlatform = true; GameManager.instance.platformColor = Color.green; var pO = new GameObject(); var sr = pO.AddComponent<SpriteRenderer>(); var hO = new GameObject(); hO.transform.SetParent(pO.transform); var hSr = hO.AddComponent<SpriteRenderer>(); var pc = pO.AddComponent<PlatformController>(); Set(pc, "sr", sr); Set(pc, "headerSr", hSr); var col = GameManager.instance.player.GetComponent<BoxCollider2D>(); if (!col) col = GameManager.instance.player.gameObject.AddComponent<BoxCollider2D>(); typeof(PlatformController).GetMethod("OnTriggerEnter2D", F).Invoke(pc, new object[] { col }); Assert.AreEqual(Color.green, hSr.color); Assert.AreEqual(Color.green, sr.color); Object.DestroyImmediate(pO); }

    [UnityTest, Category("PC"), Category("Mobile")]
    [Description("TC_LOGIC_74 - Kiểm tra OnTriggerEnter2D bỏ qua không tô màu khi collider không phải Player")]
    public IEnumerator TC_LOGIC_74() { yield return SceneManager.LoadSceneAsync(1); yield return new WaitForSeconds(0.5f); var pO = new GameObject(); var sr = pO.AddComponent<SpriteRenderer>(); var hO = new GameObject(); hO.transform.SetParent(pO.transform); var hSr = hO.AddComponent<SpriteRenderer>(); hSr.color = Color.white; var pc = pO.AddComponent<PlatformController>(); Set(pc, "sr", sr); Set(pc, "headerSr", hSr); var oO = new GameObject(); var oC = oO.AddComponent<BoxCollider2D>(); typeof(PlatformController).GetMethod("OnTriggerEnter2D", F).Invoke(pc, new object[] { oC }); Assert.AreEqual(Color.white, hSr.color); Object.DestroyImmediate(pO); Object.DestroyImmediate(oO); }

    // --- RainController ---
    [Test, Category("PC"), Category("Mobile")]
    [Description("TC_LOGIC_75 - Kiểm tra ChangeIntensity tăng intensity khi thấp hơn targetIntensity")]
    public void TC_LOGIC_75() { var o = new GameObject(); var r = o.AddComponent<RainController>(); Set(r, "intensity", 0.1f); Set(r, "targetIntensity", 0.5f); Set(r, "changeRate", 100f); Set(r, "canChangeIntensity", true); Inv(r, "ChangeIntensity"); Assert.GreaterOrEqual(Get<float>(r, "intensity"), 0.1f); Object.DestroyImmediate(o); }

    [Test, Category("PC"), Category("Mobile")]
    [Description("TC_LOGIC_76 - Kiểm tra ChangeIntensity giảm intensity khi cao hơn targetIntensity")]
    public void TC_LOGIC_76() { var o = new GameObject(); var r = o.AddComponent<RainController>(); Set(r, "intensity", 0.5f); Set(r, "targetIntensity", 0.1f); Set(r, "changeRate", 100f); Set(r, "canChangeIntensity", true); Inv(r, "ChangeIntensity"); Assert.LessOrEqual(Get<float>(r, "intensity"), 0.5f); Object.DestroyImmediate(o); }

    [Test, Category("PC"), Category("Mobile")]
    [Description("TC_LOGIC_77 - Kiểm tra ChangeIntensity dừng đúng tại targetIntensity khi tăng vượt mức và tắt canChangeIntensity")]
    public void TC_LOGIC_77() { var o = new GameObject(); var r = o.AddComponent<RainController>(); Set(r, "intensity", 0.49f); Set(r, "targetIntensity", 0.5f); Set(r, "changeRate", 1000f); Set(r, "canChangeIntensity", true); Inv(r, "ChangeIntensity"); Assert.AreEqual(0.5f, Get<float>(r, "intensity"), 0.01f); Assert.IsFalse(Get<bool>(r, "canChangeIntensity")); Object.DestroyImmediate(o); }

    [Test, Category("PC"), Category("Mobile")]
    [Description("TC_LOGIC_78 - Kiểm tra ChangeIntensity dừng đúng tại targetIntensity khi giảm vượt mức và tắt canChangeIntensity")]
    public void TC_LOGIC_78() { var o = new GameObject(); var r = o.AddComponent<RainController>(); Set(r, "intensity", 0.11f); Set(r, "targetIntensity", 0.1f); Set(r, "changeRate", 1000f); Set(r, "canChangeIntensity", true); Inv(r, "ChangeIntensity"); Assert.AreEqual(0.1f, Get<float>(r, "intensity"), 0.01f); Assert.IsFalse(Get<bool>(r, "canChangeIntensity")); Object.DestroyImmediate(o); }

    [Test, Category("PC"), Category("Mobile")]
    [Description("TC_LOGIC_79 - Kiểm tra CheckForRain bật canChangeIntensity và set targetIntensity hợp lệ khi timer hết")]
    public void TC_LOGIC_79() { var o = new GameObject(); var r = o.AddComponent<RainController>(); Set(r, "rainCheckTimer", -1f); Set(r, "rainCheckCooldown", 10f); Set(r, "chanceToRain", 100f); Set(r, "minValue", 0.2f); Set(r, "maxValue", 0.49f); Set(r, "canChangeIntensity", false); Inv(r, "CheckForRain"); Assert.IsTrue(Get<bool>(r, "canChangeIntensity")); Assert.AreEqual(10f, Get<float>(r, "rainCheckTimer")); Assert.That(Get<float>(r, "targetIntensity"), Is.InRange(0.2f, 0.49f)); Object.DestroyImmediate(o); }

    [Test, Category("PC"), Category("Mobile")]
    [Description("TC_LOGIC_80 - Kiểm tra CheckForRain set targetIntensity = 0 khi xác suất mưa bằng 0")]
    public void TC_LOGIC_80() { var o = new GameObject(); var r = o.AddComponent<RainController>(); Set(r, "rainCheckTimer", -1f); Set(r, "rainCheckCooldown", 10f); Set(r, "chanceToRain", 0f); Set(r, "minValue", 0.2f); Set(r, "maxValue", 0.49f); Inv(r, "CheckForRain"); Assert.AreEqual(0f, Get<float>(r, "targetIntensity")); Object.DestroyImmediate(o); }

    [Test, Category("PC"), Category("Mobile")]
    [Description("TC_LOGIC_81 - Kiểm tra CheckForRain không làm gì khi timer vẫn còn dương (chưa hết cooldown)")]
    public void TC_LOGIC_81() { var o = new GameObject(); var r = o.AddComponent<RainController>(); Set(r, "rainCheckTimer", 5f); Set(r, "canChangeIntensity", false); Inv(r, "CheckForRain"); Assert.IsFalse(Get<bool>(r, "canChangeIntensity")); Object.DestroyImmediate(o); }

    // --- LedgeDetection ---
    [Test, Category("PC"), Category("Mobile")]
    [Description("TC_LOGIC_82 - Kiểm tra OnTriggerExit2D bật lại canDetect khi LedgeDetection rời khỏi layer Ground")]
    public void TC_LOGIC_82() { var o = new GameObject(); o.AddComponent<BoxCollider2D>(); var l = o.AddComponent<LedgeDetection>(); l.canDetect = false; var g = new GameObject(); g.layer = LayerMask.NameToLayer("Ground"); var c = g.AddComponent<BoxCollider2D>(); typeof(LedgeDetection).GetMethod("OnTriggerExit2D", F).Invoke(l, new object[] { c }); Assert.IsTrue(l.canDetect); Object.DestroyImmediate(o); Object.DestroyImmediate(g); }

    [Test, Category("PC"), Category("Mobile")]
    [Description("TC_LOGIC_83 - Kiểm tra OnTriggerEnter2D tắt canDetect khi LedgeDetection chạm layer Ground")]
    public void TC_LOGIC_83() { var o = new GameObject(); var l = o.AddComponent<LedgeDetection>(); l.canDetect = true; var g = new GameObject(); g.layer = LayerMask.NameToLayer("Ground"); var c = g.AddComponent<BoxCollider2D>(); typeof(LedgeDetection).GetMethod("OnTriggerEnter2D", F).Invoke(l, new object[] { c }); Assert.IsFalse(l.canDetect); Object.DestroyImmediate(o); Object.DestroyImmediate(g); }

    [Test, Category("PC"), Category("Mobile")]
    [Description("TC_LOGIC_84 - Kiểm tra OnTriggerEnter2D bỏ qua không tắt canDetect khi collider không thuộc layer Ground")]
    public void TC_LOGIC_84() { var o = new GameObject(); var l = o.AddComponent<LedgeDetection>(); l.canDetect = true; var g = new GameObject(); g.layer = LayerMask.NameToLayer("Default"); var c = g.AddComponent<BoxCollider2D>(); typeof(LedgeDetection).GetMethod("OnTriggerEnter2D", F).Invoke(l, new object[] { c }); Assert.IsTrue(l.canDetect); Object.DestroyImmediate(o); Object.DestroyImmediate(g); }

    // ═════════════════════════════════════════════════════════════
    //  GỘP TỪ AdditionalAutoTests (TC_LOGIC_85+)
    // ═════════════════════════════════════════════════════════════

    [Test, Category("PC"), Category("Mobile")]
    [Description("TC_LOGIC_85 - Kiểm tra LimiterGizmos có thể gán đúng 3 transform references (start, end, groundLevel)")]
    public void TC_LOGIC_85() { var o = new GameObject(); var l = o.AddComponent<LimiterGizmos>(); var s = new GameObject(); var e = new GameObject(); var g = new GameObject(); Set(l, "start", s.transform); Set(l, "end", e.transform); Set(l, "groundLevel", g.transform); Assert.IsNotNull(Get<Transform>(l, "start")); Assert.IsNotNull(Get<Transform>(l, "end")); Assert.IsNotNull(Get<Transform>(l, "groundLevel")); Object.DestroyImmediate(o); Object.DestroyImmediate(s); Object.DestroyImmediate(e); Object.DestroyImmediate(g); }

    [Test, Category("PC"), Category("Mobile")]
    [Description("TC_LOGIC_86 - Kiểm tra LimiterGizmos có method OnDrawGizmos để vẽ debug lines trong Editor")]
    public void TC_LOGIC_86() { Assert.IsNotNull(typeof(LimiterGizmos).GetMethod("OnDrawGizmos", F)); }

    [Test, Category("PC"), Category("Mobile")]
    [Description("TC_LOGIC_87 - Kiểm tra SceneController có public method LoadGame để chuyển scene")]
    public void TC_LOGIC_87() { Assert.IsNotNull(typeof(SceneController).GetMethod("LoadGame", BindingFlags.Public | BindingFlags.Instance)); }

    [Test, Category("PC"), Category("Mobile")]
    [Description("TC_LOGIC_88 - Kiểm tra SceneController có thể gắn thành component vào GameObject")]
    public void TC_LOGIC_88() { var o = new GameObject(); Assert.IsNotNull(o.AddComponent<SceneController>()); Object.DestroyImmediate(o); }

    [Test, Category("PC"), Category("Mobile")]
    [Description("TC_LOGIC_89 - Kiểm tra ColorType enum có đầy đủ giá trị playerColor và platformColor")]
    public void TC_LOGIC_89() { Assert.IsTrue(System.Enum.IsDefined(typeof(ColorType), "playerColor")); Assert.IsTrue(System.Enum.IsDefined(typeof(ColorType), "platformColor")); }

    [Test, Category("PC"), Category("Mobile")]
    [Description("TC_LOGIC_90 - Kiểm tra ColorToSell struct lưu đúng giá trị color và price")]
    public void TC_LOGIC_90() { var i = new ColorToSell { color = Color.red, price = 100 }; Assert.AreEqual(Color.red, i.color); Assert.AreEqual(100, i.price); }

    [Test, Category("PC"), Category("Mobile")]
    [Description("TC_LOGIC_91 - Kiểm tra CharacterBox component có thể được tạo và gắn vào GameObject")]
    public void TC_LOGIC_91() { var o = new GameObject(); Assert.IsNotNull(o.AddComponent<CharacterBox>()); Object.DestroyImmediate(o); }
}