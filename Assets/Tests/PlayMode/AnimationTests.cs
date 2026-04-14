using System.Collections;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

/// <summary>
/// Animation Tests - Tất cả test liên quan đến Animator, animation state, animation trigger.
/// Gộp từ: PlayMode, NewAutoTests_PlayMode, RunToLife_Assignment_Tests.
/// </summary>
public class AnimationTests
{
    private GameObject tempEnemyObj;

    [TearDown]
    public void Teardown()
    {
        if (tempEnemyObj != null) Object.DestroyImmediate(tempEnemyObj);
        Time.timeScale = 1f;
    }

    [UnityTest]
    [Category("PC")]
    [Category("Mobile")]
    [Description("Kiểm tra nhân vật có lực bay lên (velocity Y > 0) sau khi gọi JumpButton")]
    public IEnumerator TC_ANIMATION_03_NhanVatNhayLen_KhiBamJumpButton()
    {
        yield return SceneManager.LoadSceneAsync(1);
        Time.timeScale = 1;
        GameManager.instance.UnlockPlayer();

        Player player = GameManager.instance.player;
        Rigidbody2D rb = player.GetComponent<Rigidbody2D>();

        yield return new WaitForSeconds(1f);

        float velocityBeforeJump = rb.linearVelocity.y;
        player.JumpButton();
        yield return new WaitForFixedUpdate();

        Assert.That(rb.linearVelocity.y, Is.GreaterThan(velocityBeforeJump),
            "Nhân vật không có lực bay lên sau khi nhảy!");
        Assert.That(rb.linearVelocity.y, Is.GreaterThan(0),
            "Vận tốc Y phải dương ngay sau khi nhảy!");

        yield return new WaitForSeconds(0.5f);
    }

    [UnityTest]
    [Category("PC")]
    [Category("Mobile")]
    [Description("Kiểm tra Animator parameter 'isSliding' được bật khi Player đang trượt")]
    public IEnumerator TC_ANIMATION_05_AnimatorBatIsSliding_KhiPlayerDangTruot()
    {
        yield return SceneManager.LoadSceneAsync(1);
        yield return new WaitForSeconds(1f);

        Player player = GameManager.instance.player;
        Animator animator = player.GetComponent<Animator>();
        Assert.That(animator, Is.Not.Null, "Player không có Animator component!");

        FieldInfo isSlidingField = typeof(Player).GetField("isSliding",
            BindingFlags.NonPublic | BindingFlags.Instance);
        Assert.That(isSlidingField, Is.Not.Null, "Không tìm thấy field isSliding!");
        isSlidingField.SetValue(player, true);

        yield return null;

        Assert.That(animator.GetBool("isSliding"), Is.True,
            "Animator parameter 'isSliding' phải được bật khi isSliding = true!");
    }

    [Test]
    [Category("PC")]
    [Category("Mobile")]
    [Description("Kiểm tra AnimatatinTrigger của Enemy: phục hồi gravity, bật canMove, tắt justRespawned")]
    public void TC_ANIMATION_09_EnemyAnimationTrigger_PhucHoiTrangThai()
    {
        tempEnemyObj = new GameObject();
        tempEnemyObj.SetActive(false);

        Enemy enemy = tempEnemyObj.AddComponent<Enemy>();
        Rigidbody2D rb = tempEnemyObj.AddComponent<Rigidbody2D>();

        var flags = BindingFlags.NonPublic | BindingFlags.Instance;

        typeof(Enemy).GetField("rb", flags).SetValue(enemy, rb);
        rb.gravityScale = 0f;
        enemy.canMove = false;

        typeof(Enemy).GetField("defaultGravityScale", flags).SetValue(enemy, 5f);
        typeof(Enemy).GetField("justRespawned", flags).SetValue(enemy, true);

        typeof(Enemy).GetMethod("AnimatatinTrigger", flags).Invoke(enemy, null);

        bool justRespawned = (bool)typeof(Enemy).GetField("justRespawned", flags).GetValue(enemy);

        Assert.That(rb.gravityScale, Is.EqualTo(5f),
            "gravityScale phải phục hồi về defaultGravityScale!");
        Assert.That(enemy.canMove, Is.True,
            "canMove phải là true sau AnimationTrigger!");
        Assert.That(justRespawned, Is.False,
            "justRespawned phải reset về false!");
    }

    [UnityTest]
    [Category("PC")]
    [Category("Mobile")]
    [Description("Kiểm tra Animator parameter 'xVelocity' khớp với vận tốc ngang thực tế của Rigidbody2D")]
    public IEnumerator TC_ANIMATION_10_AnimatorXVelocity_KhopVoiRigidbody()
    {
        yield return SceneManager.LoadSceneAsync(1);
        GameManager.instance.UnlockPlayer();

        Player player = GameManager.instance.player;
        Animator anim = player.GetComponent<Animator>();
        Rigidbody2D rb = player.GetComponent<Rigidbody2D>();

        yield return new WaitForSeconds(0.5f);

        float animVelocity = anim.GetFloat("xVelocity");
        float rbVelocity = rb.linearVelocity.x;

        Assert.AreEqual(rbVelocity, animVelocity, 0.01f,
            "Animator xVelocity phải khớp với Rigidbody2D velocity.x.");
    }

    [UnityTest]
    [Category("PC")]
    [Category("Mobile")]
    [Description("Kiểm tra Animator 'isGrounded' = true khi Player đứng yên trên mặt đất")]
    public IEnumerator TC_ANIMATION_11_AnimatorIsGrounded_True_KhiDungTrenDat()
    {
        yield return SceneManager.LoadSceneAsync(1);
        GameManager.instance.UnlockPlayer();

        Player player = GameManager.instance.player;
        Animator anim = player.GetComponent<Animator>();

        yield return new WaitForSeconds(1.0f);

        bool isGrounded = anim.GetBool("isGrounded");
        Assert.IsTrue(isGrounded,
            "Animator isGrounded phải là true khi Player đứng trên mặt đất.");
    }

    [UnityTest]
    [Category("PC")]
    [Category("Mobile")]
    [Description("Kiểm tra Animator 'isGrounded' = false ngay sau khi Player nhảy lên không trung")]
    public IEnumerator TC_ANIMATION_12_AnimatorIsGrounded_False_SauKhiNhay()
    {
        yield return SceneManager.LoadSceneAsync(1);
        GameManager.instance.UnlockPlayer();

        Player player = GameManager.instance.player;
        Animator anim = player.GetComponent<Animator>();

        yield return new WaitForSeconds(0.5f);

        player.JumpButton();
        yield return new WaitForSeconds(0.1f);

        bool isGrounded = anim.GetBool("isGrounded");
        Assert.IsFalse(isGrounded,
            "Animator isGrounded phải là false ngay sau khi Player nhảy lên.");
    }
}
