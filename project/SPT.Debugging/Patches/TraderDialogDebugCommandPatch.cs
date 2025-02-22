using SPT.Reflection.Patching;
using EFT.UI;
using HarmonyLib;
using System.Reflection;

namespace SPT.Debugging.Patches
{
    /**
     * The purpose of this patch is to enable BSG's trader dialog debug console commands
     * 
     * `debug_show_dialog_screen [Lightkeeper, Btr]`: Show the LightKeeper or BTR dialog screen, without needing to interact with the NPC
     * `debug_close_current_screen`: Close the current dialog screen, in the event something has broken
     */
    internal class TraderDialogDebugCommandPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(ConsoleScreen), nameof(ConsoleScreen.InitConsole));
        }

        [PatchPostfix]
        internal static void PatchPostfix()
        {
            ConsoleScreen.Processor.RegisterCommandGroup<TraderDialogInteractionScreenClass>();
        }
    }
}
