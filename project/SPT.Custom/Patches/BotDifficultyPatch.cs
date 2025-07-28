using System.Reflection;
using EFT;
using EFT.UI;
using SPT.Custom.Utils;
using SPT.Reflection.Patching;
using SPT.Reflection.Utils;

namespace SPT.Custom.Patches;

public class BotDifficultyPatch : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        const string methodName = "LoadDifficultyStringInternal";
        const BindingFlags flags = BindingFlags.Public | BindingFlags.Static;

        return PatchConstants
            .EftTypes.SingleCustom(x => x.GetMethod(methodName, flags) != null)
            .GetMethod(methodName, flags);
    }

    [PatchPrefix]
    public static bool PatchPrefix(
        ref string __result,
        BotDifficulty botDifficulty,
        WildSpawnType role
    )
    {
        __result = DifficultyManager.Get(botDifficulty, role);
        var resultIsNullEmpty = string.IsNullOrWhiteSpace(__result);
        if (resultIsNullEmpty)
        {
            ConsoleScreen.LogError(
                $"Unable to get difficulty settings for {role} {botDifficulty}"
            );

            return true; // Do original method
        }

        return false; // Skip original
    }
}