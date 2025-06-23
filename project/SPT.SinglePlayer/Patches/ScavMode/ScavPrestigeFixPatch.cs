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
        return AccessTools.Constructor(typeof(GClass3794), [typeof(Profile), typeof(InventoryController), typeof(QuestBookClass), typeof(ISession)
        ], false);
    }

    [PatchPrefix]
    public static void PatchPrefix(ref Profile profile)
    {
        if (profile.Side == EPlayerSide.Savage)
        {
            profile = PatchConstants.BackEndSession.Profile;
        }
    }
}