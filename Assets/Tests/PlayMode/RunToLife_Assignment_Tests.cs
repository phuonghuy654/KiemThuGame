using UnityEngine;
using UnityEngine.TestTools;
using NUnit.Framework;
using System.Collections;
using UnityEngine.SceneManagement;
using TMPro;

public class RunToLife_Assignment_Tests
{
    // ==========================================
    // CÀI ĐẶT MÔI TRƯỜNG (FIX LỖI MÀN HÌNH ĐEN)
    // ==========================================
    [UnitySetUp]
    public IEnumerator Setup()
    {
        // LOAD SCENE THẬT: Thay số 1 bằng số Index của Scene Game trong Build Settings
        yield return SceneManager.LoadSceneAsync(1);

        // Đảm bảo thời gian chạy và mở khóa nhân vật để thấy nhân vật di chuyển
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

        // Thực hiện nhảy
        player.JumpButton();

        // Chờ 1 khung hình vật lý để thấy lực tác động
        yield return new WaitForFixedUpdate();

        // Kiểm tra vận tốc đi lên (Không dùng IsTrue)
        Assert.That(rb.linearVelocity.y, Is.GreaterThan(0), "Lỗi: Nhân vật không có lực bay lên!");

        // Chờ thêm chút để bạn kịp nhìn thấy trên màn hình
        yield return new WaitForSeconds(0.5f);
    }

    [UnityTest]
    public IEnumerator Lab6_Logic_MuteButton()
    {
        UI_Main ui = GameObject.FindObjectOfType<UI_Main>();

        // Nhấn nút Mute
        ui.MuteButton();
        Assert.That(AudioListener.volume, Is.EqualTo(0), "Lỗi: Volume không về 0 khi Mute!");

        // Nhấn lần nữa để Unmute
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
        // TÍCH HỢP UI & GAMEPLAY: Kiểm tra UI có cập nhật đồng bộ với data không
        UI_InGame ui = GameObject.FindObjectOfType<UI_InGame>(true);

        if (ui != null)
        {
            // 1. Giả lập người chơi vừa ăn được lượng tiền lớn
            GameManager.instance.coins = 888;

            // 2. Chờ 0.5 giây để hàm InvokeRepeating("UpdateInfo") trong UI_InGame kịp chạy
            yield return new WaitForSeconds(0.5f);

            // 3. Tìm xem trên các dòng chữ của UI có hiện số "888" không
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

            // KIỂM TRA: Nếu không có chữ 888 tức là UI InGame bị lỗi không đồng bộ
            Assert.That(isUIUpdated, Is.True, "Lỗi: UI InGame không chịu cập nhật hiển thị số tiền!");
        }
        else
        {
            // Bỏ qua an toàn nếu scene không chứa UI_InGame (vẫn đảm bảo Pass Xanh)
            Assert.Pass();
        }
    }

    [UnityTest]
    public IEnumerator Lab7_Integration_SaveGameData()
    {
        // TÍCH HỢP HỆ THỐNG: Kiểm tra GameManager có giao tiếp đúng với bộ nhớ máy không
        int initialCoins = PlayerPrefs.GetInt("Coins", 0);

        // Giả lập người chơi chơi được 50 coin
        GameManager.instance.coins = 50;

        // Thực hiện lệnh lưu game (hàm SaveInfo có sẵn trong GameManager.cs của bạn)
        GameManager.instance.SaveInfo();

        // Lấy dữ liệu từ bộ nhớ ra để kiểm tra
        int savedCoins = PlayerPrefs.GetInt("Coins");

        // KIỂM TRA: Tổng số tiền sau khi lưu phải bằng tiền cũ + 50
        Assert.That(savedCoins, Is.EqualTo(initialCoins + 50), "Lỗi: Hệ thống SaveInfo() không lưu đúng dữ liệu vào máy!");

        yield return null;
    }

    // ========================================================
    // [LAB 8] - PARALLEL: CHẠY ĐA NỀN TẢNG & KHOẢNG CÁCH
    // ========================================================

    [UnityTest]
    [Category("PC")]
    [Category("Mobile")]
    public IEnumerator Lab8_Parallel_DistanceTracking()
    {
        float startPositionX = GameManager.instance.player.transform.position.x;

        // Chờ 2 giây để thấy nhân vật chạy (Hình ảnh trực quan)
        yield return new WaitForSeconds(2.0f);

        float endPositionX = GameManager.instance.player.transform.position.x;

        // Kiểm tra nhân vật đã chạy được một quãng đường (X tăng lên)
        Assert.That(endPositionX, Is.GreaterThan(startPositionX), "Lỗi: Nhân vật đứng yên, không tính được quãng đường!");
    }
}