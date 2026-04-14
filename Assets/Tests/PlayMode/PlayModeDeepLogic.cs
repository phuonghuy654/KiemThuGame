using System.Collections;
using System.Reflection;
using System.Text.RegularExpressions;
using NUnit.Framework;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

public class PlayModeDeepLogic
{
    private GameObject tempCoinObj;
    private GameObject tempEnemyObj;
    private GameObject tempPartPrefab;
    private GameObject tempGeneratorObj;
    private GameObject tempPlayerObj;
    private GameObject trapObj;
    private GameObject audioManagerObj;
    private GameObject gameManagerObj;
    private GameObject uiObj;
    private GameObject shopObj;

    [TearDown]
    public void Teardown()
    {
        if (tempCoinObj != null) Object.DestroyImmediate(tempCoinObj);
        if (tempEnemyObj != null) Object.DestroyImmediate(tempEnemyObj);
        if (tempPartPrefab != null) Object.DestroyImmediate(tempPartPrefab);
        if (tempGeneratorObj != null) Object.DestroyImmediate(tempGeneratorObj);
        if (tempPlayerObj != null) Object.DestroyImmediate(tempPlayerObj);
        if (trapObj != null) Object.DestroyImmediate(trapObj);
        if (audioManagerObj != null) Object.DestroyImmediate(audioManagerObj);
        if (gameManagerObj != null) Object.DestroyImmediate(gameManagerObj);
        if (shopObj != null) Object.DestroyImmediate(shopObj);
        if (uiObj != null) Object.DestroyImmediate(uiObj);
        PlayerPrefs.DeleteKey("Coins");
    }

    [UnityTest]
    [Description("TC_LOGIC_9 - Kiem tra diem so tang khi nhan vat thu thap coin")]
    public IEnumerator TC_LOGIC_9_Coin_OnTriggerEnter2D_WithPlayer_CoinDestroyedAndCoinsIncremented()
    {
        yield return SceneManager.LoadSceneAsync(1);
        yield return new WaitForSeconds(0.5f);

        int coinsBeforePickup = GameManager.instance.coins;

        tempCoinObj = new GameObject("Coin");
        Coin coin = tempCoinObj.AddComponent<Coin>();

        tempPlayerObj = new GameObject("Player");
        tempPlayerObj.tag = "Player";
        BoxCollider2D playerCollider = tempPlayerObj.AddComponent<BoxCollider2D>();

        MethodInfo triggerMethod = typeof(Coin).GetMethod("OnTriggerEnter2D",
            BindingFlags.NonPublic | BindingFlags.Instance);
        triggerMethod.Invoke(coin, new object[] { playerCollider });

        yield return null;

        Assert.That(tempCoinObj == null || !tempCoinObj.activeInHierarchy, Is.True,
            "Coin khong bi huy sau khi Player cham vao!");
        Assert.That(GameManager.instance.coins, Is.GreaterThan(coinsBeforePickup),
            $"So coins khong tang sau khi nhat! Truoc: {coinsBeforePickup}, sau: {GameManager.instance.coins}.");
    }


    [Test]
    [Description("TC_LOGIC_5 - Kiem tra GeneratePlatform spawn part moi khi player gan (unit)")]
    public void TC_LOGIC_5_LevelGenerator_GeneratePlatform_SpawnsNewPartWhenPlayerIsNear()
    {
        // -------------------------------------------------------
        // 1. Tạo Part prefab  (PHẢI active để Find() hoạt động)
        // -------------------------------------------------------
        tempPartPrefab = new GameObject("Part");
        // KHÔNG SetActive(false) — Instantiate cần object active
        // để Find("StartPoint") / Find("EndPoint") trả về đúng giá trị.

        GameObject startPoint = new GameObject("StartPoint");
        GameObject endPoint = new GameObject("EndPoint");
        startPoint.transform.SetParent(tempPartPrefab.transform);
        endPoint.transform.SetParent(tempPartPrefab.transform);

        // StartPoint ở x=0, EndPoint ở x=10
        // → sau khi spawn, nextPartPosition.x sẽ ~ 10
        // → Distance từ player(0,0) đến (10,0) = 10 < distanceToSpawn(20)
        //   → vòng while sẽ spawn thêm 1 lần nữa rồi dừng (EndPoint lần 2 ~ 20)
        startPoint.transform.position = new Vector3(0f, 0f, 0f);
        endPoint.transform.position = new Vector3(10f, 0f, 0f);

        // -------------------------------------------------------
        // 2. Tạo LevelGenerator
        // -------------------------------------------------------
        tempGeneratorObj = new GameObject("LevelGenerator");
        LevelGenerator generator = tempGeneratorObj.AddComponent<LevelGenerator>();

        // -------------------------------------------------------
        // 3. Tạo Player
        // -------------------------------------------------------
        tempPlayerObj = new GameObject("Player");
        tempPlayerObj.transform.position = Vector3.zero; // player tại (0,0,0)

        // -------------------------------------------------------
        // 4. Gán các field qua Reflection (đúng tên với LevelGenerator.cs)
        // -------------------------------------------------------
        var bindFlags = BindingFlags.NonPublic | BindingFlags.Instance;

        // levelPart : Transform[]
        FieldInfo levelPartField = typeof(LevelGenerator).GetField("levelPart", bindFlags);
        Assert.IsNotNull(levelPartField, "Khong tim thay field 'levelPart'");
        levelPartField.SetValue(generator, new Transform[] { tempPartPrefab.transform });

        // player : Transform
        FieldInfo playerField = typeof(LevelGenerator).GetField("player", bindFlags);
        Assert.IsNotNull(playerField, "Khong tim thay field 'player'");
        playerField.SetValue(generator, tempPlayerObj.transform);

        // distanceToSpawn = 20  (player cách nextPartPosition 5 → < 20 → spawn)
        FieldInfo distanceToSpawnField = typeof(LevelGenerator).GetField("distanceToSpawn", bindFlags);
        Assert.IsNotNull(distanceToSpawnField, "Khong tim thay field 'distanceToSpawn'");
        distanceToSpawnField.SetValue(generator, 20f);

        // distanceToDelete phải đủ lớn để DeletePlatform() không xóa part vừa spawn
        FieldInfo distanceToDeleteField = typeof(LevelGenerator).GetField("distanceToDelete", bindFlags);
        Assert.IsNotNull(distanceToDeleteField, "Khong tim thay field 'distanceToDelete'");
        distanceToDeleteField.SetValue(generator, 1000f);

        // nextPartPosition = (5,0,0)
        // Distance từ player(0,0) → (5,0) = 5 < distanceToSpawn(20) → điều kiện spawn ĐÚNG
        FieldInfo nextPartPositionField = typeof(LevelGenerator).GetField("nextPartPosition", bindFlags);
        Assert.IsNotNull(nextPartPositionField, "Khong tim thay field 'nextPartPosition'");
        nextPartPositionField.SetValue(generator, new Vector3(5f, 0f, 0f));

        // -------------------------------------------------------
        // 5. Gọi GeneratePlatform() trực tiếp qua Reflection
        // -------------------------------------------------------
        MethodInfo generateMethod = typeof(LevelGenerator).GetMethod("GeneratePlatform", bindFlags);
        Assert.IsNotNull(generateMethod, "Khong tim thay method 'GeneratePlatform'");
        generateMethod.Invoke(generator, null);

        // -------------------------------------------------------
        // 6. Assert: generator phải có ít nhất 1 child (part đã được spawn)
        // -------------------------------------------------------
        Assert.That(
            tempGeneratorObj.transform.childCount,
            Is.GreaterThan(0),
            "GeneratePlatform() khong spawn phan map moi khi player du gan nextPartPosition!"
        );
    }
}