using NUnit.Framework;
using UnityEngine;
using TMPro;
using System.Reflection;

public class EditModeExtended
{
    [Test]
    public void GameManager_UnlockPlayer_SetsPlayerUnlockedFlagToTrue()
    {
        GameObject gmObj = new GameObject();
        GameManager gm = gmObj.AddComponent<GameManager>();

        GameObject playerObj = new GameObject();
        Player player = playerObj.AddComponent<Player>();
        gm.player = player;

        gm.UnlockPlayer();

        Assert.IsTrue(player.playerUnlocked);

        Object.DestroyImmediate(playerObj);
        Object.DestroyImmediate(gmObj);
    }

    [Test]
    public void GameManager_SaveColor_UpdatesPlayerPrefsCorrectly()
    {
        GameObject gmObj = new GameObject();
        GameManager gm = gmObj.AddComponent<GameManager>();

        PlayerPrefs.SetFloat("ColorR", 0f);
        PlayerPrefs.SetFloat("ColorG", 0f);
        PlayerPrefs.SetFloat("ColorB", 0f);

        gm.SaveColor(0.5f, 0.25f, 0.75f);

        Assert.AreEqual(0.5f, PlayerPrefs.GetFloat("ColorR"));
        Assert.AreEqual(0.25f, PlayerPrefs.GetFloat("ColorG"));
        Assert.AreEqual(0.75f, PlayerPrefs.GetFloat("ColorB"));

        Object.DestroyImmediate(gmObj);
    }

    [Test]
    public void UI_Shop_NotEnoughMoney_ReturnsFalseIfPriceExceedsCoins()
    {
        GameObject shopObj = new GameObject();
        UI_Shop shop = shopObj.AddComponent<UI_Shop>();

        PlayerPrefs.SetInt("Coins", 10);

        MethodInfo enoughMoneyMethod = typeof(UI_Shop).GetMethod("EnoughMoney", BindingFlags.NonPublic | BindingFlags.Instance);
        bool result = (bool)enoughMoneyMethod.Invoke(shop, new object[] { 50 });

        Assert.IsFalse(result);
        Assert.AreEqual(10, PlayerPrefs.GetInt("Coins"));

        Object.DestroyImmediate(shopObj);
    }
}