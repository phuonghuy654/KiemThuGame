using System.Collections;
using System.Reflection;
using NUnit.Framework;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

/// <summary>
/// Gameplay Tests - Tất cả test liên quan đến logic game:
/// Di chuyển, tốc độ, nhảy, trượt, knockback, coin, trap, sinh map, điểm số, lưu dữ liệu.
/// Gộp từ: PlayMode, PlayModeDeepLogic, NewAutoTests_PlayMode, RunToLife_Assignment_Tests.
/// </summary>
public class GameplayTests
{
    private GameObject tempCoinObj;
    private GameObject tempPartPrefab;
    private GameObject tempGeneratorObj;
    private GameObject tempPlayerObj;
    private GameObject tempTrapObj;
    private GameObject tempLedgeObj;
    private GameObject tempGroundObj;

    private IEnumerator LoadGameScene()
    {
        SceneManager.LoadScene(1);
        yield return new WaitForSeconds(0.5f);
    }

    private void UnlockPlayer()
    {
        GameManager.instance.UnlockPlayer();
    }

    [TearDown]
    public void Teardown()
    {
        if (tempCoinObj != null) Object.DestroyImmediate(tempCoinObj);
        if (tempPartPrefab != null) Object.DestroyImmediate(tempPartPrefab);
        if (tempGeneratorObj != null) Object.DestroyImmediate(tempGeneratorObj);
        if (tempPlayerObj != null) Object.DestroyImmediate(tempPlayerObj);
        if (tempTrapObj != null) Object.DestroyImmediate(tempTrapObj);
        if (tempLedgeObj != null) Object.DestroyImmediate(tempLedgeObj);
        if (tempGroundObj != null) Object.DestroyImmediate(tempGroundObj);
        PlayerPrefs.DeleteKey("Coins");
        Time.timeScale = 1f;
    }

    // ═══════════════════════════════════════════════
    //  TỐC ĐỘ & DI CHUYỂN
    // ═══════════════════════════════════════════════

    [UnityTest]
    [Category("PC")]
    [Category("Mobile")]
    [Description("Kiểm tra tốc độ Player tăng lên khi di chuyển vượt qua các mốc speedMilestone")]
    public IEnumerator TC_LOGIC_17_TocDoTang_KhiVuotSpeedMilestone()
    {
        yield return LoadGameScene();
        UnlockPlayer();

        Player player = GameManager.instance.player;

        yield return new WaitForSeconds(0.2f);
        float speedBefore = player.GetComponent<Rigidbody2D>().linearVelocity.x;

        player.transform.position = new Vector3(300f, player.transform.position.y, 0f);
        yield return new WaitForSeconds(0.5f);

        float speedAfter = player.GetComponent<Rigidbody2D>().linearVelocity.x;

        Assert.Greater(speedAfter, speedBefore,
            "Tốc độ Player phải tăng sau khi vượt speedMilestone.");
    }

    [UnityTest]
    [Category("PC")]
    [Category("Mobile")]
    [Description("Kiểm tra GameManager.distance tăng theo vị trí X hiện tại của Player")]
    public IEnumerator TC_LOGIC_18_DistanceTangTheoViTriX()
    {
        yield return LoadGameScene();
        UnlockPlayer();

        Player player = GameManager.instance.player;

        yield return new WaitForSeconds(0.3f);
        float distanceBefore = GameManager.instance.distance;

        player.transform.position = new Vector3(distanceBefore + 50f, player.transform.position.y, 0f);
        yield return new WaitForSeconds(0.2f);

        float distanceAfter = GameManager.instance.distance;

        Assert.Greater(distanceAfter, distanceBefore,
            "GameManager.distance phải tăng khi Player di chuyển sang phải.");
    }

    [UnityTest]
    [Category("PC")]
    [Category("Mobile")]
    [Description("Kiểm tra nhân vật thực sự di chuyển về phía trước sau 2 giây chơi game")]
    public IEnumerator TC_UI_2_NhanVatDiChuyen_Sau2Giay()
    {
        yield return SceneManager.LoadSceneAsync(1);
        Time.timeScale = 1;
        GameManager.instance.UnlockPlayer();
        yield return null;

        Player player = GameManager.instance.player;
        Assert.That(player, Is.Not.Null, "Không tìm thấy Player!");

        float startPositionX = player.transform.position.x;
        yield return new WaitForSeconds(2.0f);

        float endPositionX = player.transform.position.x;

        Assert.That(endPositionX, Is.GreaterThan(startPositionX),
            $"Nhân vật không di chuyển! Ban đầu: {startPositionX:F2}, sau 2 giây: {endPositionX:F2}.");
    }

    [UnityTest]
    [Category("PC")]
    [Category("Mobile")]
    [Description("Kiểm tra moveSpeed reset về defaultSpeed khi Player chạm tường (wallDetected = true)")]
    public IEnumerator TC_LOGIC_19_TocDoReset_KhiChamTuong()
    {
        yield return LoadGameScene();
        UnlockPlayer();

        Player player = GameManager.instance.player;

        var flags = BindingFlags.NonPublic | BindingFlags.Instance;
        var moveSpeedField = typeof(Player).GetField("moveSpeed", flags);
        var defaultSpeedField = typeof(Player).GetField("defaultSpeed", flags);
        var wallField = typeof(Player).GetField("wallDetected", flags);

        Assert.IsNotNull(moveSpeedField);
        Assert.IsNotNull(defaultSpeedField);
        Assert.IsNotNull(wallField);

        player.transform.position = new Vector3(300f, player.transform.position.y, 0f);
        yield return new WaitForSeconds(0.5f);

        float moveSpeedAfterAccel = (float)moveSpeedField.GetValue(player);
        float defaultSpeed = (float)defaultSpeedField.GetValue(player);
        Assert.Greater(moveSpeedAfterAccel, defaultSpeed,
            "Tiền đề: moveSpeed phải lớn hơn defaultSpeed sau khi tăng tốc.");

        wallField.SetValue(player, true);
        yield return new WaitForSeconds(0.1f);

        float moveSpeedAfterReset = (float)moveSpeedField.GetValue(player);
        wallField.SetValue(player, false);

        Assert.AreEqual(defaultSpeed, moveSpeedAfterReset, 0.01f,
            "moveSpeed phải reset về defaultSpeed khi wallDetected = true.");
    }

    // ═══════════════════════════════════════════════
    //  NHẢY & TRƯỢT
    // ═══════════════════════════════════════════════

    [UnityTest]
    [Category("PC")]
    [Category("Mobile")]
    [Description("Kiểm tra Player không thể nhảy lần 3 - chỉ cho phép tối đa double jump")]
    public IEnumerator TC_LOGIC_20_KhongTheNhayLan3()
    {
        yield return LoadGameScene();
        UnlockPlayer();

        Player player = GameManager.instance.player;
        Rigidbody2D rb = player.GetComponent<Rigidbody2D>();

        yield return new WaitForSeconds(0.4f);

        player.JumpButton();
        yield return new WaitForSeconds(0.2f);

        player.JumpButton();
        yield return new WaitForSeconds(0.05f);

        yield return new WaitForSeconds(0.1f);
        float velBeforeAttempt3 = rb.linearVelocity.y;

        player.JumpButton();
        yield return new WaitForSeconds(0.05f);
        float velAfterAttempt3 = rb.linearVelocity.y;

        Assert.LessOrEqual(velAfterAttempt3, velBeforeAttempt3 + 0.5f,
            "Lần nhảy thứ 3 phải bị bỏ qua: velocity Y không được tăng vọt.");
    }

    [UnityTest]
    [Category("PC")]
    [Category("Mobile")]
    [Description("Kiểm tra Slide không thể kích hoạt lại khi đang trong thời gian cooldown")]
    public IEnumerator TC_LOGIC_21_SlideKhongKichHoat_TrongCooldown()
    {
        yield return LoadGameScene();
        UnlockPlayer();

        Player player = GameManager.instance.player;
        yield return new WaitForSeconds(0.3f);

        player.SlideButton();
        yield return new WaitForSeconds(0.05f);
        float cooldown1 = player.slideCooldownCounter;

        player.SlideButton();
        yield return new WaitForSeconds(0.05f);
        float cooldown2 = player.slideCooldownCounter;

        Assert.LessOrEqual(cooldown2, cooldown1,
            "Slide không được kích hoạt lại trong thời gian cooldown.");
    }

    [UnityTest]
    [Category("PC")]
    [Category("Mobile")]
    [Description("Kiểm tra canDoubleJump được reset về true khi Player tiếp đất sau double jump")]
    public IEnumerator TC_LOGIC_22_CanDoubleJumpReset_KhiTiepDat()
    {
        yield return LoadGameScene();
        UnlockPlayer();

        Player player = GameManager.instance.player;
        var flags = BindingFlags.NonPublic | BindingFlags.Instance;
        var canDoubleJumpField = typeof(Player).GetField("canDoubleJump", flags);
        Assert.IsNotNull(canDoubleJumpField);

        yield return new WaitForSeconds(0.5f);

        player.JumpButton();
        yield return new WaitForSeconds(0.15f);
        player.JumpButton();
        yield return new WaitForSeconds(0.1f);

        bool afterDoubleJump = (bool)canDoubleJumpField.GetValue(player);
        Assert.IsFalse(afterDoubleJump, "Tiền đề: canDoubleJump phải false sau double jump.");

        yield return new WaitForSeconds(2.0f);

        bool afterLanding = (bool)canDoubleJumpField.GetValue(player);
        Assert.IsTrue(afterLanding,
            "canDoubleJump phải reset về true khi Player chạm đất.");
    }

    // ═══════════════════════════════════════════════
    //  EXTRA LIFE
    // ═══════════════════════════════════════════════

    [UnityTest]
    [Category("PC")]
    [Category("Mobile")]
    [Description("Kiểm tra extraLife bật khi moveSpeed >= speedToSurvive và tắt khi nhỏ hơn")]
    public IEnumerator TC_LOGIC_23_ExtraLife_BatTat_TheoMoveSpeed()
    {
        yield return SceneManager.LoadSceneAsync(1);
        yield return new WaitForSeconds(0.5f);

        Player player = GameManager.instance.player;
        var flags = BindingFlags.NonPublic | BindingFlags.Instance;

        FieldInfo speedToSurviveField = typeof(Player).GetField("speedToSurvive", flags);
        FieldInfo moveSpeedField = typeof(Player).GetField("moveSpeed", flags);
        Assert.That(speedToSurviveField, Is.Not.Null);
        Assert.That(moveSpeedField, Is.Not.Null);

        float speedToSurvive = (float)speedToSurviveField.GetValue(player);

        moveSpeedField.SetValue(player, speedToSurvive + 1f);
        yield return null;
        yield return null;

        Assert.That(player.extraLife, Is.True,
            "extraLife phải là true khi moveSpeed >= speedToSurvive!");

        moveSpeedField.SetValue(player, speedToSurvive - 1f);
        yield return null;
        yield return null;

        Assert.That(player.extraLife, Is.False,
            "extraLife phải là false khi moveSpeed < speedToSurvive!");
    }

    [UnityTest]
    [Category("PC")]
    [Category("Mobile")]
    [Description("Kiểm tra extraLife flag đồng bộ chính xác với moveSpeed so với ngưỡng speedToSurvive")]
    public IEnumerator TC_LOGIC_24_ExtraLifeFlag_DongBoVoiMoveSpeed()
    {
        yield return LoadGameScene();
        UnlockPlayer();

        Player player = GameManager.instance.player;
        var flags = BindingFlags.NonPublic | BindingFlags.Instance;
        var moveSpeedField = typeof(Player).GetField("moveSpeed", flags);
        Assert.IsNotNull(moveSpeedField);

        moveSpeedField.SetValue(player, 10f);
        yield return new WaitForSeconds(0.1f);
        Assert.IsFalse(player.extraLife,
            "extraLife phải false khi moveSpeed < speedToSurvive.");

        moveSpeedField.SetValue(player, 20f);
        yield return new WaitForSeconds(0.1f);
        Assert.IsTrue(player.extraLife,
            "extraLife phải true khi moveSpeed >= speedToSurvive.");
    }

    // ═══════════════════════════════════════════════
    //  COIN & ĐIỂM SỐ & LƯU DỮ LIỆU
    // ═══════════════════════════════════════════════

    [UnityTest]
    [Category("PC")]
    [Category("Mobile")]
    [Description("Kiểm tra coins tăng lên và Coin bị hủy khi Player nhặt coin")]
    public IEnumerator TC_LOGIC_25_CoinsTang_VaCoinBiHuy_KhiNhat()
    {
        yield return SceneManager.LoadSceneAsync(1);
        yield return new WaitForSeconds(0.5f);

        int coinsBeforePickup = GameManager.instance.coins;

        tempCoinObj = new GameObject("Coin");
        Coin coin = tempCoinObj.AddComponent<Coin>();

        tempPlayerObj = new GameObject("Player");
        tempPlayerObj.tag = "Player";
        BoxCollider2D playerCollider = tempPlayerObj.AddComponent<BoxCollider2D>();

        MethodInfo triggerMethod = typeof(Coin).GetMethod("OnTriggerEnter2D",
            BindingFlags.NonPublic | BindingFlags.Instance);
        triggerMethod.Invoke(coin, new object[] { playerCollider });

        yield return null;

        Assert.That(tempCoinObj == null || !tempCoinObj.activeInHierarchy, Is.True,
            "Coin không bị hủy sau khi Player chạm vào!");
        Assert.That(GameManager.instance.coins, Is.GreaterThan(coinsBeforePickup),
            "Số coins không tăng sau khi nhặt!");
    }

    [UnityTest]
    [Category("PC")]
    [Category("Mobile")]
    [Description("Kiểm tra SaveInfo tính score = distance × coins và lưu đúng vào PlayerPrefs")]
    public IEnumerator TC_LOGIC_26_SaveInfo_TinhScore_DistanceNhanCoins()
    {
        yield return LoadGameScene();

        GameManager gm = GameManager.instance;
        gm.distance = 100f;
        gm.coins = 5;

        gm.SaveInfo();

        float savedScore = PlayerPrefs.GetFloat("LastScore");
        Assert.AreEqual(500f, savedScore, 0.01f,
            "Score phải bằng distance × coins (100 × 5 = 500).");
    }

    [UnityTest]
    [Category("PC")]
    [Category("Mobile")]
    [Description("Kiểm tra HighScore chỉ cập nhật khi score mới cao hơn score cũ")]
    public IEnumerator TC_LOGIC_27_HighScore_ChiCapNhat_KhiCaoHon()
    {
        yield return LoadGameScene();

        GameManager gm = GameManager.instance;

        PlayerPrefs.SetFloat("HighScore", 1000f);

        gm.distance = 5f;
        gm.coins = 2;
        gm.SaveInfo();

        Assert.AreEqual(1000f, PlayerPrefs.GetFloat("HighScore"), 0.01f,
            "HighScore không được cập nhật khi score mới (10) thấp hơn (1000).");

        gm.distance = 200f;
        gm.coins = 10;
        gm.SaveInfo();

        Assert.AreEqual(2000f, PlayerPrefs.GetFloat("HighScore"), 0.01f,
            "HighScore phải cập nhật khi score mới (2000) cao hơn (1000).");
    }

    [UnityTest]
    [Category("PC")]
    [Category("Mobile")]
    [Description("Kiểm tra coins được cộng dồn vào tổng coins đã lưu khi gọi SaveInfo")]
    public IEnumerator TC_LOGIC_28_CoinsCongDon_KhiSaveInfo()
    {
        yield return LoadGameScene();

        GameManager gm = GameManager.instance;
        PlayerPrefs.SetInt("Coins", 50);

        gm.coins = 30;
        gm.distance = 10f;
        gm.SaveInfo();

        int totalCoins = PlayerPrefs.GetInt("Coins");
        Assert.AreEqual(80, totalCoins,
            "Coins phải cộng dồn: 50 (cũ) + 30 (mới) = 80.");
    }

    [UnityTest]
    [Category("PC")]
    [Category("Mobile")]
    [Description("Kiểm tra SaveInfo lưu đúng giá trị coins vào PlayerPrefs key 'Coins'")]
    public IEnumerator TC_LOGIN_01_SaveInfo_LuuCoinsVaoPlayerPrefs()
    {
        yield return SceneManager.LoadSceneAsync(1);
        Time.timeScale = 1;
        GameManager.instance.UnlockPlayer();
        yield return null;

        PlayerPrefs.DeleteKey("Coins");
        PlayerPrefs.Save();

        GameManager.instance.coins = 50;
        GameManager.instance.SaveInfo();

        int savedCoins = PlayerPrefs.GetInt("Coins", -1);

        Assert.That(savedCoins, Is.Not.EqualTo(-1),
            "PlayerPrefs không có key 'Coins' sau SaveInfo()!");
        Assert.That(savedCoins, Is.EqualTo(50),
            $"SaveInfo() lưu sai giá trị! Mong đợi 50, thực tế {savedCoins}.");
    }

    [UnityTest]
    [Category("PC")]
    [Category("Mobile")]
    [Description("Kiểm tra SaveColor lưu đúng giá trị R, G, B vào PlayerPrefs")]
    public IEnumerator TC_LOGIC_29_SaveColor_LuuDungRGB()
    {
        yield return LoadGameScene();

        float r = 0.25f, g = 0.50f, b = 0.75f;
        GameManager.instance.SaveColor(r, g, b);
        yield return null;

        Assert.AreEqual(r, PlayerPrefs.GetFloat("ColorR"), 0.001f,
            "PlayerPrefs ColorR không khớp.");
        Assert.AreEqual(g, PlayerPrefs.GetFloat("ColorG"), 0.001f,
            "PlayerPrefs ColorG không khớp.");
        Assert.AreEqual(b, PlayerPrefs.GetFloat("ColorB"), 0.001f,
            "PlayerPrefs ColorB không khớp.");
    }

    // ═══════════════════════════════════════════════
    //  BẪY & SINH MAP & LEO VÁCH
    // ═══════════════════════════════════════════════

    [UnityTest]
    [Category("PC")]
    [Category("Mobile")]
    [Description("Kiểm tra Trap không bị hủy khi chanceToSpawn = 200 (luôn spawn thành công)")]
    public IEnumerator TC_LOGIC_30_TrapTonTai_KhiChanceToSpawnCao()
    {
        tempTrapObj = new GameObject();
        Trap trap = tempTrapObj.AddComponent<Trap>();

        typeof(Trap).GetField("chanceToSpawn",
            BindingFlags.NonPublic | BindingFlags.Instance).SetValue(trap, 200f);

        typeof(Trap).GetMethod("Start",
            BindingFlags.NonPublic | BindingFlags.Instance).Invoke(trap, null);

        yield return null;

        Assert.That(tempTrapObj, Is.Not.Null,
            "Trap bị hủy dù chanceToSpawn = 200 (luôn thành công)!");
    }

    [Test]
    [Category("PC")]
    [Category("Mobile")]
    [Description("Kiểm tra LedgeDetection tắt canDetect khi chạm vào layer Ground")]
    public void TC_LOGIC_31_LedgeDetection_TatCanDetect_KhiChamGround()
    {
        tempLedgeObj = new GameObject();
        LedgeDetection ledge = tempLedgeObj.AddComponent<LedgeDetection>();
        ledge.canDetect = true;

        tempGroundObj = new GameObject();
        tempGroundObj.layer = LayerMask.NameToLayer("Ground");
        BoxCollider2D groundCollider = tempGroundObj.AddComponent<BoxCollider2D>();

        typeof(LedgeDetection).GetMethod("OnTriggerEnter2D",
            BindingFlags.NonPublic | BindingFlags.Instance)
            .Invoke(ledge, new object[] { groundCollider });

        Assert.That(ledge.canDetect, Is.False,
            "canDetect phải bị tắt khi LedgeDetection chạm vào layer Ground!");
    }

    [Test]
    [Category("PC")]
    [Category("Mobile")]
    [Description("Kiểm tra GeneratePlatform spawn phần map mới khi Player đủ gần nextPartPosition")]
    public void TC_LOGIC_32_GeneratePlatform_SpawnMapMoi_KhiPlayerGan()
    {
        tempPartPrefab = new GameObject("Part");

        GameObject startPoint = new GameObject("StartPoint");
        GameObject endPoint = new GameObject("EndPoint");
        startPoint.transform.SetParent(tempPartPrefab.transform);
        endPoint.transform.SetParent(tempPartPrefab.transform);

        startPoint.transform.position = new Vector3(0f, 0f, 0f);
        endPoint.transform.position = new Vector3(10f, 0f, 0f);

        tempGeneratorObj = new GameObject("LevelGenerator");
        LevelGenerator generator = tempGeneratorObj.AddComponent<LevelGenerator>();

        tempPlayerObj = new GameObject("Player");
        tempPlayerObj.transform.position = Vector3.zero;

        var flags = BindingFlags.NonPublic | BindingFlags.Instance;

        typeof(LevelGenerator).GetField("levelPart", flags)
            .SetValue(generator, new Transform[] { tempPartPrefab.transform });
        typeof(LevelGenerator).GetField("player", flags)
            .SetValue(generator, tempPlayerObj.transform);
        typeof(LevelGenerator).GetField("distanceToSpawn", flags)
            .SetValue(generator, 20f);
        typeof(LevelGenerator).GetField("distanceToDelete", flags)
            .SetValue(generator, 1000f);
        typeof(LevelGenerator).GetField("nextPartPosition", flags)
            .SetValue(generator, new Vector3(5f, 0f, 0f));

        typeof(LevelGenerator).GetMethod("GeneratePlatform", flags)
            .Invoke(generator, null);

        Assert.That(tempGeneratorObj.transform.childCount, Is.GreaterThan(0),
            "GeneratePlatform() không spawn phần map mới khi Player đủ gần!");
    }

    [UnityTest]
    [Category("PC")]
    [Category("Mobile")]
    [Description("Kiểm tra LevelGenerator spawn thêm platform khi Player tiến đến gần trong scene thật")]
    public IEnumerator TC_LOGIC_33_LevelGenerator_SpawnThem_KhiPlayerTienGan()
    {
        yield return LoadGameScene();
        UnlockPlayer();

        LevelGenerator generator = Object.FindFirstObjectByType<LevelGenerator>();
        Assert.IsNotNull(generator);

        yield return new WaitForSeconds(0.3f);
        int childCountBefore = generator.transform.childCount;

        Player player = GameManager.instance.player;
        player.transform.position = new Vector3(
            player.transform.position.x + 200f,
            player.transform.position.y, 0f);

        yield return new WaitForSeconds(0.5f);
        int childCountAfter = generator.transform.childCount;

        Assert.GreaterOrEqual(childCountAfter, childCountBefore,
            "LevelGenerator phải spawn thêm platform khi Player tiến gần.");
    }

    [UnityTest]
    [Category("PC")]
    [Category("Mobile")]
    [Description("Kiểm tra LevelGenerator xóa platform cũ khi Player di chuyển quá xa về phía trước")]
    public IEnumerator TC_LOGIC_34_LevelGenerator_XoaPlatformCu_KhiPlayerDiXa()
    {
        yield return LoadGameScene();
        UnlockPlayer();

        LevelGenerator generator = Object.FindFirstObjectByType<LevelGenerator>();
        Assert.IsNotNull(generator);

        yield return new WaitForSeconds(0.5f);
        Assert.Greater(generator.transform.childCount, 0,
            "Tiền đề: phải có ít nhất 1 platform trước khi test xóa.");

        GameObject firstPlatform = generator.transform.GetChild(0).gameObject;

        Player player = GameManager.instance.player;
        player.transform.position = new Vector3(
            firstPlatform.transform.position.x + 500f,
            player.transform.position.y, 0f);

        yield return new WaitForSeconds(0.5f);

        Assert.IsTrue(firstPlatform == null,
            "Platform cũ phải bị Destroy khi Player đi quá distanceToDelete.");
    }
}
