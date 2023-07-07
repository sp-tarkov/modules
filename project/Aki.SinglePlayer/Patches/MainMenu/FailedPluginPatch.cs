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
     * the user. This is done via a toast in the bottom right, as well as a console message
     **/
    internal class FailedPluginPatch : ModulePatch
    {
        private static MethodInfo _displayMessageNotificationMethod;
        private static bool _messageShown = false;

        protected override MethodBase GetTargetMethod()
        {
            _displayMessageNotificationMethod = AccessTools.Method(typeof(NotificationManagerClass), "DisplayMessageNotification");

            var desiredType = typeof(MenuScreen);
            var desiredMethod = desiredType.GetMethod("Show", PatchConstants.PrivateFlags);
            return desiredMethod;
        }

        [PatchPostfix]
        private static void PatchPostfix()
        {
            // Skip if we've already shown the message, or there are no errors
            if (_messageShown || Chainloader.DependencyErrors.Count == 0)
            {
                return;
            }

            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.AppendLine("Errors occurred during plugin loading");
            stringBuilder.AppendLine("-------------------------------------");
            stringBuilder.AppendLine();
            foreach (string error in Chainloader.DependencyErrors)
            {
                stringBuilder.AppendLine(error);
                stringBuilder.AppendLine();
            }
            string errorMessage = stringBuilder.ToString();

            // Show a toast in the bottom right of the screen
            _displayMessageNotificationMethod.Invoke(null, new object[] { errorMessage, ENotificationDurationType.Infinite, ENotificationIconType.Alert, Color.red });

            // Show an error in the BepInEx console/log file
            Logger.LogError(errorMessage);

            // Show an error in the in-game console, we have to write this in reverse order because of the nature of the console output
            foreach (string line in errorMessage.Split('\n').Reverse())
            {
                if (line.Trim().Length > 0)
                {
                    ConsoleScreen.LogError(line);
                }
            }

            _messageShown = true;
        }
    }
}
