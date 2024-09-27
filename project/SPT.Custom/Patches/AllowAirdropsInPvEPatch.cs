﻿using System.Reflection;
using SPT.Reflection.Patching;

namespace SPT.Custom.Patches
{
    /// <summary>
    /// Every maps base.json config from BSG come with a "MinPlayersCountToSpawnAirdrop" property value of 6,
    /// this patch sets the associated property to always return 1 regardless of what config says
    /// </summary>
    public class AllowAirdropsInPvEPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return typeof(GClass2333).GetProperty(nameof(GClass2333.Int32_0)).GetGetMethod();
        }

        [PatchPrefix]
        public static bool PatchPrefix(ref int __result)
        {
            __result = 1;

            return false; // Skip original
        }
    }
}