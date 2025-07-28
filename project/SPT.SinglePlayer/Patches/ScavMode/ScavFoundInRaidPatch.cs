using System.Reflection;
using EFT;
using HarmonyLib;
using SPT.Reflection.Patching;

namespace SPT.SinglePlayer.Patches.ScavMode;

public class ScavFoundInRaidPatch : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return AccessTools.Method(typeof(GameWorld), nameof(GameWorld.OnGameStarted));
    }

    [PatchPrefix]
    public static void PatchPrefix(GameWorld __instance)
    {
        var player = __instance.MainPlayer;

        if (player == null || player.Profile.Side != EPlayerSide.Savage)
        {
            return;
        }

        player.Profile.SetSpawnedInSession(true);
    }
}