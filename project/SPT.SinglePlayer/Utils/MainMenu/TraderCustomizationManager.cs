using System;
using System.Collections.Generic;
using System.Reflection;
using EFT;
using HarmonyLib;
using SPT.Common.Http;
using SPT.Common.Utils;
using SPT.SinglePlayer.Models.MainMenu;

namespace SPT.SinglePlayer.Utils.MainMenu;

public static class TraderCustomizationManager
{
    private static FieldInfo _traderIdToTypeField;
    static TraderCustomizationManager()
    {
        _traderIdToTypeField = AccessTools.Field(typeof(Profile.TraderInfo), "TraderIdToType");
    }
    public static void AddModdedTraders()
    {
        try
        {
            var json = RequestHandler.GetJson("/singleplayer/moddedTraders");
            var response = Json.Deserialize<ModdedTraderListResponse>(json);

            if (response?.ModdedTraders == null || response.ModdedTraders.Count == 0)
            {
                return;
            }

            foreach (var traderId in response.ModdedTraders)
            {
                var traderType = Profile.ETraderType.Ragman;

                if (_traderIdToTypeField == null)
                {
                    return;
                }

                var traderIdToTypeDictionary = (Dictionary<MongoID, Profile.ETraderType>)
                    _traderIdToTypeField.GetValue(null);

                traderIdToTypeDictionary[traderId] = traderType;

            }
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Error loading modded customization traders: {ex}");
        }
    }
}
