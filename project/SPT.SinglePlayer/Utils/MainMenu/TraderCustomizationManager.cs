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
        if (_traderIdToTypeField == null)
        {
            Console.WriteLine("TraderCustomizationManager: Reflection failed. Could not find field: TraderIdToType");

            return;
        }

        try
        {
            var json = RequestHandler.GetJson("/singleplayer/moddedTraders");
            var response = Json.Deserialize<ModdedTraderListResponse>(json);

            if (response?.ModdedTraders == null || response.ModdedTraders.Count == 0)
            {
                return;
            }

            // Get ref to static dict we want to add custom trader to
            var traderIdToTypeDictionary = (Dictionary<MongoID, Profile.ETraderType>) _traderIdToTypeField.GetValue(null);
            foreach (var traderId in response.ModdedTraders)
            {
                // Store modded traders as ragman type as he has the clothing functionality
                traderIdToTypeDictionary[traderId] = Profile.ETraderType.Ragman;
            }
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Error loading modded customization traders", ex);
        }
    }
}
