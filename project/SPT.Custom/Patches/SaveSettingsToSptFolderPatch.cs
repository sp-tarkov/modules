using SPT.Reflection.Patching;
using HarmonyLib;
using System;
using System.IO;
using System.Reflection;

namespace SPT.Custom.Patches
{
    /// <summary>
    /// Redirect the settings data to save into the SPT folder, not app data
    /// </summary>
    public class SaveSettingsToSptFolderPatch : ModulePatch
    {
        private static readonly string _sptPath = Path.Combine(Environment.CurrentDirectory, "user", "sptSettings");

        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Constructor(typeof(SharedGameSettingsClass));
        }

        [PatchPrefix]
        public static void PatchPrefix(ref string ___string_0, ref string ___string_1)
        {
            if (!Directory.Exists(_sptPath))
            {
                Directory.CreateDirectory(_sptPath);
            }

            ___string_0 = _sptPath;
            ___string_1 = _sptPath;
        }
    }
}
