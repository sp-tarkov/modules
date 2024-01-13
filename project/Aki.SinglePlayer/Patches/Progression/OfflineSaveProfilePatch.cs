using Aki.Common.Http;
using Aki.Reflection.Patching;
using Aki.Reflection.Utils;
using Aki.SinglePlayer.Models.Progression;
using Aki.SinglePlayer.Utils.Progression;
using Comfort.Common;
using EFT;
using HarmonyLib;
using Newtonsoft.Json;
using System;
using System.Linq;
using System.Reflection;

namespace Aki.SinglePlayer.Patches.Progression
{
    /// <summary>
    /// After a raid, the client doesn't update the server on what occurred during the raid. To persist loot/quest changes etc we 
    /// make the client send the active profile to a spt-specific endpoint `/raid/profile/save` where we can update the players profile json
    /// </summary>
    public class OfflineSaveProfilePatch : ModulePatch
    {
        private static readonly JsonConverter[] _defaultJsonConverters;

        static OfflineSaveProfilePatch()
        {
            _ = nameof(ClientMetrics.Metrics);

            var converterClass = typeof(AbstractGame).Assembly.GetTypes()
                .First(t => t.GetField("Converters", BindingFlags.Static | BindingFlags.Public) != null);

            _defaultJsonConverters = Traverse.Create(converterClass).Field<JsonConverter[]>("Converters").Value;
        }

        protected override MethodBase GetTargetMethod()
        {
            // method_45 - as of 16432
            // method_43 - as of 18876
            var desiredType = typeof(TarkovApplication);
            var desiredMethod = Array.Find(desiredType.GetMethods(PatchConstants.PublicDeclaredFlags), IsTargetMethod);

            Logger.LogDebug($"{this.GetType().Name} Type: {desiredType?.Name}");
            Logger.LogDebug($"{this.GetType().Name} Method: {desiredMethod?.Name}");

            return desiredMethod;
        }

        private bool IsTargetMethod(MethodInfo arg)
        {
            var parameters = arg.GetParameters();
            return parameters.Length > 4
                && parameters[0]?.Name == "profileId"
                && parameters[1]?.Name == "savageProfile"
                && parameters[2]?.Name == "location"
                && arg.ReturnType == typeof(void);
        }

        [PatchPrefix]
        private static void PatchPrefix(string profileId, RaidSettings ____raidSettings, TarkovApplication __instance, Result<ExitStatus, TimeSpan, ClientMetrics> result)
        {
            // Get scav or pmc profile based on IsScav value
            var profile = (____raidSettings.IsScav)
                ? PatchConstants.BackEndSession.ProfileOfPet
                : PatchConstants.BackEndSession.Profile;

            SaveProfileRequest request = new SaveProfileRequest
            {
                Exit = result.Value0.ToString().ToLowerInvariant(), // Exit player used to leave raid
                Profile = profile, // players scav or pmc profile, depending on type of raid they did
                Health = Utils.Healing.HealthListener.Instance.CurrentHealth, // Specific limb/effect damage data the player has at end of raid
                Insurance = Utils.Insurance.InsuredItemManager.Instance.GetTrackedItems(), // A copy of items insured by player with durability values as of raid end (if item is returned, send it back with correct durability values)
				IsPlayerScav = ____raidSettings.IsScav
			};

            RequestHandler.PutJson("/raid/profile/save", request.ToJson(_defaultJsonConverters.AddItem(new NotesJsonConverter()).ToArray()));
        }
    }
}
