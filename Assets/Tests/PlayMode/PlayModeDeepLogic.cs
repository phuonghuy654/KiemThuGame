using System.Collections;
using System.Reflection;
using NUnit.Framework;
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

    [TearDown]
    public void Teardown()
    {
        if (tempCoinObj != null) Object.DestroyImmediate(tempCoinObj);
        if (tempEnemyObj != null) Object.DestroyImmediate(tempEnemyObj);
        if (tempPartPrefab != null) Object.DestroyImmediate(tempPartPrefab);
        if (tempGeneratorObj != null) Object.DestroyImmediate(tempGeneratorObj);
        if (tempPlayerObj != null) Object.DestroyImmediate(tempPlayerObj);
    }

    [UnityTest]
    [Description("TC_REGISTER_23 - Kiem tra diem so tang khi nhan vat thu thap coin")]
    public IEnumerator TC_REGISTER_23_Coin_OnTriggerEnter2D_WithPlayer_CoinDestroyedAndCoinsIncremented()
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

    [UnityTest]
    [Description("TC_REGISTER_51 - Kiem tra he thong mua Check Point trong Shop")]
    public IEnumerator TC_REGISTER_51_Shop_BuyCheckPoint_SelectionPersistedAfterSave()
    {
        yield return SceneManager.LoadSceneAsync(1);
        yield return new WaitForSeconds(0.5f);

        UI_Shop shop = GameObject.FindObjectOfType<UI_Shop>(true);
        Assert.That(shop, Is.Not.Null, "Khong tim thay UI_Shop trong scene!");

        PlayerPrefs.SetInt("Coins", 9999);
        PlayerPrefs.Save();

        MethodInfo buyMethod = typeof(UI_Shop).GetMethod("BuyCheckPoint",
            BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);

        if (buyMethod == null)
        {
            buyMethod = typeof(UI_Shop).GetMethod("BuyItem",
                BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
        }

        Assert.That(buyMethod, Is.Not.Null,
            "Khong tim thay method mua Check Point trong UI_Shop!");

        buyMethod.Invoke(shop, new object[] { 0 });
        yield return null;

        int savedSelection = PlayerPrefs.GetInt("CheckPointSelected", -1);
        Assert.That(savedSelection, Is.Not.EqualTo(-1),
            "PlayerPrefs khong luu lua chon Check Point sau khi mua!");
        Assert.That(savedSelection, Is.GreaterThanOrEqualTo(0),
            $"Gia tri luu Check Point khong hop le: {savedSelection}.");
    }

    [UnityTest]
    [Description("TC_REGISTER_18 - Kiem tra xu ly va cham voi chuong ngai vat (Coin bi Enemy huy)")]
    public IEnumerator TC_REGISTER_18_Coin_OnTriggerEnter2D_Enemy_DestroysCoin()
    {
        tempCoinObj = new GameObject();
        Coin coin = tempCoinObj.AddComponent<Coin>();

        tempEnemyObj = new GameObject();
        tempEnemyObj.SetActive(false);
        tempEnemyObj.AddComponent<Enemy>();
        BoxCollider2D enemyCollider = tempEnemyObj.AddComponent<BoxCollider2D>();

        MethodInfo triggerMethod = typeof(Coin).GetMethod("OnTriggerEnter2D",
            BindingFlags.NonPublic | BindingFlags.Instance);
        triggerMethod.Invoke(coin, new object[] { enemyCollider });

        yield return null;

        Assert.That(tempCoinObj == null, Is.True,
            "Coin khong bi huy khi Enemy cham vao!");
    }

    [Test]
    [Description("TC_REGISTER_19 - Kiem tra GeneratePlatform spawn part moi khi player gan (unit)")]
    public void TC_REGISTER_19_LevelGenerator_GeneratePlatform_SpawnsNewPartWhenPlayerIsNear()
    {
        tempPartPrefab = new GameObject("Part");
        tempPartPrefab.SetActive(false);

        GameObject startPoint = new GameObject("StartPoint");
        GameObject endPoint = new GameObject("EndPoint");
        startPoint.transform.SetParent(tempPartPrefab.transform);
        endPoint.transform.SetParent(tempPartPrefab.transform);
        startPoint.transform.localPosition = new Vector3(-5f, 0f, 0f);
        endPoint.transform.localPosition = new Vector3(5f, 0f, 0f);

        tempGeneratorObj = new GameObject();
        tempGeneratorObj.SetActive(false);
        LevelGenerator generator = tempGeneratorObj.AddComponent<LevelGenerator>();

        tempPlayerObj = new GameObject();
        tempPlayerObj.transform.position = new Vector3(0f, 0f, 0f);

        FieldInfo levelPartField = typeof(LevelGenerator).GetField("levelPart",
            BindingFlags.NonPublic | BindingFlags.Instance);
        levelPartField.SetValue(generator, new Transform[] { tempPartPrefab.transform });

        FieldInfo playerField = typeof(LevelGenerator).GetField("player",
            BindingFlags.NonPublic | BindingFlags.Instance);
        playerField.SetValue(generator, tempPlayerObj.transform);

        FieldInfo distanceToSpawnField = typeof(LevelGenerator).GetField("distanceToSpawn",
            BindingFlags.NonPublic | BindingFlags.Instance);
        distanceToSpawnField.SetValue(generator, 20f);

        FieldInfo nextPartPositionField = typeof(LevelGenerator).GetField("nextPartPosition",
            BindingFlags.NonPublic | BindingFlags.Instance);
        nextPartPositionField.SetValue(generator, new Vector3(10f, 0f, 0f));

        MethodInfo generateMethod = typeof(LevelGenerator).GetMethod("GeneratePlatform",
            BindingFlags.NonPublic | BindingFlags.Instance);
        generateMethod.Invoke(generator, null);

        Assert.That(tempGeneratorObj.transform.childCount, Is.GreaterThan(0),
            "GeneratePlatform() khong spawn phan map moi khi player du gan!");
    }
}