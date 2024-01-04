using Aki.Common.Http;
using Aki.Common.Utils;
using Aki.Reflection.Patching;
using Aki.Reflection.Utils;
using Aki.Custom.Models;
using EFT.UI;
using HarmonyLib;
using System.Linq;
using System.Reflection;
using Comfort.Common;

namespace Aki.Custom.Patches
{
    public class VersionLabelPatch : ModulePatch
    {
        private static string _versionLabel;

        protected override MethodBase GetTargetMethod()
        {
            try
            {
                return PatchConstants.EftTypes
                .Single(x => x.GetField("Taxonomy", BindingFlags.Public | BindingFlags.Instance) != null)
                .GetMethod("Create", BindingFlags.Public | BindingFlags.Static);
            }
            catch (System.Exception e)
            {
                Logger.LogInfo($"VersionLabelPatch failed {e.Message} {e.StackTrace} {e.InnerException.StackTrace}");
                throw;
            }
            
        }

        [PatchPostfix]
        internal static void PatchPostfix(object __result)
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
            major.SetValue($"{existingValue} {_versionLabel}" );
        }
    }
}