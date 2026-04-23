using System.Collections;
using System.Reflection;
using NUnit.Framework;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;

/// <summary>
/// UI Tests - Kiểm tra giao diện: UI_Main, UI_Shop, UI_EndGame, Buttons, PlayFab.
/// </summary>
public class UITests
{
    private static readonly BindingFlags FLAGS = BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public;
    private void Set(object t, string f, object v) { t.GetType().GetField(f, FLAGS).SetValue(t, v); }
    private T Get<T>(object t, string f) { return (T)t.GetType().GetField(f, FLAGS).GetValue(t); }

    [TearDown]
    public void Teardown() { Time.timeScale = 1f; }

    // ═══════════════════════════════════════
    //  GIỮ NGUYÊN TỪ FILE GỐC
    // ═══════════════════════════════════════

    [UnityTest, Category("PC"), Category("Mobile")]
    [Description("TC_UI_7 - Kiểm tra UI InGame hiển thị đúng số coins khi giá trị coins thay đổi")]
    public IEnumerator TC_UI_7_UIInGame_HienThiDungSoCoins()
    {
        yield return SceneManager.LoadSceneAsync(1);
        Time.timeScale = 1; GameManager.instance.UnlockPlayer(); yield return null;

        UI_InGame ui = GameObject.FindObjectOfType<UI_InGame>(true);
        if (ui == null) { Assert.Pass("Không có UI_InGame, bỏ qua."); yield break; }

        GameManager.instance.coins = 888;
        yield return new WaitForSeconds(0.5f);

        TextMeshProUGUI[] allTexts = ui.GetComponentsInChildren<TextMeshProUGUI>(true);
        bool found = false;
        foreach (var txt in allTexts) { if (txt.text.Contains("888")) { found = true; break; } }
        Assert.IsTrue(found, "UI InGame không hiển thị đúng số coins (888)!");
    }

    [UnityTest, Category("PC"), Category("Mobile")]
    [Description("TC_UI_16 - Kiểm tra nút Pause: lần 1 dừng game (timeScale=0), lần 2 tiếp tục (timeScale=1)")]
    public IEnumerator TC_UI_16_NutPause_ToggleTimeScale()
    {
        yield return SceneManager.LoadSceneAsync(1);
        yield return new WaitForSeconds(0.3f);

        UI_Main uiMain = Object.FindFirstObjectByType<UI_Main>();
        Assert.IsNotNull(uiMain);
        typeof(UI_Main).GetField("gamePaused", FLAGS).SetValue(uiMain, false);
        Time.timeScale = 1f; yield return null;

        uiMain.PauseGameButton(); yield return null;
        Assert.AreEqual(0f, Time.timeScale, "Pause lần 1: timeScale phải = 0.");
        uiMain.PauseGameButton(); yield return null;
        Assert.AreEqual(1f, Time.timeScale, "Pause lần 2: timeScale phải = 1.");
    }

    // ═══════════════════════════════════════
    //  GỘP TỪ CoverageBoostTests → UI_MAIN
    // ═══════════════════════════════════════

    [UnityTest, Category("PC"), Category("Mobile")]
    [Description("TC_UI_39 - Kiểm tra SwitchMenuTo kích hoạt đúng menu target và ẩn các menu con khác")]
    public IEnumerator TC_UI_39_SwitchMenuTo_KichHoatDungTarget()
    {
        yield return SceneManager.LoadSceneAsync(1);
        yield return new WaitForSeconds(0.5f);
        UI_Main uiMain = Object.FindFirstObjectByType<UI_Main>();
        var endGame = Get<GameObject>(uiMain, "endGame");
        uiMain.SwitchMenuTo(endGame); yield return null;
        Assert.IsTrue(endGame.activeSelf, "SwitchMenuTo phải activate menu target.");
    }

    [UnityTest, Category("PC"), Category("Mobile")]
    [Description("TC_UI_40 - Kiểm tra SwitchSkyBox gọi GameManager.SetupSkyBox và lưu đúng index vào PlayerPrefs")]
    public IEnumerator TC_UI_40_SwitchSkyBox_LuuDungIndex()
    {
        yield return SceneManager.LoadSceneAsync(1);
        yield return new WaitForSeconds(0.5f);
        Object.FindFirstObjectByType<UI_Main>().SwitchSkyBox(1); yield return null;
        Assert.AreEqual(1, PlayerPrefs.GetInt("SkyBoxSetting"));
    }

    [UnityTest, Category("PC"), Category("Mobile")]
    [Description("TC_UI_41 - Kiểm tra StartGameButton mở khóa Player (playerUnlocked = true) khi nhấn bắt đầu")]
    public IEnumerator TC_UI_41_StartGameButton_MoKhoaPlayer()
    {
        yield return SceneManager.LoadSceneAsync(1);
        yield return new WaitForSeconds(0.5f);
        Player p = GameManager.instance.player; p.playerUnlocked = false;
        Object.FindFirstObjectByType<UI_Main>().StartGameButton(); yield return null;
        Assert.IsTrue(p.playerUnlocked, "StartGameButton phải unlock Player.");
    }

    [UnityTest, Category("PC"), Category("Mobile")]
    [Description("TC_UI_42 - Kiểm tra OpenEndGameUI chuyển sang hiển thị panel End Game khi game kết thúc")]
    public IEnumerator TC_UI_42_OpenEndGameUI_HienThiPanelEndGame()
    {
        yield return SceneManager.LoadSceneAsync(1);
        yield return new WaitForSeconds(0.5f);
        UI_Main uiMain = Object.FindFirstObjectByType<UI_Main>();
        var endGame = Get<GameObject>(uiMain, "endGame");
        uiMain.OpenEndGameUI(); yield return null;
        Assert.IsTrue(endGame.activeSelf, "OpenEndGameUI phải activate endGame panel.");
    }

    // ═══════════════════════════════════════
    //  GỘP TỪ AdditionalAutoTests → UI_SHOP
    // ═══════════════════════════════════════

    [Test, Category("PC"), Category("Mobile")]
    [Description("TC_UI_43 - Kiểm tra EnoughMoney trả về false khi số coins hiện tại nhỏ hơn giá mua")]
    public void TC_UI_43_EnoughMoney_False_KhiKhongDuTien()
    {
        PlayerPrefs.SetInt("Coins", 10);
        var sO = new GameObject(); var cO = new GameObject(); var nO = new GameObject();
        var shop = sO.AddComponent<UI_Shop>();
        Set(shop, "coinsText", cO.AddComponent<TextMeshProUGUI>());
        Set(shop, "notifyText", nO.AddComponent<TextMeshProUGUI>());
        bool result = (bool)typeof(UI_Shop).GetMethod("EnoughMoney", FLAGS).Invoke(shop, new object[] { 50 });
        Assert.IsFalse(result, "EnoughMoney phải false khi coins (10) < price (50).");
        PlayerPrefs.DeleteKey("Coins");
        Object.DestroyImmediate(sO); Object.DestroyImmediate(cO); Object.DestroyImmediate(nO);
    }

    [Test, Category("PC"), Category("Mobile")]
    [Description("TC_UI_44 - Kiểm tra EnoughMoney trả về true và trừ đúng số coins khi đủ tiền mua")]
    public void TC_UI_44_EnoughMoney_True_VaTruDungCoins()
    {
        PlayerPrefs.SetInt("Coins", 100);
        var sO = new GameObject(); var cO = new GameObject(); var nO = new GameObject();
        var shop = sO.AddComponent<UI_Shop>();
        Set(shop, "coinsText", cO.AddComponent<TextMeshProUGUI>());
        Set(shop, "notifyText", nO.AddComponent<TextMeshProUGUI>());
        bool result = (bool)typeof(UI_Shop).GetMethod("EnoughMoney", FLAGS).Invoke(shop, new object[] { 30 });
        Assert.IsTrue(result); Assert.AreEqual(70, PlayerPrefs.GetInt("Coins"));
        PlayerPrefs.DeleteKey("Coins");
        Object.DestroyImmediate(sO); Object.DestroyImmediate(cO); Object.DestroyImmediate(nO);
    }

    [Test, Category("PC"), Category("Mobile")]
    [Description("TC_UI_45 - Kiểm tra EnoughMoney trả về false khi coins bằng đúng giá mua (boundary: dùng > thay vì >=)")]
    public void TC_UI_45_EnoughMoney_False_KhiCoinsBangGia()
    {
        PlayerPrefs.SetInt("Coins", 50);
        var sO = new GameObject(); var cO = new GameObject(); var nO = new GameObject();
        var shop = sO.AddComponent<UI_Shop>();
        Set(shop, "coinsText", cO.AddComponent<TextMeshProUGUI>());
        Set(shop, "notifyText", nO.AddComponent<TextMeshProUGUI>());
        bool result = (bool)typeof(UI_Shop).GetMethod("EnoughMoney", FLAGS).Invoke(shop, new object[] { 50 });
        Assert.IsFalse(result, "EnoughMoney dùng '>' nên coins == price phải false.");
        PlayerPrefs.DeleteKey("Coins");
        Object.DestroyImmediate(sO); Object.DestroyImmediate(cO); Object.DestroyImmediate(nO);
    }

    [Test, Category("PC"), Category("Mobile")]
    [Description("TC_UI_46 - Kiểm tra EnoughMoney cập nhật đúng số coins còn lại trên coinsText UI sau khi trừ tiền")]
    public void TC_UI_46_EnoughMoney_CapNhatCoinsTextUI()
    {
        PlayerPrefs.SetInt("Coins", 200);
        var sO = new GameObject(); var cO = new GameObject(); var nO = new GameObject();
        var ct = cO.AddComponent<TextMeshProUGUI>();
        var shop = sO.AddComponent<UI_Shop>();
        Set(shop, "coinsText", ct); Set(shop, "notifyText", nO.AddComponent<TextMeshProUGUI>());
        typeof(UI_Shop).GetMethod("EnoughMoney", FLAGS).Invoke(shop, new object[] { 50 });
        Assert.IsTrue(ct.text.Contains("150"), "coinsText phải hiển thị 150 sau khi mua.");
        PlayerPrefs.DeleteKey("Coins");
        Object.DestroyImmediate(sO); Object.DestroyImmediate(cO); Object.DestroyImmediate(nO);
    }

    // ═══════════════════════════════════════
    //  GỘP TỪ AdditionalAutoTests → UI_ENDGAME
    // ═══════════════════════════════════════

    [UnityTest, Category("PC"), Category("Mobile")]
    [Description("TC_UI_47 - Kiểm tra UI_EndGame hiển thị đúng distance, coins và score khi giá trị > 0")]
    public IEnumerator TC_UI_47_EndGame_HienThiDung_KhiGiaTriDuong()
    {
        yield return SceneManager.LoadSceneAsync(1); yield return new WaitForSeconds(0.5f);
        GameManager.instance.distance = 500f; GameManager.instance.coins = 10; GameManager.instance.score = 100f;
        var go = new GameObject(); var dO = new GameObject(); var cO = new GameObject(); var sO = new GameObject();
        var dT = dO.AddComponent<TextMeshProUGUI>(); var cT = cO.AddComponent<TextMeshProUGUI>(); var sT = sO.AddComponent<TextMeshProUGUI>();
        var eg = go.AddComponent<UI_EndGame>(); Set(eg, "distance", dT); Set(eg, "coins", cT); Set(eg, "score", sT);
        yield return null;
        Assert.IsTrue(dT.text.Contains("500")); Assert.IsTrue(cT.text.Contains("10"));
        Object.DestroyImmediate(go); Object.DestroyImmediate(dO); Object.DestroyImmediate(cO); Object.DestroyImmediate(sO);
    }

    [UnityTest, Category("PC"), Category("Mobile")]
    [Description("TC_UI_48 - Kiểm tra UI_EndGame không cập nhật text khi distance <= 0 (return early)")]
    public IEnumerator TC_UI_48_EndGame_KhongCapNhat_KhiDistanceBangKhong()
    {
        yield return SceneManager.LoadSceneAsync(1); yield return new WaitForSeconds(0.5f);
        GameManager.instance.distance = 0f; GameManager.instance.coins = 10;
        var go = new GameObject(); var dO = new GameObject(); var cO = new GameObject(); var sO = new GameObject();
        var dT = dO.AddComponent<TextMeshProUGUI>(); dT.text = "";
        var eg = go.AddComponent<UI_EndGame>(); Set(eg, "distance", dT); Set(eg, "coins", cO.AddComponent<TextMeshProUGUI>()); Set(eg, "score", sO.AddComponent<TextMeshProUGUI>());
        yield return null;
        Assert.AreEqual("", dT.text);
        Object.DestroyImmediate(go); Object.DestroyImmediate(dO); Object.DestroyImmediate(cO); Object.DestroyImmediate(sO);
    }

    [UnityTest, Category("PC"), Category("Mobile")]
    [Description("TC_UI_49 - Kiểm tra UI_EndGame không cập nhật score khi coins <= 0 (return early)")]
    public IEnumerator TC_UI_49_EndGame_KhongCapNhat_KhiCoinsBangKhong()
    {
        yield return SceneManager.LoadSceneAsync(1); yield return new WaitForSeconds(0.5f);
        GameManager.instance.distance = 100f; GameManager.instance.coins = 0;
        var go = new GameObject(); var dO = new GameObject(); var cO = new GameObject(); var sO = new GameObject();
        var sT = sO.AddComponent<TextMeshProUGUI>(); sT.text = "";
        var eg = go.AddComponent<UI_EndGame>(); Set(eg, "distance", dO.AddComponent<TextMeshProUGUI>()); Set(eg, "coins", cO.AddComponent<TextMeshProUGUI>()); Set(eg, "score", sT);
        yield return null;
        Assert.AreEqual("", sT.text);
        Object.DestroyImmediate(go); Object.DestroyImmediate(dO); Object.DestroyImmediate(cO); Object.DestroyImmediate(sO);
    }

    // ═══════════════════════════════════════
    //  GỘP TỪ AdditionalAutoTests → BUTTONS
    // ═══════════════════════════════════════

    [UnityTest, Category("PC"), Category("Mobile")]
    [Description("TC_UI_50 - Kiểm tra UI_ButtonJump implement đúng interface IPointerDownHandler")]
    public IEnumerator TC_UI_50_ButtonJump_ImplementIPointerDownHandler()
    {
        var o = new GameObject(); var b = o.AddComponent<UI_ButtonJump>(); yield return null;
        Assert.IsTrue(b is IPointerDownHandler); Object.DestroyImmediate(o);
    }

    [UnityTest, Category("PC"), Category("Mobile")]
    [Description("TC_UI_51 - Kiểm tra UI_ButtonSlide implement đúng interface IPointerDownHandler")]
    public IEnumerator TC_UI_51_ButtonSlide_ImplementIPointerDownHandler()
    {
        var o = new GameObject(); var b = o.AddComponent<UI_ButtonSlide>(); yield return null;
        Assert.IsTrue(b is IPointerDownHandler); Object.DestroyImmediate(o);
    }

    [UnityTest, Category("PC"), Category("Mobile")]
    [Description("TC_UI_52 - Kiểm tra OnPointerDown của UI_ButtonJump kích hoạt nhảy cho Player trong scene thật")]
    public IEnumerator TC_UI_52_ButtonJump_OnPointerDown_KichHoatNhay()
    {
        yield return SceneManager.LoadSceneAsync(1); yield return new WaitForSeconds(0.5f);
        GameManager.instance.UnlockPlayer(); yield return new WaitForSeconds(0.5f);
        var rb = GameManager.instance.player.GetComponent<Rigidbody2D>();
        float before = rb.linearVelocity.y;
        var o = new GameObject(); var b = o.AddComponent<UI_ButtonJump>();
        b.OnPointerDown(new PointerEventData(EventSystem.current));
        yield return new WaitForFixedUpdate();
        Assert.Greater(rb.linearVelocity.y, before); Object.DestroyImmediate(o);
    }

    [UnityTest, Category("PC"), Category("Mobile")]
    [Description("TC_UI_53 - Kiểm tra OnPointerDown của UI_ButtonSlide kích hoạt trượt cho Player trong scene thật")]
    public IEnumerator TC_UI_53_ButtonSlide_OnPointerDown_KichHoatTruot()
    {
        yield return SceneManager.LoadSceneAsync(1); yield return new WaitForSeconds(0.5f);
        GameManager.instance.UnlockPlayer(); yield return new WaitForSeconds(0.5f);
        var o = new GameObject(); var b = o.AddComponent<UI_ButtonSlide>();
        b.OnPointerDown(new PointerEventData(EventSystem.current));
        yield return new WaitForSeconds(0.1f);
        bool isSliding = (bool)typeof(Player).GetField("isSliding", FLAGS).GetValue(GameManager.instance.player);
        Assert.IsTrue(isSliding); Object.DestroyImmediate(o);
    }

    // ═══════════════════════════════════════
    //  GỘP TỪ AdditionalAutoTests → PLAYFAB
    // ═══════════════════════════════════════

    private GameObject pfmObj;
    private PlayFabManager pfm;

    private void SetupPFM()
    {
        pfmObj = new GameObject("PFM"); pfm = pfmObj.AddComponent<PlayFabManager>();
        pfm.registerPanel = new GameObject("R"); pfm.loginPanel = new GameObject("L");
        pfm.messageText = new GameObject("MT").AddComponent<TextMeshProUGUI>();
        pfm.loginMessageText = new GameObject("LMT").AddComponent<TextMeshProUGUI>();
        pfm.emailInput = new GameObject("EI").AddComponent<TMP_InputField>();
        pfm.passwordInput = new GameObject("PI").AddComponent<TMP_InputField>();
        pfm.usernameInput = new GameObject("UI").AddComponent<TMP_InputField>();
        pfm.loginEmailInput = new GameObject("LEI").AddComponent<TMP_InputField>();
        pfm.loginPasswordInput = new GameObject("LPI").AddComponent<TMP_InputField>();
    }

    private void TeardownPFM()
    {
        Object.DestroyImmediate(pfm?.registerPanel); Object.DestroyImmediate(pfm?.loginPanel);
        Object.DestroyImmediate(pfm?.messageText?.gameObject); Object.DestroyImmediate(pfm?.loginMessageText?.gameObject);
        Object.DestroyImmediate(pfm?.emailInput?.gameObject); Object.DestroyImmediate(pfm?.passwordInput?.gameObject);
        Object.DestroyImmediate(pfm?.usernameInput?.gameObject); Object.DestroyImmediate(pfm?.loginEmailInput?.gameObject);
        Object.DestroyImmediate(pfm?.loginPasswordInput?.gameObject); Object.DestroyImmediate(pfmObj);
    }

    [UnityTest, Category("PC"), Category("Mobile")]
    [Description("TC_REGISTER_04 - Kiểm tra RegisterButton từ chối mật khẩu ngắn hơn 6 ký tự và hiện thông báo lỗi")]
    public IEnumerator TC_REGISTER_04_TuChoiMatKhauNganHon6()
    {
        SetupPFM(); yield return null;
        pfm.passwordInput.text = "123"; pfm.RegisterButton(); yield return null;
        Assert.AreEqual("Password too short!", pfm.messageText.text); TeardownPFM();
    }

    [UnityTest, Category("PC"), Category("Mobile")]
    [Description("TC_REGISTER_05 - Kiểm tra RegisterButton chấp nhận mật khẩu đúng 6 ký tự (không báo lỗi 'Password too short')")]
    public IEnumerator TC_REGISTER_05_ChapNhanMatKhauDung6KyTu()
    {
        SetupPFM(); yield return null;
        pfm.passwordInput.text = "123456"; pfm.emailInput.text = "t@t.com"; pfm.usernameInput.text = "user";
        pfm.RegisterButton(); yield return null;
        Assert.AreNotEqual("Password too short!", pfm.messageText.text); TeardownPFM();
    }

    [UnityTest, Category("PC"), Category("Mobile")]
    [Description("TC_REGISTER_06 - Kiểm tra RegisterButton chấp nhận mật khẩu dài hơn 6 ký tự (không báo lỗi)")]
    public IEnumerator TC_REGISTER_06_ChapNhanMatKhauDaiHon6()
    {
        SetupPFM(); yield return null;
        pfm.passwordInput.text = "securepass123"; pfm.emailInput.text = "t@t.com"; pfm.usernameInput.text = "user";
        pfm.RegisterButton(); yield return null;
        Assert.AreNotEqual("Password too short!", pfm.messageText.text); TeardownPFM();
    }

    [UnityTest, Category("PC"), Category("Mobile")]
    [Description("TC_LOGIN_04 - Kiểm tra signInClick bật registerPanel và tắt loginPanel khi nhấn chuyển sang đăng ký")]
    public IEnumerator TC_LOGIN_04_SignInClick_BatRegisterTatLogin()
    {
        SetupPFM(); yield return null;
        pfm.registerPanel.SetActive(false); pfm.loginPanel.SetActive(true);
        pfm.signInClick();
        Assert.IsTrue(pfm.registerPanel.activeSelf); Assert.IsFalse(pfm.loginPanel.activeSelf); TeardownPFM();
    }

    [UnityTest, Category("PC"), Category("Mobile")]
    [Description("TC_LOGIN_05 - Kiểm tra backClick tắt registerPanel và bật loginPanel khi nhấn quay lại đăng nhập")]
    public IEnumerator TC_LOGIN_05_BackClick_TatRegisterBatLogin()
    {
        SetupPFM(); yield return null;
        pfm.registerPanel.SetActive(true); pfm.loginPanel.SetActive(false);
        pfm.backClick();
        Assert.IsFalse(pfm.registerPanel.activeSelf); Assert.IsTrue(pfm.loginPanel.activeSelf); TeardownPFM();
    }

    [UnityTest, Category("PC"), Category("Mobile")]
    [Description("TC_LOGIN_06 - Kiểm tra signInClick rồi backClick khôi phục đúng trạng thái ban đầu của 2 panel")]
    public IEnumerator TC_LOGIN_06_SignInRoiBack_KhoiPhucTrangThai()
    {
        SetupPFM(); yield return null;
        pfm.registerPanel.SetActive(false); pfm.loginPanel.SetActive(true);
        pfm.signInClick(); pfm.backClick();
        Assert.IsFalse(pfm.registerPanel.activeSelf); Assert.IsTrue(pfm.loginPanel.activeSelf); TeardownPFM();
    }

    [Test, Category("PC"), Category("Mobile")]
    [Description("TC_LOGIN_07 - Kiểm tra PlayFabManager có đầy đủ các public fields cần thiết để gán từ Inspector")]
    public void TC_LOGIN_07_PlayFab_CoDayDuPublicFields()
    {
        var o = new GameObject(); o.AddComponent<PlayFabManager>();
        Assert.IsNotNull(typeof(PlayFabManager).GetField("registerPanel"));
        Assert.IsNotNull(typeof(PlayFabManager).GetField("loginPanel"));
        Assert.IsNotNull(typeof(PlayFabManager).GetField("messageText"));
        Assert.IsNotNull(typeof(PlayFabManager).GetField("emailInput"));
        Assert.IsNotNull(typeof(PlayFabManager).GetField("passwordInput"));
        Assert.IsNotNull(typeof(PlayFabManager).GetField("usernameInput"));
        Object.DestroyImmediate(o);
    }
}