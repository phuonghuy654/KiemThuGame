using System.Collections;
using NUnit.Framework;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

public class RunToLife_Assignment_Tests
{
    [UnitySetUp]
    public IEnumerator Setup()
    {
        yield return SceneManager.LoadSceneAsync(1);

        Time.timeScale = 1;
        if (GameManager.instance != null)
        {
            GameManager.instance.UnlockPlayer();
        }
        yield return null;
    }

    // ========================================================
    // [LAB 6] - UNIT TEST: ÂM THANH & NHẢY (CÓ HÌNH ẢNH)
    // ========================================================

    [UnityTest]
    public IEnumerator Lab6_Visual_PlayerJump()
    {
        Player player = GameManager.instance.player;
        Rigidbody2D rb = player.GetComponent<Rigidbody2D>();

        player.JumpButton();

        yield return new WaitForFixedUpdate();

        Assert.That(rb.linearVelocity.y, Is.GreaterThan(0), "Lỗi: Nhân vật không có lực bay lên!");

        yield return new WaitForSeconds(0.5f);
    }

    [UnityTest]
    public IEnumerator Lab6_Logic_MuteButton()
    {
        UI_Main ui = GameObject.FindObjectOfType<UI_Main>();

        ui.MuteButton();
        Assert.That(AudioListener.volume, Is.EqualTo(0), "Lỗi: Volume không về 0 khi Mute!");

        ui.MuteButton();
        Assert.That(AudioListener.volume, Is.EqualTo(1), "Lỗi: Volume không quay lại 1 khi Unmute!");

        yield return null;
    }

    // ========================================================
    // [LAB 7] - INTEGRATION: TÍCH HỢP UI & HỆ THỐNG LƯU TRỮ
    // ========================================================

    [UnityTest]
    public IEnumerator Lab7_Integration_UpdateUI_InGame()
    {
        UI_InGame ui = GameObject.FindObjectOfType<UI_InGame>(true);

        if (ui != null)
        {
            GameManager.instance.coins = 888;

            yield return new WaitForSeconds(0.5f);

            TextMeshProUGUI[] allTexts = ui.GetComponentsInChildren<TextMeshProUGUI>(true);
            bool isUIUpdated = false;

            foreach (var txt in allTexts)
            {
                if (txt.text.Contains("888"))
                {
                    isUIUpdated = true;
                    break;
                }
            }

            Assert.That(isUIUpdated, Is.True, "Lỗi: UI InGame không chịu cập nhật hiển thị số tiền!");
        }
        else
        {
            Assert.Pass();
        }
    }

    [UnityTest]
    public IEnumerator Lab7_Integration_SaveGameData()
    {
        int initialCoins = PlayerPrefs.GetInt("Coins", 0);

        GameManager.instance.coins = 50;

        GameManager.instance.SaveInfo();

        int savedCoins = PlayerPrefs.GetInt("Coins");

        Assert.That(savedCoins, Is.EqualTo(initialCoins + 50), "Lỗi: Hệ thống SaveInfo() không lưu đúng dữ liệu vào máy!");

        yield return null;
    }

    // [LAB 8] - Parallel: MÔI TRƯỜNG & KHOẢNG CÁCH
    [UnityTest]
    [Category("PC")]
    [Category("Mobile")]
    public IEnumerator Lab8_Parallel_DistanceTracking()
    {
        float startPositionX = GameManager.instance.player.transform.position.x;

        yield return new WaitForSeconds(2.0f);

        float endPositionX = GameManager.instance.player.transform.position.x;

        Assert.That(endPositionX, Is.GreaterThan(startPositionX), "Lỗi: Nhân vật đứng yên, không tính được quãng đường!");
    }
}