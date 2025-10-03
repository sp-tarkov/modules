using System.Reflection;
using HarmonyLib;
using SPT.Reflection.Patching;

namespace SPT.SinglePlayer.Patches.MainMenu;

/// <summary>
/// The purpose of this patch is to prevent the game from loading without the locales
/// Somewhere in BSG's spagetti, the game *sometimes* has the locale string in the dictionary,
/// Even though the locale was not loaded.
/// To workaround this issue, we clear the dictionary before the game usually checks and requests for it.
/// </summary>
public class LocaleFixPatch : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return AccessTools.Method(typeof(GClass2306), nameof(GClass2306.Execute));
    }

    [PatchPrefix]
    public static void PatchPrefix(ISession session)
    {
        var localeManager = LocaleManagerClass.LocaleManagerClass;
        // String_0 is the locale string: "en", "ru"
        var locale = localeManager.String_0;
        if (localeManager.ContainsCulture(locale))
        {
            Logger.LogWarning($"Locale already loaded: {locale}, this should not be possible, clearing dictionary");
        }
        localeManager.Dictionary_3.Clear();
    }
}
