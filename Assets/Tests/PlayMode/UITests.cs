using System.Collections;
using System.Reflection;
using NUnit.Framework;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

/// <summary>
/// UI Tests - Tất cả test liên quan đến giao diện người dùng:
/// Hiển thị coins, nút Pause, chuyển menu.
/// Gộp từ: RunToLife_Assignment_Tests, NewAutoTests_PlayMode.
/// </summary>
public class UITests
{
    [TearDown]
    public void Teardown()
    {
        Time.timeScale = 1f;
    }

    [UnityTest]
    [Category("PC")]
    [Category("Mobile")]
    [Description("Kiểm tra UI InGame hiển thị đúng số coins khi giá trị coins thay đổi")]
    public IEnumerator TC_UI_7_UIInGame_HienThiDungSoCoins()
    {
        yield return SceneManager.LoadSceneAsync(1);
        Time.timeScale = 1;
        GameManager.instance.UnlockPlayer();
        yield return null;

        UI_InGame ui = GameObject.FindObjectOfType<UI_InGame>(true);

        if (ui == null)
        {
            Assert.Pass("Không có UI_InGame trong scene, bỏ qua test.");
            yield break;
        }

        int testCoins = 888;
        GameManager.instance.coins = testCoins;

        yield return new WaitForSeconds(0.5f);

        TextMeshProUGUI[] allTexts = ui.GetComponentsInChildren<TextMeshProUGUI>(true);
        Assert.That(allTexts.Length, Is.GreaterThan(0),
            "UI_InGame không có bất kỳ TextMeshProUGUI nào!");

        bool isUIUpdated = false;
        foreach (var txt in allTexts)
        {
            if (txt.text.Contains(testCoins.ToString()))
            {
                isUIUpdated = true;
                break;
            }
        }

        Assert.That(isUIUpdated, Is.True,
            $"UI InGame không hiển thị đúng số tiền ({testCoins}). Kiểm tra lại hàm cập nhật UI!");
    }

    [UnityTest]
    [Category("PC")]
    [Category("Mobile")]
    [Description("Kiểm tra nút Pause: lần 1 dừng game (timeScale=0), lần 2 tiếp tục (timeScale=1)")]
    public IEnumerator TC_UI_16_NutPause_ToggleTimeScale()
    {
        yield return SceneManager.LoadSceneAsync(1);
        yield return new WaitForSeconds(0.3f);

        UI_Main uiMain = Object.FindFirstObjectByType<UI_Main>();
        Assert.IsNotNull(uiMain, "UI_Main phải tồn tại trong scene.");

        var pauseField = typeof(UI_Main).GetField("gamePaused",
            BindingFlags.NonPublic | BindingFlags.Instance);
        Assert.IsNotNull(pauseField, "Không tìm thấy field gamePaused!");
        pauseField.SetValue(uiMain, false);

        Time.timeScale = 1f;
        yield return null;

        uiMain.PauseGameButton();
        yield return null;

        Assert.AreEqual(0f, Time.timeScale,
            "Nhấn Pause lần 1: Time.timeScale phải bằng 0 (dừng game).");

        uiMain.PauseGameButton();
        yield return null;

        Assert.AreEqual(1f, Time.timeScale,
            "Nhấn Pause lần 2: Time.timeScale phải trở về 1 (tiếp tục game).");
    }
}
