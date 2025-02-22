using SPT.Reflection.Patching;
using EFT.UI;
using HarmonyLib;
using System.Reflection;
using SPT.Debugging.Commands;

namespace SPT.Debugging.Patches
{
    /**
     * The purpose of this patch is to enable variable debug commands in one central location, for easier
     * disabling prior to shipping a release
     * 
     * BSG Trader debug commands:
     * `debug_show_dialog_screen [Lightkeeper, Btr]`: Show the LightKeeper or BTR dialog screen, without needing to interact with the NPC
     * `debug_close_current_screen`: Close the current dialog screen, in the event something has broken
     * `debug_return_to_root_screen`: Return to root screen (Main menu)
     * `debug_recreate_backend`: Reload everything
     * `debug_reload_profile`: Reload player profile
     * 
     * SPT specific debug commands:
     * `debug_extract [Survived, Killed, Runner, MissingInAction, Transit]`: Extract from the raid with the given status
     */
    internal class EnableDebugCommandsPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(ConsoleScreen), nameof(ConsoleScreen.InitConsole));
        }

        [PatchPostfix]
        internal static void PatchPostfix()
        {
            ConsoleScreen.Processor.RegisterCommandGroup<TraderDialogInteractionScreenClass>();
            ConsoleScreen.Processor.RegisterCommandGroup<DebugCommands>();
        }
    }
}
