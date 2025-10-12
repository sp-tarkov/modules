using System.Reflection;
using EFT;
using HarmonyLib;
using SPT.Reflection.Patching;

namespace SPT.SinglePlayer.Patches.RaidFix;

/**
 * The purpose of this patch is to assign the correct `Side` to PMC bots after their profile has been
 * pulled from the server.
 *
 * This is required, as the data coming back from the server needs to have the Side set to Savage, which
 * breaks certain things like armband slots, and non-lootable melee weapons
 */
public class PmcBotSidePatch : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return AccessTools.Method(typeof(BotCreationDataClass), nameof(BotCreationDataClass.ChooseProfile));
    }

    [PatchPostfix]
    public static void PatchPostfix(ref Profile __result)
    {
        if (__result == null)
        {
            return;
        }

        if (__result.Info.Settings.Role == WildSpawnType.pmcBEAR)
        {
            __result.Info.Side = EPlayerSide.Bear;
        }
        else if (__result.Info.Settings.Role == WildSpawnType.pmcUSEC)
        {
            __result.Info.Side = EPlayerSide.Usec;
        }
    }
}
