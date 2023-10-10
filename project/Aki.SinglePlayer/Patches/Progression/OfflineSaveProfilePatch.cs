using Aki.Common.Http;
using Aki.Reflection.Patching;
using Aki.Reflection.Utils;
using Aki.SinglePlayer.Models.Progression;
using Aki.SinglePlayer.Utils.Progression;
using Comfort.Common;
using EFT;
using HarmonyLib;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Linq;
using System.Reflection;

namespace Aki.SinglePlayer.Patches.Progression
{
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
            var desiredType = PatchConstants.EftTypes.Single(x => x.Name == "TarkovApplication");
            var desiredMethod = Array.Find(desiredType.GetMethods(PatchConstants.PrivateFlags), IsTargetMethod);

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
                Exit = result.Value0.ToString().ToLowerInvariant(),
                Profile = profile,
                Health = Utils.Healing.HealthListener.Instance.CurrentHealth,
                Insurance = Utils.Insurance.InsuredItemManager.Instance.GetTrackedItems(),
				IsPlayerScav = ____raidSettings.IsScav
			};

            RequestHandler.PutJson("/raid/profile/save", request.ToJson(_defaultJsonConverters.AddItem(new NotesJsonConverter()).ToArray()));
        }
    }
}
