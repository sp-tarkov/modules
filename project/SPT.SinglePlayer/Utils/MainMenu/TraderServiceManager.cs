using System.Collections.Generic;
using System.Reflection;
using EFT;
using HarmonyLib;
using Newtonsoft.Json;
using SPT.Common.Http;
using SPT.SinglePlayer.Models.MainMenu;

namespace SPT.SinglePlayer.Utils.MainMenu;

public static class TraderServiceManager
{
    private static Dictionary<string, Profile.ETraderServiceSource> _traderIdToTraderServiceDict = new()
    {
        {
            "5ac3b934156ae10c4430e83c",
            Profile.ETraderServiceSource.Ragman
        },
        {
            "6617beeaa9cfa777ca915b7c",
            Profile.ETraderServiceSource.ArenaManager
        }
    };

    private static FieldInfo _traderIdToTraderServiceField;

    static TraderServiceManager()
    {
        _traderIdToTraderServiceField = AccessTools.Field(typeof(Profile.TraderInfo), "TraderIdToTraderService");
    }

    public static void GetModdedTraderData()
    {
        var req = RequestHandler.GetJson("/singleplayer/moddedTraders");
        var moddedTraders = JsonConvert.DeserializeObject<ModdedTraders>(req);

        AddModdedTradersToClothingServiceDict(moddedTraders);
    }

    private static void AddModdedTradersToClothingServiceDict(ModdedTraders traders)
    {
        foreach (var trader in traders.clothingService)
        {
            _traderIdToTraderServiceDict.Add(trader, Profile.ETraderServiceSource.Ragman);
        }

        _traderIdToTraderServiceField.SetValue(_traderIdToTraderServiceField, _traderIdToTraderServiceDict);
    }
}