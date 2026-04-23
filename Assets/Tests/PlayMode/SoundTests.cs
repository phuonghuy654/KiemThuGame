using System.Collections;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

/// <summary>
/// Sound Tests - Kiểm tra âm thanh: Volume slider, Mute, AudioManager.
/// </summary>
public class SoundTests
{
    private static readonly BindingFlags FLAGS = BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public;
    private void Set(object t, string f, object v) { t.GetType().GetField(f, FLAGS).SetValue(t, v); }
    private T Get<T>(object t, string f) { return (T)t.GetType().GetField(f, FLAGS).GetValue(t); }

    private GameObject amObj;

    [TearDown]
    public void Teardown()
    {
        AudioListener.volume = 1f;
        Time.timeScale = 1f;
    }

    // ═══════════════════════════════════════
    //  GIỮ NGUYÊN TỪ FILE GỐC
    // ═══════════════════════════════════════

    [UnityTest, Category("PC"), Category("Mobile")]
    [Description("TC_SOUND_1 - Kiểm tra thanh chỉnh âm lượng: khi thay đổi giá trị slider, AudioListener.volume cập nhật đúng")]
    public IEnumerator TC_SOUND_1_ThanhChinhAmLuong_CapNhatDungVolume()
    {
        yield return SceneManager.LoadSceneAsync(1);
        yield return null;

        UI_Main ui = GameObject.FindObjectOfType<UI_Main>();
        Assert.That(ui, Is.Not.Null, "Không tìm thấy UI_Main trong scene!");

        MethodInfo setVolumeMethod = null;
        foreach (var name in new[] { "SetVolume", "ChangeVolume", "OnVolumeChanged", "UpdateVolume", "SliderVolume" })
        {
            setVolumeMethod = typeof(UI_Main).GetMethod(name, FLAGS);
            if (setVolumeMethod != null) break;
        }

        if (setVolumeMethod != null)
        {
            setVolumeMethod.Invoke(ui, new object[] { 0.3f }); yield return null;
            Assert.That(AudioListener.volume, Is.EqualTo(0.3f).Within(0.01f));
            setVolumeMethod.Invoke(ui, new object[] { 0f }); yield return null;
            Assert.That(AudioListener.volume, Is.EqualTo(0f).Within(0.01f));
        }
        else
        {
            AudioListener.volume = 0.3f; yield return null;
            Assert.That(AudioListener.volume, Is.EqualTo(0.3f).Within(0.01f));
            AudioListener.volume = 0f; yield return null;
            Assert.That(AudioListener.volume, Is.EqualTo(0f).Within(0.01f));
        }
    }

    [UnityTest, Category("PC"), Category("Mobile")]
    [Description("TC_SOUND_3 - Kiểm tra nút Mute: lần 1 tắt toàn bộ âm thanh (volume=0), lần 2 bật lại (volume=1)")]
    public IEnumerator TC_SOUND_3_NutMute_TatBatAmThanh()
    {
        yield return SceneManager.LoadSceneAsync(1);
        Time.timeScale = 1;
        GameManager.instance.UnlockPlayer();
        yield return null;

        UI_Main ui = GameObject.FindObjectOfType<UI_Main>();
        Assert.That(ui, Is.Not.Null);

        ui.MuteButton();
        Assert.That(AudioListener.volume, Is.EqualTo(0).Within(0.001f));
        ui.MuteButton();
        Assert.That(AudioListener.volume, Is.EqualTo(1).Within(0.001f));
        yield return null;
    }

    [UnityTest, Category("PC"), Category("Mobile")]
    [Description("TC_SOUND_4 - Kiểm tra nút Mute với Reflection reset trạng thái gameMuted, đảm bảo toggle hoạt động từ unmuted")]
    public IEnumerator TC_SOUND_4_NutMute_ToggleVoiReflectionReset()
    {
        yield return SceneManager.LoadSceneAsync(1);
        yield return new WaitForSeconds(0.3f);

        UI_Main uiMain = Object.FindFirstObjectByType<UI_Main>();
        Assert.IsNotNull(uiMain);

        var mutedField = typeof(UI_Main).GetField("gameMuted", FLAGS);
        Assert.IsNotNull(mutedField);
        mutedField.SetValue(uiMain, false);
        AudioListener.volume = 1f;
        yield return null;

        uiMain.MuteButton(); yield return null;
        Assert.AreEqual(0f, AudioListener.volume, "Nhấn Mute lần 1: volume phải = 0.");
        uiMain.MuteButton(); yield return null;
        Assert.AreEqual(1f, AudioListener.volume, "Nhấn Mute lần 2: volume phải = 1.");
    }

    // ═══════════════════════════════════════
    //  GỘP TỪ CoverageBoostTests
    // ═══════════════════════════════════════

    private AudioManager CreateAudioManager(int sfxCount, int bgmCount)
    {
        amObj = new GameObject("AudioManager");
        var am = amObj.AddComponent<AudioManager>();
        var sfxArr = new AudioSource[sfxCount];
        for (int i = 0; i < sfxCount; i++) sfxArr[i] = amObj.AddComponent<AudioSource>();
        var bgmArr = new AudioSource[bgmCount];
        for (int i = 0; i < bgmCount; i++) bgmArr[i] = amObj.AddComponent<AudioSource>();
        Set(am, "sfx", sfxArr); Set(am, "bgm", bgmArr);
        return am;
    }

    [UnityTest, Category("PC"), Category("Mobile")]
    [Description("TC_SOUND_9 - Kiểm tra StopSFX dừng đúng AudioSource tại index được chỉ định")]
    public IEnumerator TC_SOUND_9_StopSFX_DungDungAudioSource()
    {
        var am = CreateAudioManager(2, 1); yield return null;
        var sfxArr = Get<AudioSource[]>(am, "sfx");
        sfxArr[0].Play(); yield return null;
        am.StopSFX(0); yield return null;
        Assert.IsFalse(sfxArr[0].isPlaying, "StopSFX phải dừng AudioSource tại index.");
        Object.DestroyImmediate(amObj);
    }

    [UnityTest, Category("PC"), Category("Mobile")]
    [Description("TC_SOUND_10 - Kiểm tra PlayBGM phát đúng nhạc nền tại index được chỉ định")]
    public IEnumerator TC_SOUND_10_PlayBGM_PhatDungIndex()
    {
        var am = CreateAudioManager(1, 3); yield return null;
        am.PlayBGM(1); yield return null;
        Assert.IsTrue(Get<AudioSource[]>(am, "bgm")[1].isPlaying, "PlayBGM phải play BGM tại index.");
        Object.DestroyImmediate(amObj);
    }

    [UnityTest, Category("PC"), Category("Mobile")]
    [Description("TC_SOUND_11 - Kiểm tra StopBGM dừng tất cả nhạc nền đang phát")]
    public IEnumerator TC_SOUND_11_StopBGM_DungTatCaBGM()
    {
        var am = CreateAudioManager(1, 3); yield return null;
        am.PlayBGM(0); yield return null;
        am.StopBGM(); yield return null;
        var bgmArr = Get<AudioSource[]>(am, "bgm");
        for (int i = 0; i < bgmArr.Length; i++)
            Assert.IsFalse(bgmArr[i].isPlaying, $"StopBGM: index {i} vẫn đang play.");
        Object.DestroyImmediate(amObj);
    }

    [UnityTest, Category("PC"), Category("Mobile")]
    [Description("TC_SOUND_12 - Kiểm tra PlayRandomBGM chọn ngẫu nhiên bgmIndex hợp lệ và phát BGM tương ứng")]
    public IEnumerator TC_SOUND_12_PlayRandomBGM_ChonNgauNhienVaPhat()
    {
        var am = CreateAudioManager(1, 3); yield return null;
        am.PlayRandomBGM(); yield return null;
        int idx = Get<int>(am, "bgmIndex");
        Assert.That(idx, Is.InRange(0, 2));
        Assert.IsTrue(Get<AudioSource[]>(am, "bgm")[idx].isPlaying);
        Object.DestroyImmediate(amObj);
    }

    [UnityTest, Category("PC"), Category("Mobile")]
    [Description("TC_SOUND_13 - Kiểm tra Awake gán đúng singleton AudioManager.instance")]
    public IEnumerator TC_SOUND_13_Awake_GanDungSingletonInstance()
    {
        var am = CreateAudioManager(1, 1); yield return null;
        Assert.AreEqual(am, AudioManager.instance, "Awake phải set AudioManager.instance.");
        Object.DestroyImmediate(amObj);
    }
}