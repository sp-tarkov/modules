using System.Reflection;
using SPT.Common.Utils;
using SPT.Core.Utils;
using SPT.Reflection.Patching;
using HarmonyLib;

namespace SPT.Core.Patches
{
    public class GameValidationPatch : ModulePatch
    {
        private const string PluginName = "SPT.Core";
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