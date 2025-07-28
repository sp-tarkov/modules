using System.Reflection;
using Comfort.Common;
using EFT.UI;
using HarmonyLib;
using SPT.Common.Http;
using SPT.Common.Utils;
using SPT.Custom.Models;
using SPT.Reflection.Patching;
using SPT.Reflection.Utils;

namespace SPT.Custom.Patches
{
    public class VersionLabelPatch : ModulePatch
    {
        private static string _versionLabel;

        protected override MethodBase GetTargetMethod()
        {
            return PatchConstants.EftTypes
                .SingleCustom(x => x.GetField("Taxonomy", BindingFlags.Public | BindingFlags.Instance) != null)
                .GetMethod("Create", BindingFlags.Public | BindingFlags.Static);
        }

        [PatchPostfix]
        public static void PatchPostfix(object __result)
        {
            if (string.IsNullOrEmpty(_versionLabel))
            {
                var json = RequestHandler.GetJson("/singleplayer/settings/version");
                _versionLabel = Json.Deserialize<VersionResponse>(json).Version;
                Logger.LogInfo($"Server version: {_versionLabel}");
            }

            Traverse.Create(Singleton<PreloaderUI>.Instance).Field("_alphaVersionLabel").Property("LocalizationKey").SetValue("{0}");
            Traverse.Create(Singleton<PreloaderUI>.Instance).Field("string_2").SetValue(_versionLabel);
            var major = Traverse.Create(__result).Field("Major");
            var existingValue = major.GetValue();
            major.SetValue($"{existingValue} {_versionLabel}");
        }
    }
}
