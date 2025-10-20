using System.Linq;
using System.Reflection;
using HarmonyLib;
using SPT.Reflection.Patching;
using TMPro;

/// <summary>
/// The LocaleManager has a race condition when attempting to merge the character map that can
/// be triggered by the Dialogue route returning while the locale route is processing. Work around
/// the race condition by re-implementing the TMP asset loading to use a copy of the `GClass2347` 
/// dictionary values instead of a direct reference to it that may be modified in another thread
///
/// Note: This is in SPT.Custom because it references TMPro, which isn't available in SPT.SinglePlayer
/// </summary>

namespace SPT.Custom.Patches;
internal class LocaleManagerRaceConditionFixPatch : ModulePatch
{
    /**

     */
    protected override MethodBase GetTargetMethod()
    {
        return AccessTools.FirstMethod(typeof(LocaleManagerClass), IsTargetMethod);
    }

    private bool IsTargetMethod(MethodInfo method)
    {
        // This is `method_1` in 40087
        ParameterInfo[] parameters = method.GetParameters();
        return parameters.Length == 1
            && parameters[0].ParameterType == typeof(string)
            && parameters[0].Name == "localeType";
    }

    [PatchPrefix]
    public static bool PatchPrefix(LocaleManagerClass __instance, string localeType)
    {
        // Code cleaned up and variables renamed for clarity

        LocaleManagerClass.Class1675 fontAssets = new LocaleManagerClass.Class1675();
        if (__instance.Dictionary_1.TryGetValue(localeType, out fontAssets.mainFallBack))
        {
            foreach (var keyValuePair in __instance.IreadOnlyDictionary_1)
            {
                var (fontAsset, fontAssetList) = keyValuePair;
                LocaleManagerClass.Class1676 fontFallback = new LocaleManagerClass.Class1676();
                fontFallback.class1675_0 = fontAssets;
                fontFallback.fallBackCache = __instance.IreadOnlyDictionary_2[fontAsset];
                fontAssetList.RemoveAll(fontFallback.method_0);
                foreach (var cacheEntry in fontFallback.fallBackCache)
                {
                    if (cacheEntry != fontFallback.class1675_0.mainFallBack)
                    {
                        fontAssetList.Add(cacheEntry);
                    }
                }
            }
        }

        foreach (TMP_FontAsset dynamicFont in __instance.Ienumerable_0)
        {
            if (dynamicFont != fontAssets.mainFallBack)
            {
                dynamicFont.ClearFontAssetData(true);
                dynamicFont.ReadFontAssetDefinition();
            }
            else if (fontAssets.mainFallBack != null && fontAssets.mainFallBack.atlasPopulationMode == AtlasPopulationMode.Dynamic && __instance.Dictionary_4.TryGetValue(localeType, out var fontDictionary))
            {
                // This is the fix, previously this would reference fontDictionary directly, we now use .Values.ToList() to clone it
                foreach (var characters in fontDictionary.Values.ToList())
                {
                    fontAssets.mainFallBack.TryAddCharacters(characters, false);
                }
            }
        }

        // Skip original
        return false;
    }
}
