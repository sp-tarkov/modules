using SPT.Reflection.CodeWrapper;
using SPT.Reflection.Patching;
using SPT.Reflection.Utils;
using Comfort.Common;
using EFT;
using HarmonyLib;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

namespace SPT.SinglePlayer.Patches.ScavMode
{
    public class ScavProfileLoadPatch : ModulePatch
    {
        /// <summary>
        /// This was changed from an IL Patch,
        /// aim is just to replace loaded profile with the Scav profile when creating a game
        /// </summary>
        /// <returns></returns>
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(LocalGame), nameof(LocalGame.smethod_6));
        }

        [PatchPrefix]
        public static void PatchPrefix(ref Profile profile, LocalRaidSettings raidSettings)
        {
            // check raidsettings to see if its a pmc raid
            if (raidSettings.playerSide == ESideType.Pmc)
            {
                Logger.LogInfo("Side was PMC, returning");
                return;
            }

            // if not get scav profile
            // load that into the profile param
            Logger.LogInfo("Side was Scav, setting profile");
            profile = PatchConstants.BackEndSession.ProfileOfPet;
        }
    }
}
