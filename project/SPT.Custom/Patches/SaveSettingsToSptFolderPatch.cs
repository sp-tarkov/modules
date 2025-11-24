using System;
using System.IO;
using System.Reflection;
using EFT.Settings;
using HarmonyLib;
using SPT.Reflection.Patching;

namespace SPT.Custom.Patches;

/// <summary>
/// Redirect the settings data to save into the SPT folder, not app data
/// </summary>
public class SaveSettingsToSptFolderPatch : ModulePatch
{
    private static readonly string _sptPath = Path.Combine(Environment.CurrentDirectory, "SPT", "user", "sptSettings");

    protected override MethodBase GetTargetMethod()
    {
        return AccessTools.Constructor(typeof(SettingsManager));
    }

    [PatchPrefix]
    public static void PatchPrefix()
    {
        if (!Directory.Exists(_sptPath))
        {
            Directory.CreateDirectory(_sptPath);
        }

        SettingsManager.String_0 = _sptPath;
        SettingsManager.String_1 = _sptPath;
    }
}
