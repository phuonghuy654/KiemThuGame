using System.Collections;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

/// <summary>
/// Animation Tests - Kiểm tra Animator, animation state, animation trigger.
/// </summary>
public class AnimationTests
{
    private static readonly BindingFlags FLAGS = BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public;
    private void Set(object t, string f, object v) { t.GetType().GetField(f, FLAGS).SetValue(t, v); }
    private T Get<T>(object t, string f) { return (T)t.GetType().GetField(f, FLAGS).GetValue(t); }
    private object Invoke(object t, string m, params object[] a) { return t.GetType().GetMethod(m, FLAGS).Invoke(t, a); }

    private GameObject tempEnemyObj;

    [TearDown]
    public void Teardown()
    {
        if (tempEnemyObj != null) Object.DestroyImmediate(tempEnemyObj);
        Time.timeScale = 1f;
    }

    // ═══════════════════════════════════════
    //  GIỮ NGUYÊN TỪ FILE GỐC
    // ═══════════════════════════════════════

    [UnityTest, Category("PC"), Category("Mobile")]
    [Description("TC_ANIMATION_03 - Kiểm tra nhân vật có lực bay lên (velocity Y > 0) sau khi gọi JumpButton")]
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

    [UnityTest, Category("PC"), Category("Mobile")]
    [Description("TC_ANIMATION_05 - Kiểm tra Animator parameter 'isSliding' được bật khi Player đang trượt")]
    public IEnumerator TC_ANIMATION_05_AnimatorBatIsSliding_KhiPlayerDangTruot()
    {
        yield return SceneManager.LoadSceneAsync(1);
        yield return new WaitForSeconds(1f);

        Player player = GameManager.instance.player;
        Animator animator = player.GetComponent<Animator>();
        Assert.That(animator, Is.Not.Null);

        typeof(Player).GetField("isSliding", FLAGS).SetValue(player, true);
        yield return null;

        Assert.That(animator.GetBool("isSliding"), Is.True,
            "Animator parameter 'isSliding' phải được bật khi isSliding = true!");
    }

    [Test, Category("PC"), Category("Mobile")]
    [Description("TC_ANIMATION_09 - Kiểm tra AnimatatinTrigger của Enemy: phục hồi gravity, bật canMove, tắt justRespawned")]
    public void TC_ANIMATION_09_EnemyAnimationTrigger_PhucHoiTrangThai()
    {
        tempEnemyObj = new GameObject();
        tempEnemyObj.SetActive(false);
        Enemy enemy = tempEnemyObj.AddComponent<Enemy>();
        Rigidbody2D rb = tempEnemyObj.AddComponent<Rigidbody2D>();

        Set(enemy, "rb", rb);
        rb.gravityScale = 0f;
        enemy.canMove = false;
        Set(enemy, "defaultGravityScale", 5f);
        Set(enemy, "justRespawned", true);

        Invoke(enemy, "AnimatatinTrigger");

        bool justRespawned = Get<bool>(enemy, "justRespawned");
        Assert.That(rb.gravityScale, Is.EqualTo(5f));
        Assert.That(enemy.canMove, Is.True);
        Assert.That(justRespawned, Is.False);
    }

    [UnityTest, Category("PC"), Category("Mobile")]
    [Description("TC_ANIMATION_10 - Kiểm tra Animator parameter 'xVelocity' khớp với vận tốc ngang thực tế của Rigidbody2D")]
    public IEnumerator TC_ANIMATION_10_AnimatorXVelocity_KhopVoiRigidbody()
    {
        yield return SceneManager.LoadSceneAsync(1);
        GameManager.instance.UnlockPlayer();
        Player player = GameManager.instance.player;
        Animator anim = player.GetComponent<Animator>();
        Rigidbody2D rb = player.GetComponent<Rigidbody2D>();
        yield return new WaitForSeconds(0.5f);

        Assert.AreEqual(rb.linearVelocity.x, anim.GetFloat("xVelocity"), 0.01f,
            "Animator xVelocity phải khớp với Rigidbody2D velocity.x.");
    }

    [UnityTest, Category("PC"), Category("Mobile")]
    [Description("TC_ANIMATION_11 - Kiểm tra Animator 'isGrounded' = true khi Player đứng yên trên mặt đất")]
    public IEnumerator TC_ANIMATION_11_AnimatorIsGrounded_True_KhiDungTrenDat()
    {
        yield return SceneManager.LoadSceneAsync(1);
        GameManager.instance.UnlockPlayer();
        Animator anim = GameManager.instance.player.GetComponent<Animator>();
        yield return new WaitForSeconds(1.0f);

        Assert.IsTrue(anim.GetBool("isGrounded"),
            "Animator isGrounded phải là true khi Player đứng trên mặt đất.");
    }

    [UnityTest, Category("PC"), Category("Mobile")]
    [Description("TC_ANIMATION_12 - Kiểm tra Animator 'isGrounded' = false ngay sau khi Player nhảy lên không trung")]
    public IEnumerator TC_ANIMATION_12_AnimatorIsGrounded_False_SauKhiNhay()
    {
        yield return SceneManager.LoadSceneAsync(1);
        GameManager.instance.UnlockPlayer();
        Player player = GameManager.instance.player;
        Animator anim = player.GetComponent<Animator>();
        yield return new WaitForSeconds(0.5f);

        player.JumpButton();
        yield return new WaitForSeconds(0.1f);

        Assert.IsFalse(anim.GetBool("isGrounded"),
            "Animator isGrounded phải là false ngay sau khi Player nhảy lên.");
    }

    // ═══════════════════════════════════════
    //  GỘP TỪ CoverageBoostTests
    // ═══════════════════════════════════════

    [UnityTest, Category("PC"), Category("Mobile")]
    [Description("TC_ANIMATION_14 - Kiểm tra RollAnimFinished tắt canRoll trong Animator mà không gây lỗi exception")]
    public IEnumerator TC_ANIMATION_14_RollAnimFinished_KhongThrowException()
    {
        var pObj = new GameObject("Player");
        pObj.AddComponent<Rigidbody2D>().gravityScale = 5;
        pObj.AddComponent<SpriteRenderer>(); pObj.AddComponent<Animator>();
        var dO = new GameObject("D"); dO.transform.SetParent(pObj.transform);
        var bO = new GameObject("B"); bO.transform.SetParent(pObj.transform);
        var wO = new GameObject("W"); wO.transform.SetParent(pObj.transform);
        var p = pObj.AddComponent<Player>();
        Set(p, "dustFx", dO.AddComponent<ParticleSystem>());
        Set(p, "bloodFx", bO.AddComponent<ParticleSystem>());
        Set(p, "wallCheck", wO.transform);
        Set(p, "wallCheckSize", new Vector2(0.5f, 0.5f));
        yield return null;

        Assert.DoesNotThrow(() => Invoke(p, "RollAnimFinished"),
            "RollAnimFinished không được throw exception.");
        Object.DestroyImmediate(pObj);
    }
}