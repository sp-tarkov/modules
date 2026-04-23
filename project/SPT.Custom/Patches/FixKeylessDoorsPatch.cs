using System.Reflection;
using EFT;
using EFT.Interactive;
using HarmonyLib;
using SPT.Reflection.Patching;

namespace SPT.Custom.Patches;

public class FixKeylessDoorsPatch : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return AccessTools.Method(typeof(GameWorld), nameof(GameWorld.OnGameStarted));
    }

    [PatchPostfix]
    private static void Postfix(GameWorld __instance)
    {
        var doors = LocationScene.GetAllObjects<WorldInteractiveObject>();

        foreach (var door in doors)
        {
            // Fix Military checkpoint key because BSG cant assign a key to doors
            if (door.Id == "door_custom_multiScene_00000")
            {
                door.KeyId = "5913915886f774123603c392";
                break;
            }
        }
    }
}
