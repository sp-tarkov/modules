using System.Reflection;
using SPT.Reflection.Patching;
using SPT.Reflection.Utils;

namespace SPT.SinglePlayer.Patches.RaidFix;

public class RemoveUsedBotProfilePatch : ModulePatch
{
    static RemoveUsedBotProfilePatch()
    {
        _ = nameof(IGetProfileData.ChooseProfile);
    }

    protected override MethodBase GetTargetMethod()
    {
        return typeof(BotProfileClient).BaseType.GetMethods().SingleCustom(m => m.Name == nameof(BotProfileClient.GetNewProfile) && m.IsVirtual);
    }

    /// <summary>
    /// BotsPresets.GetNewProfile()
    [PatchPrefix]
    public static bool PatchPrefix(ref bool withDelete)
    {
        withDelete = true;

        return true; // Do original method
    }
}
