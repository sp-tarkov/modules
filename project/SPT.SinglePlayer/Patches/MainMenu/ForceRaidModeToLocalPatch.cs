using System.Reflection;
using EFT;
using HarmonyLib;
using SPT.Reflection.Patching;

namespace SPT.SinglePlayer.Patches.MainMenu;

/// <summary>
/// This patch ensures that the gamemode is always <see cref="ERaidMode.Local"/> and that IsPveOffline is always true when starting a game<br/>
/// This prevents a bug where the gameworld is instantiated as an online world
/// One outcome of not having this patch is grenades do not explode after being thrown
/// </summary>
public class ForceRaidModeToLocalPatch : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return AccessTools.Method(typeof(TarkovApplication), nameof(TarkovApplication.LoadMapAndData));
    }

    [PatchPrefix]
    public static void Prefix(ref RaidSettings ____raidSettings, bool canEscape)
    {
        ____raidSettings.RaidMode = ERaidMode.Local;
        ____raidSettings.IsPveOffline = true;
    }
}
