using System.Reflection;
using Aki.Common.Utils;
using Aki.Core.Utils;
using Aki.Reflection.Patching;
using HarmonyLib;

namespace Aki.Core.Patches
{
    public class GameValidationPatch : ModulePatch
    {
        private const string PluginName = "Aki.Core";
        private const string ErrorMessage = "Validation failed";
        private static BepInEx.Logging.ManualLogSource _logger = null;
        private static bool _hasRun = false;
        
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(BattleeyePatchClass), nameof(BattleeyePatchClass.RunValidation));
        }
        
        [PatchPostfix]
        private static void PatchPostfix()
        {
            if (ValidationUtil.Validate() || _hasRun)
                return;
            
            if (_logger == null)
                _logger = BepInEx.Logging.Logger.CreateLogSource(PluginName);
            
            _hasRun = true;
            ServerLog.Warn($"Warning: {PluginName}", ErrorMessage);
            _logger?.LogWarning(ErrorMessage);
        }
    }
}