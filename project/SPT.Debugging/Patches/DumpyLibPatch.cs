using System.Reflection;
using EFT.UI;
using HarmonyLib;
using SPT.Debugging.Scripts;
using SPT.Reflection.Patching;

namespace SPT.Debugging.Patches;

/// <summary>
/// Used to debug dumpLib issues https://dev.sp-tarkov.com/SPT/AssemblyTool/src/branch/master/DumpLib/DumpyTool.cs
/// </summary>
public class DumpyLibPatch : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return AccessTools.Method(typeof(MenuScreen), nameof(MenuScreen.Awake));
    }

    [PatchPostfix]
    public static void PatchPostfix(MenuScreen __instance)
    {
        // attach Monobehaviour so we can interact with UE
        SPTDebuggingPlugin.HookObject.AddComponent<DumpylibScript>();
    }
}