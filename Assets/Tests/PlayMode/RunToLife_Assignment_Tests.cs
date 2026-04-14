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

    [UnityTest]
    [Description("TC_ANIMATION_03 - Kiem tra nhan vat nhay khi bam JumpButton")]
    public IEnumerator TC_ANIMATION_03_PlayerJump_AfterJumpButton_RigidbodyHasUpwardVelocity()
    {
        Player player = GameManager.instance.player;
        Rigidbody2D rb = player.GetComponent<Rigidbody2D>();

        yield return new WaitForSeconds(1f);

        float velocityBeforeJump = rb.linearVelocity.y;
        player.JumpButton();
        yield return new WaitForFixedUpdate();

        Assert.That(rb.linearVelocity.y, Is.GreaterThan(velocityBeforeJump),
            "Nhan vat khong co luc bay len sau khi nhay!");
        Assert.That(rb.linearVelocity.y, Is.GreaterThan(0),
            "Van toc Y phai duong ngay sau khi nhay!");

        yield return new WaitForSeconds(0.5f);
    }

    [UnityTest]
    [Description("TC_SOUND_3 - Kiem tra Mute toan bo am thanh")]
    public IEnumerator TC_SOUND_3_MuteButton_TogglesAudioListenerVolume_BetweenZeroAndOne()
    {
        UI_Main ui = GameObject.FindObjectOfType<UI_Main>();
        Assert.That(ui, Is.Not.Null, "Khong tim thay UI_Main trong scene!");

        ui.MuteButton();
        Assert.That(AudioListener.volume, Is.EqualTo(0).Within(0.001f),
            "Volume phai bang 0 sau khi nhan Mute lan dau!");

        ui.MuteButton();
        Assert.That(AudioListener.volume, Is.EqualTo(1).Within(0.001f),
            "Volume phai tro ve 1 sau khi nhan Mute lan hai (Unmute)!");

        yield return null;
    }

    [UnityTest]
    [Description("TC_UI_7 - Cap nhat so Coins tren UI trong game")]
    public IEnumerator TC_UI_7_UpdateUI_WhenCoinsChanged_InGameUIDisplaysCorrectValue()
    {
        UI_InGame ui = GameObject.FindObjectOfType<UI_InGame>(true);

        if (ui == null)
        {
            Assert.Pass("Khong co UI_InGame trong scene, bo qua test.");
            yield break;
        }

        int testCoins = 888;
        GameManager.instance.coins = testCoins;

        yield return new WaitForSeconds(0.5f);

        TextMeshProUGUI[] allTexts = ui.GetComponentsInChildren<TextMeshProUGUI>(true);
        Assert.That(allTexts.Length, Is.GreaterThan(0),
            "UI_InGame khong co bat ky TextMeshProUGUI nao!");

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
            $"UI InGame khong hien thi dung so tien ({testCoins}). Kiem tra lai ham cap nhat UI!");
    }

    [UnityTest]
    [Description("TC_LOGIN_01 - Luu du lieu game vao PlayerPrefs")]
    public IEnumerator TC_LOGIN_01_SaveInfo_AfterSettingCoins_PlayerPrefsStoresCorrectValue()
    {
        PlayerPrefs.DeleteKey("Coins");
        PlayerPrefs.Save();

        int coinsToSave = 50;
        GameManager.instance.coins = coinsToSave;
        GameManager.instance.SaveInfo();

        int savedCoins = PlayerPrefs.GetInt("Coins", -1);

        Assert.That(savedCoins, Is.Not.EqualTo(-1),
            "PlayerPrefs khong co key 'Coins' sau khi SaveInfo()!");
        Assert.That(savedCoins, Is.EqualTo(coinsToSave),
            $"SaveInfo() luu sai gia tri! Mong doi {coinsToSave}, thuc te nhan duoc {savedCoins}.");

        yield return null;
    }

    [UnityTest]
    [Category("PC")]
    [Category("Mobile")]
    [Description("TC_UI_2 - Kiem tra he thong quang duong trong thoi gian choi game")]
    public IEnumerator TC_UI_2_DistanceTracking_AfterTimeElapsed_PlayerMovesForward()
    {
        Player player = GameManager.instance.player;
        Assert.That(player, Is.Not.Null, "Khong tim thay Player trong GameManager!");

        float startPositionX = player.transform.position.x;

        yield return new WaitForSeconds(2.0f);

        float endPositionX = player.transform.position.x;
        float distanceTravelled = endPositionX - startPositionX;

        Assert.That(endPositionX, Is.GreaterThan(startPositionX),
            $"Nhan vat khong di chuyen! Vi tri ban dau: {startPositionX:F2}, vi tri sau 2 giay: {endPositionX:F2}.");
        Assert.That(distanceTravelled, Is.GreaterThan(0),
            $"Quang duong di duoc phai lon hon 0, thuc te: {distanceTravelled:F2}.");
    }
}