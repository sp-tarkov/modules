using System;
using System.Reflection;
using EFT;
using EFT.InventoryLogic;
using HarmonyLib;
using SPT.Reflection.Patching;
using SPT.Reflection.Utils;

namespace SPT.SinglePlayer.Patches.ScavMode;

/// <summary>
/// The game is not accepting Scav profiles for the PrestigeController,
/// as a workaround we give the PMC profile so we can get in-raid as a scav
/// this may cause issues with the Prestige system if/when implemented
/// </summary>
public class ScavPrestigeFixPatch : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return AccessTools.Constructor(typeof(GClass3698), new Type[] { typeof(Profile), typeof(InventoryController), typeof(GClass3874), typeof(ISession) }, false);
    }

    [PatchPrefix]
    public static void PatchPrefix(GClass3698 __instance, ref Profile profile)
    {
        if (profile.Side == EPlayerSide.Savage)
        {
            profile = PatchConstants.BackEndSession.Profile;
        }
    }
}