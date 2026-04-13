using System.Collections;
using System.Reflection;
using NUnit.Framework;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

public class PlayModeFailTests
{
    [UnityTest]
    [Description("TC_REGISTER_19 - Kiem tra spawn map lien tuc (integration, Expected: Fail)")]
    public IEnumerator TC19_LevelGenerator_AfterPlayerMoves_MapPartsChange()
    {
        yield return SceneManager.LoadSceneAsync(1);
        yield return new WaitForSeconds(1f);

        LevelGenerator generator = GameObject.FindObjectOfType<LevelGenerator>();
        Assert.That(generator, Is.Not.Null, "Khong tim thay LevelGenerator trong scene!");

        Transform player = GameManager.instance.player.transform;
        float startX = player.position.x;
        int childCountAtStart = generator.transform.childCount;

        Assert.That(childCountAtStart, Is.GreaterThan(0),
            "LevelGenerator phai co it nhat 1 part khi bat dau!");

        yield return new WaitForSeconds(4f);

        float endX = player.position.x;

        Assert.That(endX, Is.GreaterThan(startX),
            $"Player khong di chuyen! Vi tri ban dau: {startX:F1}, sau 4s: {endX:F1}. " +
            "Kiem tra lai playerUnlocked.");

        FieldInfo nextPartField = typeof(LevelGenerator).GetField("nextPartPosition",
            BindingFlags.NonPublic | BindingFlags.Instance);
        Vector3 nextPartPos = (Vector3)nextPartField.GetValue(generator);

        Assert.That(nextPartPos.x, Is.GreaterThan(startX),
            $"nextPartPosition ({nextPartPos.x:F1}) khong tien theo player ({endX:F1}). " +
            "Map khong duoc spawn them — day la bug TC_REGISTER_19!");

        FieldInfo distField = typeof(LevelGenerator).GetField("distanceToSpawn",
            BindingFlags.NonPublic | BindingFlags.Instance);
        float distanceToSpawn = (float)distField.GetValue(generator);

        Assert.That(distanceToSpawn, Is.GreaterThan(0),
            "distanceToSpawn = 0, GeneratePlatform() se khong bao gio spawn part moi!");
    }

    [UnityTest]
    [Description("TC_REGISTER_34 - Kiem tra thanh chinh am luong (Expected: Fail - tieng mua van con)")]
    public IEnumerator TC34_VolumeSlider_WhenMutedToZero_AllAudioSourcesAreSilent()
    {
        yield return SceneManager.LoadSceneAsync(1);
        yield return new WaitForSeconds(0.5f);

        UI_Main ui = GameObject.FindObjectOfType<UI_Main>();
        Assert.That(ui, Is.Not.Null, "Khong tim thay UI_Main trong scene!");

        UI_VolumeSlider[] sliders = GameObject.FindObjectsOfType<UI_VolumeSlider>(true);
        Assert.That(sliders.Length, Is.GreaterThan(0),
            "Khong tim thay UI_VolumeSlider nao trong scene!");

        ui.MuteButton();
        yield return null;

        Assert.That(AudioListener.volume, Is.EqualTo(0).Within(0.001f),
            "AudioListener.volume phai = 0 sau khi Mute, nhung van con am thanh!");

        AudioSource[] allSources = GameObject.FindObjectsOfType<AudioSource>(true);
        foreach (var source in allSources)
        {
            if (source.isPlaying)
            {
                Assert.That(source.volume, Is.EqualTo(0).Within(0.001f),
                    $"AudioSource '{source.gameObject.name}' van dang phat am thanh voi volume = {source.volume} " +
                    "sau khi Mute! Day la bug TC_REGISTER_34 (tieng mua van con).");
            }
        }

        ui.MuteButton();
        yield return null;

        Assert.That(AudioListener.volume, Is.EqualTo(1).Within(0.001f),
            "AudioListener.volume phai = 1 sau khi Unmute!");
    }

    [UnityTest]
    [Description("TC_REGISTER_06 - Dang nhap khi de trong mat khau (Expected: Fail - khong hien thong bao loi)")]
    public IEnumerator TC06_Login_WithEmptyPassword_ShowsErrorMessage()
    {
        yield return SceneManager.LoadSceneAsync(0);
        yield return new WaitForSeconds(0.5f);

        PlayFabManager pfManager = GameObject.FindObjectOfType<PlayFabManager>(true);
        Assert.That(pfManager, Is.Not.Null, "Khong tim thay PlayFabManager trong scene Login!");

        pfManager.loginEmailInput.text = "test@example.com";
        pfManager.loginPasswordInput.text = "";

        string messageBeforeLogin = pfManager.loginMessageText.text;

        pfManager.LoginButton();

        yield return new WaitForSeconds(3f);

        string messageAfterLogin = pfManager.loginMessageText.text;
        Assert.That(messageAfterLogin, Is.Not.Empty,
            "loginMessageText khong hien thong bao gi sau khi dang nhap voi mat khau trong!");

        Assert.That(messageAfterLogin, Is.Not.EqualTo(messageBeforeLogin),
            "loginMessageText khong thay doi — he thong im lang khi mat khau trong. " +
            "Can them validation: if (loginPasswordInput.text == '') { loginMessageText.text = 'Vui long nhap mat khau'; return; }");

        Assert.That(SceneManager.GetActiveScene().buildIndex, Is.EqualTo(0),
            "He thong cho phep chuyen scene du mat khau trong — day la bug nghiem trong!");

        Assert.That(messageAfterLogin.Length, Is.GreaterThan(3),
            $"Thong bao loi qua ngan hoac khong ro rang: '{messageAfterLogin}'");
    }
}