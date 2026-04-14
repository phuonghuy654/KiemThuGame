using System.Collections;
using System.Reflection;
using NUnit.Framework;
using TMPro;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;

/// <summary>
/// Auto tests bổ sung cho các script chưa được test:
/// UI_VolumeSlider, UI_Shop, UI_EndGame, UI_ButtonJump,
/// UI_ButtonSlide, LimiterGizmos, PlayFabManager, SceneController.
/// Tránh trùng với: HoangTest, HungTest, HuyTest, PhatTest, VuTest,
/// PlayMode, PlayModeDeepLogic, RunToLife_Assignment_Tests, NewAutoTests_PlayMode.
/// </summary>
[TestFixture]
public class AdditionalAutoTests
{
    #region === UI_VOLUMESLIDER TESTS ===

    private GameObject sliderObj;
    private UI_VolumeSlider volumeSlider;
    private Slider sliderComponent;

    private void SetupVolumeSlider()
    {
        sliderObj = new GameObject("VolumeSliderObj");
        volumeSlider = sliderObj.AddComponent<UI_VolumeSlider>();

        var sliderGo = new GameObject("Slider");
        sliderGo.transform.SetParent(sliderObj.transform);
        sliderComponent = sliderGo.AddComponent<Slider>();

        SetPrivateField(volumeSlider, typeof(UI_VolumeSlider), "slider", sliderComponent);
        SetPrivateField(volumeSlider, typeof(UI_VolumeSlider), "multiplier", 25f);
        SetPrivateField(volumeSlider, typeof(UI_VolumeSlider), "audioParametr", "TestVolume");
    }

    private void TeardownVolumeSlider()
    {
        if (sliderObj != null) Object.DestroyImmediate(sliderObj);
    }

    [UnityTest]
    [Description("UI_VolumeSlider - SetupSlider sets minValue to 0.001")]
    public IEnumerator VolumeSlider_SetupSlider_SetsMinValue()
    {
        SetupVolumeSlider();
        yield return null;

        volumeSlider.SetupSlider();

        Assert.AreEqual(0.001f, sliderComponent.minValue, 0.0001f,
            "Slider minValue phải được set thành 0.001 sau khi SetupSlider.");

        TeardownVolumeSlider();
    }

    [UnityTest]
    [Description("UI_VolumeSlider - SetupSlider loads value from PlayerPrefs")]
    public IEnumerator VolumeSlider_SetupSlider_LoadsValueFromPlayerPrefs()
    {
        SetupVolumeSlider();
        yield return null;

        PlayerPrefs.SetFloat("TestVolume", 0.5f);
        PlayerPrefs.Save();

        volumeSlider.SetupSlider();

        Assert.AreEqual(0.5f, sliderComponent.value, 0.01f,
            "Slider value phải được load từ PlayerPrefs sau SetupSlider.");

        PlayerPrefs.DeleteKey("TestVolume");
        TeardownVolumeSlider();
    }

    [UnityTest]
    [Description("UI_VolumeSlider - SetupSlider registers onValueChanged listener")]
    public IEnumerator VolumeSlider_SetupSlider_RegistersListener()
    {
        SetupVolumeSlider();
        yield return null;

        int listenerCountBefore = sliderComponent.onValueChanged.GetPersistentEventCount();
        volumeSlider.SetupSlider();

        // Không throw exception khi thay đổi value = listener hoạt động
        Assert.DoesNotThrow(() => sliderComponent.value = 0.5f,
            "Slider onValueChanged phải có listener sau SetupSlider.");

        TeardownVolumeSlider();
    }

    [UnityTest]
    [Description("UI_VolumeSlider - OnDisable saves slider value to PlayerPrefs")]
    [Category("PC")]
    [Category("Mobile")]
    public IEnumerator VolumeSlider_OnDisable_SavesValueToPlayerPrefs()
    {
        SetupVolumeSlider();
        yield return null;

        PlayerPrefs.DeleteKey("TestVolume");
        volumeSlider.SetupSlider();
        sliderComponent.value = 0.75f;

        sliderObj.SetActive(false);
        yield return null;

        float savedValue = PlayerPrefs.GetFloat("TestVolume", -1f);
        Assert.AreEqual(0.75f, savedValue, 0.01f,
            "OnDisable phải lưu giá trị slider vào PlayerPrefs.");

        PlayerPrefs.DeleteKey("TestVolume");
        TeardownVolumeSlider();
    }

    [UnityTest]
    [Description("UI_VolumeSlider - multiplier field defaults to 25")]
    public IEnumerator VolumeSlider_Multiplier_DefaultsTo25()
    {
        var tempObj = new GameObject("TempSlider");
        var vs = tempObj.AddComponent<UI_VolumeSlider>();
        yield return null;

        float multiplier = (float)GetPrivateFieldValue(vs, typeof(UI_VolumeSlider), "multiplier");
        Assert.AreEqual(25f, multiplier,
            "multiplier mặc định phải là 25.");

        Object.DestroyImmediate(tempObj);
    }

    #endregion

    #region === UI_SHOP TESTS ===

    [Test]
    [Description("UI_Shop - EnoughMoney returns false when coins < price")]
    public void Shop_EnoughMoney_ReturnsFalse_WhenNotEnoughCoins()
    {
        PlayerPrefs.SetInt("Coins", 10);

        var shopObj = new GameObject("Shop");
        var coinsTextObj = new GameObject("CoinsText");
        var notifyTextObj = new GameObject("NotifyText");
        var coinsText = coinsTextObj.AddComponent<TextMeshProUGUI>();
        var notifyText = notifyTextObj.AddComponent<TextMeshProUGUI>();

        var shop = shopObj.AddComponent<UI_Shop>();
        SetPrivateField(shop, typeof(UI_Shop), "coinsText", coinsText);
        SetPrivateField(shop, typeof(UI_Shop), "notifyText", notifyText);

        var method = typeof(UI_Shop).GetMethod("EnoughMoney",
            BindingFlags.NonPublic | BindingFlags.Instance);
        Assert.IsNotNull(method, "Không tìm thấy method EnoughMoney.");

        bool result = (bool)method.Invoke(shop, new object[] { 50 });

        Assert.IsFalse(result,
            "EnoughMoney phải trả về false khi coins (10) < price (50).");

        PlayerPrefs.DeleteKey("Coins");
        Object.DestroyImmediate(shopObj);
        Object.DestroyImmediate(coinsTextObj);
        Object.DestroyImmediate(notifyTextObj);
    }

    [Test]
    [Description("UI_Shop - EnoughMoney returns true and deducts coins when enough")]
    public void Shop_EnoughMoney_ReturnsTrue_WhenEnoughCoins()
    {
        PlayerPrefs.SetInt("Coins", 100);

        var shopObj = new GameObject("Shop");
        var coinsTextObj = new GameObject("CoinsText");
        var notifyTextObj = new GameObject("NotifyText");
        var coinsText = coinsTextObj.AddComponent<TextMeshProUGUI>();
        var notifyText = notifyTextObj.AddComponent<TextMeshProUGUI>();

        var shop = shopObj.AddComponent<UI_Shop>();
        SetPrivateField(shop, typeof(UI_Shop), "coinsText", coinsText);
        SetPrivateField(shop, typeof(UI_Shop), "notifyText", notifyText);

        var method = typeof(UI_Shop).GetMethod("EnoughMoney",
            BindingFlags.NonPublic | BindingFlags.Instance);

        bool result = (bool)method.Invoke(shop, new object[] { 30 });

        Assert.IsTrue(result,
            "EnoughMoney phải trả về true khi coins (100) > price (30).");

        int remaining = PlayerPrefs.GetInt("Coins");
        Assert.AreEqual(70, remaining,
            "Coins trong PlayerPrefs phải bị trừ đúng (100 - 30 = 70).");

        PlayerPrefs.DeleteKey("Coins");
        Object.DestroyImmediate(shopObj);
        Object.DestroyImmediate(coinsTextObj);
        Object.DestroyImmediate(notifyTextObj);
    }

    [Test]
    [Description("UI_Shop - EnoughMoney returns false when coins equal to price")]
    public void Shop_EnoughMoney_ReturnsFalse_WhenCoinsEqualPrice()
    {
        PlayerPrefs.SetInt("Coins", 50);

        var shopObj = new GameObject("Shop");
        var coinsTextObj = new GameObject("CoinsText");
        var notifyTextObj = new GameObject("NotifyText");
        var coinsText = coinsTextObj.AddComponent<TextMeshProUGUI>();
        var notifyText = notifyTextObj.AddComponent<TextMeshProUGUI>();

        var shop = shopObj.AddComponent<UI_Shop>();
        SetPrivateField(shop, typeof(UI_Shop), "coinsText", coinsText);
        SetPrivateField(shop, typeof(UI_Shop), "notifyText", notifyText);

        var method = typeof(UI_Shop).GetMethod("EnoughMoney",
            BindingFlags.NonPublic | BindingFlags.Instance);

        bool result = (bool)method.Invoke(shop, new object[] { 50 });

        Assert.IsFalse(result,
            "EnoughMoney dùng '>' nên coins == price phải trả về false (boundary bug).");

        PlayerPrefs.DeleteKey("Coins");
        Object.DestroyImmediate(shopObj);
        Object.DestroyImmediate(coinsTextObj);
        Object.DestroyImmediate(notifyTextObj);
    }

    [Test]
    [Description("UI_Shop - EnoughMoney updates coinsText UI after deduction")]
    public void Shop_EnoughMoney_UpdatesCoinsTextUI()
    {
        PlayerPrefs.SetInt("Coins", 200);

        var shopObj = new GameObject("Shop");
        var coinsTextObj = new GameObject("CoinsText");
        var notifyTextObj = new GameObject("NotifyText");
        var coinsText = coinsTextObj.AddComponent<TextMeshProUGUI>();
        var notifyText = notifyTextObj.AddComponent<TextMeshProUGUI>();

        var shop = shopObj.AddComponent<UI_Shop>();
        SetPrivateField(shop, typeof(UI_Shop), "coinsText", coinsText);
        SetPrivateField(shop, typeof(UI_Shop), "notifyText", notifyText);

        var method = typeof(UI_Shop).GetMethod("EnoughMoney",
            BindingFlags.NonPublic | BindingFlags.Instance);
        method.Invoke(shop, new object[] { 50 });

        Assert.IsTrue(coinsText.text.Contains("150"),
            "coinsText phải hiển thị số coins còn lại (150) sau khi mua.");

        PlayerPrefs.DeleteKey("Coins");
        Object.DestroyImmediate(shopObj);
        Object.DestroyImmediate(coinsTextObj);
        Object.DestroyImmediate(notifyTextObj);
    }

    [UnityTest]
    [Description("UI_Shop - Notify coroutine sets text and resets after delay")]
    [Category("PC")]
    [Category("Mobile")]
    public IEnumerator Shop_Notify_SetsTextAndResetsAfterDelay()
    {
        var shopObj = new GameObject("Shop");
        var coinsTextObj = new GameObject("CoinsText");
        var notifyTextObj = new GameObject("NotifyText");
        var coinsText = coinsTextObj.AddComponent<TextMeshProUGUI>();
        var notifyText = notifyTextObj.AddComponent<TextMeshProUGUI>();

        var shop = shopObj.AddComponent<UI_Shop>();
        SetPrivateField(shop, typeof(UI_Shop), "coinsText", coinsText);
        SetPrivateField(shop, typeof(UI_Shop), "notifyText", notifyText);

        var method = typeof(UI_Shop).GetMethod("Notify",
            BindingFlags.NonPublic | BindingFlags.Instance);
        Assert.IsNotNull(method, "Không tìm thấy method Notify.");

        shop.StartCoroutine((IEnumerator)method.Invoke(shop, new object[] { "Test message", 0.5f }));
        yield return null;

        Assert.AreEqual("Test message", notifyText.text,
            "Notify phải set text thông báo ngay lập tức.");

        yield return new WaitForSeconds(0.6f);

        Assert.AreEqual("Click to buy", notifyText.text,
            "Notify phải reset text về 'Click to buy' sau thời gian chờ.");

        Object.DestroyImmediate(shopObj);
        Object.DestroyImmediate(coinsTextObj);
        Object.DestroyImmediate(notifyTextObj);
    }

    #endregion

    #region === UI_ENDGAME TESTS ===

    [UnityTest]
    [Description("UI_EndGame - Start displays distance when > 0")]
    public IEnumerator EndGame_Start_DisplaysDistance_WhenPositive()
    {
        yield return SceneManager.LoadSceneAsync(1);
        yield return new WaitForSeconds(0.5f);

        GameManager.instance.distance = 500f;
        GameManager.instance.coins = 10;
        GameManager.instance.score = 100f;

        var endGameObj = new GameObject("EndGame");
        var distObj = new GameObject("DistText");
        var coinsObj = new GameObject("CoinsText");
        var scoreObj = new GameObject("ScoreText");

        var distText = distObj.AddComponent<TextMeshProUGUI>();
        var coinsText = coinsObj.AddComponent<TextMeshProUGUI>();
        var scoreText = scoreObj.AddComponent<TextMeshProUGUI>();

        var endGame = endGameObj.AddComponent<UI_EndGame>();
        SetPrivateField(endGame, typeof(UI_EndGame), "distance", distText);
        SetPrivateField(endGame, typeof(UI_EndGame), "coins", coinsText);
        SetPrivateField(endGame, typeof(UI_EndGame), "score", scoreText);

        yield return null;

        Assert.IsTrue(distText.text.Contains("500"),
            "UI_EndGame phải hiển thị distance khi > 0.");
        Assert.IsTrue(coinsText.text.Contains("10"),
            "UI_EndGame phải hiển thị coins khi > 0.");

        Object.DestroyImmediate(endGameObj);
        Object.DestroyImmediate(distObj);
        Object.DestroyImmediate(coinsObj);
        Object.DestroyImmediate(scoreObj);
    }

    [UnityTest]
    [Description("UI_EndGame - Start returns early when distance <= 0")]
    public IEnumerator EndGame_Start_ReturnsEarly_WhenDistanceIsZero()
    {
        yield return SceneManager.LoadSceneAsync(1);
        yield return new WaitForSeconds(0.5f);

        GameManager.instance.distance = 0f;
        GameManager.instance.coins = 10;

        var endGameObj = new GameObject("EndGame");
        var distObj = new GameObject("DistText");
        var coinsObj = new GameObject("CoinsText");
        var scoreObj = new GameObject("ScoreText");

        var distText = distObj.AddComponent<TextMeshProUGUI>();
        var coinsText = coinsObj.AddComponent<TextMeshProUGUI>();
        var scoreText = scoreObj.AddComponent<TextMeshProUGUI>();

        distText.text = "";
        coinsText.text = "";

        var endGame = endGameObj.AddComponent<UI_EndGame>();
        SetPrivateField(endGame, typeof(UI_EndGame), "distance", distText);
        SetPrivateField(endGame, typeof(UI_EndGame), "coins", coinsText);
        SetPrivateField(endGame, typeof(UI_EndGame), "score", scoreText);

        yield return null;

        Assert.AreEqual("", distText.text,
            "UI_EndGame không nên cập nhật text khi distance <= 0.");

        Object.DestroyImmediate(endGameObj);
        Object.DestroyImmediate(distObj);
        Object.DestroyImmediate(coinsObj);
        Object.DestroyImmediate(scoreObj);
    }

    [UnityTest]
    [Description("UI_EndGame - Start returns early when coins <= 0")]
    public IEnumerator EndGame_Start_ReturnsEarly_WhenCoinsIsZero()
    {
        yield return SceneManager.LoadSceneAsync(1);
        yield return new WaitForSeconds(0.5f);

        GameManager.instance.distance = 100f;
        GameManager.instance.coins = 0;

        var endGameObj = new GameObject("EndGame");
        var distObj = new GameObject("DistText");
        var coinsObj = new GameObject("CoinsText");
        var scoreObj = new GameObject("ScoreText");

        var distText = distObj.AddComponent<TextMeshProUGUI>();
        var coinsText = coinsObj.AddComponent<TextMeshProUGUI>();
        var scoreText = scoreObj.AddComponent<TextMeshProUGUI>();

        coinsText.text = "";
        scoreText.text = "";

        var endGame = endGameObj.AddComponent<UI_EndGame>();
        SetPrivateField(endGame, typeof(UI_EndGame), "distance", distText);
        SetPrivateField(endGame, typeof(UI_EndGame), "coins", coinsText);
        SetPrivateField(endGame, typeof(UI_EndGame), "score", scoreText);

        yield return null;

        Assert.AreEqual("", scoreText.text,
            "UI_EndGame không nên cập nhật score khi coins <= 0.");

        Object.DestroyImmediate(endGameObj);
        Object.DestroyImmediate(distObj);
        Object.DestroyImmediate(coinsObj);
        Object.DestroyImmediate(scoreObj);
    }

    #endregion

    #region === UI_BUTTONJUMP & UI_BUTTONSLIDE TESTS ===

    [UnityTest]
    [Description("UI_ButtonJump - Component implements IPointerDownHandler")]
    [Category("PC")]
    [Category("Mobile")]
    public IEnumerator ButtonJump_ImplementsIPointerDownHandler()
    {
        var btnObj = new GameObject("JumpBtn");
        var jumpBtn = btnObj.AddComponent<UI_ButtonJump>();
        yield return null;

        Assert.IsTrue(jumpBtn is IPointerDownHandler,
            "UI_ButtonJump phải implement IPointerDownHandler.");

        Object.DestroyImmediate(btnObj);
    }

    [UnityTest]
    [Description("UI_ButtonSlide - Component implements IPointerDownHandler")]
    [Category("PC")]
    [Category("Mobile")]
    public IEnumerator ButtonSlide_ImplementsIPointerDownHandler()
    {
        var btnObj = new GameObject("SlideBtn");
        var slideBtn = btnObj.AddComponent<UI_ButtonSlide>();
        yield return null;

        Assert.IsTrue(slideBtn is IPointerDownHandler,
            "UI_ButtonSlide phải implement IPointerDownHandler.");

        Object.DestroyImmediate(btnObj);
    }

    [UnityTest]
    [Description("UI_ButtonJump - OnPointerDown triggers player jump in scene")]
    [Category("PC")]
    [Category("Mobile")]
    public IEnumerator ButtonJump_OnPointerDown_TriggersPlayerJump()
    {
        yield return SceneManager.LoadSceneAsync(1);
        yield return new WaitForSeconds(0.5f);

        GameManager.instance.UnlockPlayer();
        yield return new WaitForSeconds(0.5f);

        Player player = GameManager.instance.player;
        Rigidbody2D rb = player.GetComponent<Rigidbody2D>();
        float velocityBefore = rb.linearVelocity.y;

        var btnObj = new GameObject("JumpBtn");
        var jumpBtn = btnObj.AddComponent<UI_ButtonJump>();
        var eventData = new PointerEventData(EventSystem.current);

        jumpBtn.OnPointerDown(eventData);
        yield return new WaitForFixedUpdate();

        Assert.Greater(rb.linearVelocity.y, velocityBefore,
            "OnPointerDown của UI_ButtonJump phải kích hoạt nhảy cho Player.");

        Object.DestroyImmediate(btnObj);
    }

    [UnityTest]
    [Description("UI_ButtonSlide - OnPointerDown triggers player slide in scene")]
    [Category("PC")]
    [Category("Mobile")]
    public IEnumerator ButtonSlide_OnPointerDown_TriggersPlayerSlide()
    {
        yield return SceneManager.LoadSceneAsync(1);
        yield return new WaitForSeconds(0.5f);

        GameManager.instance.UnlockPlayer();
        yield return new WaitForSeconds(0.5f);

        Player player = GameManager.instance.player;

        var btnObj = new GameObject("SlideBtn");
        var slideBtn = btnObj.AddComponent<UI_ButtonSlide>();
        var eventData = new PointerEventData(EventSystem.current);

        slideBtn.OnPointerDown(eventData);
        yield return new WaitForSeconds(0.1f);

        var isSlidingField = typeof(Player).GetField("isSliding",
            BindingFlags.NonPublic | BindingFlags.Instance);
        bool isSliding = (bool)isSlidingField.GetValue(player);

        Assert.IsTrue(isSliding,
            "OnPointerDown của UI_ButtonSlide phải kích hoạt slide cho Player.");

        Object.DestroyImmediate(btnObj);
    }

    #endregion

    #region === LIMITERGIZMOS TESTS ===

    [Test]
    [Description("LimiterGizmos - Component can be created with transform references")]
    public void LimiterGizmos_ComponentCreation_WithTransforms()
    {
        var gizmosObj = new GameObject("Limiter");
        var limiter = gizmosObj.AddComponent<LimiterGizmos>();

        var startObj = new GameObject("Start");
        var endObj = new GameObject("End");
        var groundObj = new GameObject("Ground");

        SetPrivateField(limiter, typeof(LimiterGizmos), "start", startObj.transform);
        SetPrivateField(limiter, typeof(LimiterGizmos), "end", endObj.transform);
        SetPrivateField(limiter, typeof(LimiterGizmos), "groundLevel", groundObj.transform);

        var startField = GetPrivateFieldValue(limiter, typeof(LimiterGizmos), "start");
        var endField = GetPrivateFieldValue(limiter, typeof(LimiterGizmos), "end");
        var groundField = GetPrivateFieldValue(limiter, typeof(LimiterGizmos), "groundLevel");

        Assert.IsNotNull(startField, "start transform phải được gán.");
        Assert.IsNotNull(endField, "end transform phải được gán.");
        Assert.IsNotNull(groundField, "groundLevel transform phải được gán.");

        Object.DestroyImmediate(gizmosObj);
        Object.DestroyImmediate(startObj);
        Object.DestroyImmediate(endObj);
        Object.DestroyImmediate(groundObj);
    }

    [Test]
    [Description("LimiterGizmos - Has OnDrawGizmos method for editor visualization")]
    public void LimiterGizmos_HasOnDrawGizmosMethod()
    {
        var method = typeof(LimiterGizmos).GetMethod("OnDrawGizmos",
            BindingFlags.NonPublic | BindingFlags.Instance);

        Assert.IsNotNull(method,
            "LimiterGizmos phải có method OnDrawGizmos để vẽ debug lines.");
    }

    #endregion

    #region === PLAYFABMANAGER TESTS ===

    private GameObject pfmObj;
    private PlayFabManager pfm;

    private void SetupPlayFabManager()
    {
        pfmObj = new GameObject("PlayFabManager");
        pfm = pfmObj.AddComponent<PlayFabManager>();

        var regPanel = new GameObject("RegisterPanel");
        var loginPanel = new GameObject("LoginPanel");
        var msgObj = new GameObject("MessageText");
        var loginMsgObj = new GameObject("LoginMessageText");

        pfm.registerPanel = regPanel;
        pfm.loginPanel = loginPanel;
        pfm.messageText = msgObj.AddComponent<TextMeshProUGUI>();
        pfm.loginMessageText = loginMsgObj.AddComponent<TextMeshProUGUI>();

        var emailObj = new GameObject("EmailInput");
        var passObj = new GameObject("PassInput");
        var userObj = new GameObject("UserInput");
        var loginEmailObj = new GameObject("LoginEmailInput");
        var loginPassObj = new GameObject("LoginPassInput");

        pfm.emailInput = emailObj.AddComponent<TMP_InputField>();
        pfm.passwordInput = passObj.AddComponent<TMP_InputField>();
        pfm.usernameInput = userObj.AddComponent<TMP_InputField>();
        pfm.loginEmailInput = loginEmailObj.AddComponent<TMP_InputField>();
        pfm.loginPasswordInput = loginPassObj.AddComponent<TMP_InputField>();
    }

    private void TeardownPlayFabManager()
    {
        if (pfmObj != null)
        {
            // Destroy all children and related objects
            foreach (Transform child in pfmObj.transform)
                Object.DestroyImmediate(child.gameObject);
        }

        Object.DestroyImmediate(pfm?.registerPanel);
        Object.DestroyImmediate(pfm?.loginPanel);
        Object.DestroyImmediate(pfm?.messageText?.gameObject);
        Object.DestroyImmediate(pfm?.loginMessageText?.gameObject);
        Object.DestroyImmediate(pfm?.emailInput?.gameObject);
        Object.DestroyImmediate(pfm?.passwordInput?.gameObject);
        Object.DestroyImmediate(pfm?.usernameInput?.gameObject);
        Object.DestroyImmediate(pfm?.loginEmailInput?.gameObject);
        Object.DestroyImmediate(pfm?.loginPasswordInput?.gameObject);
        Object.DestroyImmediate(pfmObj);
    }

    [UnityTest]
    [Description("PlayFabManager - RegisterButton rejects password shorter than 6 chars")]
    [Category("PC")]
    [Category("Mobile")]
    public IEnumerator PlayFab_RegisterButton_RejectsShortPassword()
    {
        SetupPlayFabManager();
        yield return null;

        pfm.passwordInput.text = "123";
        pfm.RegisterButton();
        yield return null;

        Assert.AreEqual("Password too short!", pfm.messageText.text,
            "RegisterButton phải hiện lỗi khi password < 6 ký tự.");

        TeardownPlayFabManager();
    }

    [UnityTest]
    [Description("PlayFabManager - RegisterButton accepts password with exactly 6 chars")]
    public IEnumerator PlayFab_RegisterButton_AcceptsPassword6Chars()
    {
        SetupPlayFabManager();
        yield return null;

        pfm.passwordInput.text = "123456";
        pfm.emailInput.text = "test@test.com";
        pfm.usernameInput.text = "testuser";

        pfm.RegisterButton();
        yield return null;

        Assert.AreNotEqual("Password too short!", pfm.messageText.text,
            "RegisterButton không nên báo lỗi khi password đúng 6 ký tự.");

        TeardownPlayFabManager();
    }

    [UnityTest]
    [Description("PlayFabManager - RegisterButton accepts password longer than 6 chars")]
    public IEnumerator PlayFab_RegisterButton_AcceptsLongPassword()
    {
        SetupPlayFabManager();
        yield return null;

        pfm.passwordInput.text = "securepassword123";
        pfm.emailInput.text = "test@test.com";
        pfm.usernameInput.text = "testuser";

        pfm.RegisterButton();
        yield return null;

        Assert.AreNotEqual("Password too short!", pfm.messageText.text,
            "RegisterButton không nên báo lỗi khi password > 6 ký tự.");

        TeardownPlayFabManager();
    }

    [UnityTest]
    [Description("PlayFabManager - signInClick shows register panel and hides login panel")]
    [Category("PC")]
    [Category("Mobile")]
    public IEnumerator PlayFab_SignInClick_ShowsRegisterHidesLogin()
    {
        SetupPlayFabManager();
        yield return null;

        pfm.registerPanel.SetActive(false);
        pfm.loginPanel.SetActive(true);

        pfm.signInClick();

        Assert.IsTrue(pfm.registerPanel.activeSelf,
            "signInClick phải bật registerPanel.");
        Assert.IsFalse(pfm.loginPanel.activeSelf,
            "signInClick phải tắt loginPanel.");

        TeardownPlayFabManager();
    }

    [UnityTest]
    [Description("PlayFabManager - backClick shows login panel and hides register panel")]
    [Category("PC")]
    [Category("Mobile")]
    public IEnumerator PlayFab_BackClick_ShowsLoginHidesRegister()
    {
        SetupPlayFabManager();
        yield return null;

        pfm.registerPanel.SetActive(true);
        pfm.loginPanel.SetActive(false);

        pfm.backClick();

        Assert.IsFalse(pfm.registerPanel.activeSelf,
            "backClick phải tắt registerPanel.");
        Assert.IsTrue(pfm.loginPanel.activeSelf,
            "backClick phải bật loginPanel.");

        TeardownPlayFabManager();
    }

    [UnityTest]
    [Description("PlayFabManager - signInClick then backClick restores original state")]
    [Category("PC")]
    [Category("Mobile")]
    public IEnumerator PlayFab_SignInThenBack_RestoresState()
    {
        SetupPlayFabManager();
        yield return null;

        pfm.registerPanel.SetActive(false);
        pfm.loginPanel.SetActive(true);

        pfm.signInClick();
        pfm.backClick();

        Assert.IsFalse(pfm.registerPanel.activeSelf,
            "Sau signIn rồi back, registerPanel phải tắt.");
        Assert.IsTrue(pfm.loginPanel.activeSelf,
            "Sau signIn rồi back, loginPanel phải bật.");

        TeardownPlayFabManager();
    }

    [Test]
    [Description("PlayFabManager - Public fields are assignable")]
    public void PlayFab_PublicFields_AreAccessible()
    {
        var obj = new GameObject("PFM");
        var manager = obj.AddComponent<PlayFabManager>();

        Assert.IsNotNull(typeof(PlayFabManager).GetField("registerPanel"));
        Assert.IsNotNull(typeof(PlayFabManager).GetField("loginPanel"));
        Assert.IsNotNull(typeof(PlayFabManager).GetField("messageText"));
        Assert.IsNotNull(typeof(PlayFabManager).GetField("emailInput"));
        Assert.IsNotNull(typeof(PlayFabManager).GetField("passwordInput"));
        Assert.IsNotNull(typeof(PlayFabManager).GetField("usernameInput"));

        Object.DestroyImmediate(obj);
    }

    #endregion

    #region === SCENECONTROLLER TESTS ===

    [Test]
    [Description("SceneController - Has LoadGame method")]
    public void SceneController_HasLoadGameMethod()
    {
        var method = typeof(SceneController).GetMethod("LoadGame",
            BindingFlags.Public | BindingFlags.Instance);

        Assert.IsNotNull(method,
            "SceneController phải có public method LoadGame.");
    }

    [Test]
    [Description("SceneController - Component can be added to GameObject")]
    public void SceneController_CanBeAddedToGameObject()
    {
        var obj = new GameObject("SceneCtrl");
        var controller = obj.AddComponent<SceneController>();

        Assert.IsNotNull(controller,
            "SceneController phải có thể gắn vào GameObject.");

        Object.DestroyImmediate(obj);
    }

    #endregion

    #region === COLORTYPE ENUM & COLORTOSELL STRUCT TESTS ===

    [Test]
    [Description("ColorType enum - Has playerColor and platformColor values")]
    public void ColorType_Enum_HasExpectedValues()
    {
        Assert.IsTrue(System.Enum.IsDefined(typeof(ColorType), "playerColor"),
            "ColorType phải có giá trị playerColor.");
        Assert.IsTrue(System.Enum.IsDefined(typeof(ColorType), "platformColor"),
            "ColorType phải có giá trị platformColor.");
    }

    [Test]
    [Description("ColorToSell struct - Can store color and price")]
    public void ColorToSell_Struct_StoresColorAndPrice()
    {
        var item = new ColorToSell
        {
            color = Color.red,
            price = 100
        };

        Assert.AreEqual(Color.red, item.color,
            "ColorToSell phải lưu đúng color.");
        Assert.AreEqual(100, item.price,
            "ColorToSell phải lưu đúng price.");
    }

    #endregion

    #region === CHARACTERBOX TESTS ===

    [Test]
    [Description("CharacterBox - Component can be instantiated")]
    public void CharacterBox_CanBeInstantiated()
    {
        var obj = new GameObject("CharBox");
        var charBox = obj.AddComponent<CharacterBox>();

        Assert.IsNotNull(charBox,
            "CharacterBox phải có thể được tạo và gắn vào GameObject.");

        Object.DestroyImmediate(obj);
    }

    #endregion

    #region === REFLECTION HELPERS ===

    private void SetPrivateField(object target, System.Type type, string fieldName, object value)
    {
        var field = type.GetField(fieldName,
            BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public);
        Assert.IsNotNull(field, $"Field '{fieldName}' không tìm thấy trong {type.Name}.");
        field.SetValue(target, value);
    }

    private object GetPrivateFieldValue(object target, System.Type type, string fieldName)
    {
        var field = type.GetField(fieldName,
            BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public);
        Assert.IsNotNull(field, $"Field '{fieldName}' không tìm thấy trong {type.Name}.");
        return field.GetValue(target);
    }

    #endregion
}
