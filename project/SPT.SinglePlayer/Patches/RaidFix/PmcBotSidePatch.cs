using EFT;
using HarmonyLib;
using SPT.Reflection.Patching;
using SPT.Reflection.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace SPT.SinglePlayer.Patches.RaidFix
{
    /**
     * The purpose of this patch is to assign the correct `Side` to PMC bots after their profile has been
     * pulled from the server.
     * 
     * This is required, as the data coming back from the server needs to have the Side set to Savage, which
     * breaks certain things like armband slots, and non-lootable melee weapons
     */
    internal class PmcBotSidePatch : ModulePatch
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
}
