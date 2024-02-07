using System.Linq;
using System.Reflection;
using Aki.Common.Utils;
using Aki.Core.Utils;
using Aki.Reflection.Patching;
using EFT.Communications;
using EFT.UI;
using UnityEngine;

namespace Aki.Core.Patches
{
    public class GameValidationPatch : ModulePatch
    {
        private const string PluginName = "Aki.Core";
        private const string ErrorMessage = "Escape From Tarkov isn't installed on your computer. " +
                                             "Please buy a copy of the game and support the developers!";
        private static bool _repeat = false;
        private static BepInEx.Logging.ManualLogSource _logger = null;
        
        protected override MethodBase GetTargetMethod()
        {
            return typeof(MenuScreen).GetMethods().First(m => m.Name == nameof(MenuScreen.Show));
        }
        
        [PatchPostfix]
        private static void PatchPostfix()
        {
            _logger = BepInEx.Logging.Logger.CreateLogSource("SslCertificationPatch.PatchPrefix()");
            _logger?.LogInfo("Verifying game installation...");
            
            if (!ValidationUtil.Validate())
            {
                ConsoleScreen.LogError(ErrorMessage);
                ServerLog.Error(PluginName, ErrorMessage);
                _logger?.LogFatal(ErrorMessage);
                
                NotificationManagerClass.DisplayMessageNotification(ErrorMessage, ENotificationDurationType.Infinite, 
                    ENotificationIconType.Alert, Color.red);
                
                CommonUI.Instance.MenuScreen.enabled = false;
                CommonUI.Instance.MenuScreen.Close();
                
                if (!_repeat)
                {
                    _repeat = true;
                }
                else
                {
                    System.Environment.Exit(-1);
                }
            }
            else
            {
                _logger?.LogInfo("Verified game installation.");
                ConsoleScreen.Log("Successfully verified game installation.");
            }
        }
    }
}