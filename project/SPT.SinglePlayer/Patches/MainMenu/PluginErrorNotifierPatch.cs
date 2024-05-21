using SPT.Common.Utils;
using SPT.Reflection.Patching;
using BepInEx.Bootstrap;
using EFT.Communications;
using EFT.UI;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;

namespace SPT.SinglePlayer.Patches.MainMenu
{
    /***
     * On the first show of the main menu, check if any BepInEx plugins have failed to load, and inform
     * the user. This is done via a toast in the bottom right, with a more detailed console message, as
     * well as having the errors forwarded to the server console
     **/
    internal class PluginErrorNotifierPatch : ModulePatch
    {
        private static bool _messageShown = false;

        protected override MethodBase GetTargetMethod()
        {
            // We don't really care which "Show" method is returned - either will do
            return typeof(MenuScreen).GetMethods().First(m => m.Name == nameof(MenuScreen.Show));
        }

        [PatchPostfix]
        private static void PatchPostfix()
        {
            var failedPluginCount = Chainloader.DependencyErrors.Count;

            // Skip if we've already shown the message, or there are no errors
            if (_messageShown || failedPluginCount == 0)
            {
                return;
            }

            // Show a toast in the bottom right of the screen indicating how many plugins failed to load
            var consoleHeaderMessage = $"{failedPluginCount} plugin{(failedPluginCount > 1 ? "s" : "")} failed to load due to errors";
            var toastMessage = $"{consoleHeaderMessage}. Please check the console for details.";
            NotificationManagerClass.DisplayMessageNotification(toastMessage, ENotificationDurationType.Infinite, ENotificationIconType.Alert, Color.red);

            // Build the error message we'll put in the BepInEx and in-game consoles
            var stringBuilder = new StringBuilder();
            stringBuilder.AppendLine($"{consoleHeaderMessage}:");
            foreach (string error in Chainloader.DependencyErrors)
            {
                stringBuilder.AppendLine(error);
            }
            var errorMessage = stringBuilder.ToString();

            // Show an error in the BepInEx console/log file
            Logger.LogError(errorMessage);

            // Show errors in the server console
            ServerLog.Error("SPT.Singleplayer", errorMessage);

            // Show an error in the in-game console, we have to write this in reverse order because the
            // in-game console shows newer messages at the top
            foreach (string line in errorMessage.Split('\n').Reverse())
            {
                if (line.Length > 0)
                {
                    ConsoleScreen.LogError(line);
                }
            }

            _messageShown = true;
        }
    }
}
