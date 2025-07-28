using System;
using System.IO;
using System.Reflection;
using HarmonyLib;
using SPT.Reflection.Patching;

namespace SPT.Custom.Patches;

/// <summary>
/// Redirect the settings data to save into the SPT folder, not app data
/// </summary>
public class SaveSettingsToSptFolderPatch : ModulePatch
{
    private static readonly string _sptPath = Path.Combine(
        Environment.CurrentDirectory,
        "user",
        "sptSettings"
    );

    protected override MethodBase GetTargetMethod()
    {
        return AccessTools.Constructor(typeof(SharedGameSettingsClass));
    }

    [PatchPrefix]
    public static void PatchPrefix()
    {
        if (!Directory.Exists(_sptPath))
        {
            Directory.CreateDirectory(_sptPath);
        }

        SharedGameSettingsClass.String_0 = _sptPath;
        SharedGameSettingsClass.String_1 = _sptPath;
    }
}