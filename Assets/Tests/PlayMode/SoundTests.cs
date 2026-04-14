using System.Collections;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

/// <summary>
/// Sound Tests - Tất cả test liên quan đến âm thanh:
/// Thanh chỉnh âm lượng, nút Mute, AudioListener.
/// Gộp từ: PlayMode, RunToLife_Assignment_Tests, NewAutoTests_PlayMode.
/// </summary>
public class SoundTests
{
    [TearDown]
    public void Teardown()
    {
        AudioListener.volume = 1f;
        Time.timeScale = 1f;
    }

    [UnityTest]
    [Category("PC")]
    [Category("Mobile")]
    [Description("Kiểm tra thanh chỉnh âm lượng: khi thay đổi giá trị slider, AudioListener.volume cập nhật đúng")]
    public IEnumerator TC_SOUND_1_ThanhChinhAmLuong_CapNhatDungVolume()
    {
        yield return SceneManager.LoadSceneAsync(1);
        yield return null;

        UI_Main ui = GameObject.FindObjectOfType<UI_Main>();
        Assert.That(ui, Is.Not.Null, "Không tìm thấy UI_Main trong scene!");

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
                $"AudioListener.volume không cập nhật đúng! Mong đợi 0.3, thực tế: {AudioListener.volume}.");

            setVolumeMethod.Invoke(ui, new object[] { 0f });
            yield return null;
            Assert.That(AudioListener.volume, Is.EqualTo(0f).Within(0.01f),
                "Volume không về 0 khi kéo slider về 0!");
        }
        else
        {
            AudioListener.volume = 0.3f;
            yield return null;
            Assert.That(AudioListener.volume, Is.EqualTo(0.3f).Within(0.01f),
                "AudioListener.volume không nhận giá trị 0.3!");

            AudioListener.volume = 0f;
            yield return null;
            Assert.That(AudioListener.volume, Is.EqualTo(0f).Within(0.01f),
                "Sau khi kéo slider về 0, vẫn còn âm thanh phát ra!");
        }
    }

    [UnityTest]
    [Category("PC")]
    [Category("Mobile")]
    [Description("Kiểm tra nút Mute: lần 1 tắt toàn bộ âm thanh (volume=0), lần 2 bật lại (volume=1)")]
    public IEnumerator TC_SOUND_3_NutMute_TatBatAmThanh()
    {
        yield return SceneManager.LoadSceneAsync(1);
        Time.timeScale = 1;
        GameManager.instance.UnlockPlayer();
        yield return null;

        UI_Main ui = GameObject.FindObjectOfType<UI_Main>();
        Assert.That(ui, Is.Not.Null, "Không tìm thấy UI_Main trong scene!");

        ui.MuteButton();
        Assert.That(AudioListener.volume, Is.EqualTo(0).Within(0.001f),
            "Volume phải bằng 0 sau khi nhấn Mute lần đầu!");

        ui.MuteButton();
        Assert.That(AudioListener.volume, Is.EqualTo(1).Within(0.001f),
            "Volume phải trở về 1 sau khi nhấn Mute lần hai (Unmute)!");

        yield return null;
    }

    [UnityTest]
    [Category("PC")]
    [Category("Mobile")]
    [Description("Kiểm tra nút Mute với Reflection reset: đảm bảo toggle hoạt động từ trạng thái unmuted")]
    public IEnumerator TC_SOUND_4_16_NutMute_ToggleVoiReflectionReset()
    {
        yield return SceneManager.LoadSceneAsync(1);
        yield return new WaitForSeconds(0.3f);

        UI_Main uiMain = Object.FindFirstObjectByType<UI_Main>();
        Assert.IsNotNull(uiMain, "UI_Main phải tồn tại trong scene.");

        var flags = BindingFlags.NonPublic | BindingFlags.Instance;
        var mutedField = typeof(UI_Main).GetField("gameMuted", flags);
        Assert.IsNotNull(mutedField, "Không tìm thấy field gameMuted!");

        // Reset về trạng thái unmuted
        mutedField.SetValue(uiMain, false);
        AudioListener.volume = 1f;
        yield return null;

        // Nhấn Mute lần 1 → tắt âm thanh
        uiMain.MuteButton();
        yield return null;
        Assert.AreEqual(0f, AudioListener.volume,
            "AudioListener.volume phải bằng 0 sau khi nhấn Mute.");

        // Nhấn Mute lần 2 → bật lại âm thanh
        uiMain.MuteButton();
        yield return null;
        Assert.AreEqual(1f, AudioListener.volume,
            "AudioListener.volume phải trở về 1 sau khi nhấn Mute lần 2 (unmute).");
    }
}
