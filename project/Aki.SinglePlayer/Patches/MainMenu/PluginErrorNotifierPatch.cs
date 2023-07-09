using Aki.Reflection.Patching;
using Aki.Reflection.Utils;
using BepInEx.Bootstrap;
using EFT.Communications;
using EFT.UI;
using HarmonyLib;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;

namespace Aki.SinglePlayer.Patches.MainMenu
{
    /***
     * On the first show of the main menu, check if any BepInEx plugins have failed to load, and inform
     * the user. This is done via a toast in the bottom right, with a more detailed console message
     **/
    internal class PluginErrorNotifierPatch : ModulePatch
    {
        private static MethodInfo _displayMessageNotificationMethod;
        private static MethodInfo _directLogMethod;
        private static bool _messageShown = false;

        protected override MethodBase GetTargetMethod()
        {
            _displayMessageNotificationMethod = AccessTools.Method(typeof(NotificationManagerClass), "DisplayMessageNotification");
            _directLogMethod = AccessTools.Method(typeof(ConsoleScreen), "method_5");

            var desiredType = typeof(MenuScreen);
            var desiredMethod = desiredType.GetMethod("Show", PatchConstants.PrivateFlags);
            return desiredMethod;
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
            _displayMessageNotificationMethod.Invoke(null, new object[] { toastMessage, ENotificationDurationType.Infinite, ENotificationIconType.Alert, Color.red });

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

            // Show an error in the in-game console, we have to write this in reverse order because the
            // in-game console shows newer messages at the top
            ConsoleScreen consoleScreen = MonoBehaviourSingleton<PreloaderUI>.Instance.Console;
            foreach (string line in errorMessage.Split('\n').Reverse())
            {
                if (line.Length > 0)
                {
                    // Note: We directly call the internal Log method to work around a bug in 'LogError' that passes an empty string
                    //       as the StackTrace parameter, which results in extra newlines being added to the console logs
                    _directLogMethod.Invoke(consoleScreen, new object[] { line, null, LogType.Error });
                }
            }

            _messageShown = true;
        }
    }
}
