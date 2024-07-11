using SPT.Reflection.Patching;
using SPT.Reflection.Utils;
using EFT;
using System;
using System.Linq;
using System.Reflection;

namespace SPT.Custom.Patches
{
    /// <summary>
    /// On pmc heal action, remove all negative effects from limbs e.g. light/heavy bleeds
    /// Solves PMCs spending multiple minutes healing every limb
    /// </summary>
    public class PmcTakesAgesToHealLimbsPatch : ModulePatch
    {
        private static Type _targetType;
        private static readonly string methodName = "FirstAidApplied";

        public PmcTakesAgesToHealLimbsPatch()
        {
            _targetType = PatchConstants.EftTypes.FirstOrDefault(IsTargetType);
        }

        protected override MethodBase GetTargetMethod()
        {
            return _targetType.GetMethod(methodName, BindingFlags.Instance | BindingFlags.Public);
        }

        /// <summary>
        /// BotFirstAidClass
        /// </summary>
        private bool IsTargetType(Type type)
        {
            if (type.GetMethod("GetHpPercent") != null && type.GetMethod("TryApplyToCurrentPart") != null)
            {
                return true;
            }

            return false;
        }

        [PatchPrefix]
        private static bool PatchPrefix(BotOwner ___botOwner_0)
        {
            if (___botOwner_0.IsRole(WildSpawnType.pmcUSEC) || ___botOwner_0.IsRole(WildSpawnType.pmcBEAR))
            {
                var healthController = ___botOwner_0.GetPlayer.ActiveHealthController;

                healthController.RemoveNegativeEffects(EBodyPart.Head);
                healthController.RemoveNegativeEffects(EBodyPart.Chest);
                healthController.RemoveNegativeEffects(EBodyPart.Stomach);
                healthController.RemoveNegativeEffects(EBodyPart.LeftLeg);
                healthController.RemoveNegativeEffects(EBodyPart.RightLeg);
                healthController.RemoveNegativeEffects(EBodyPart.LeftArm);
                healthController.RemoveNegativeEffects(EBodyPart.RightArm);
            }

            return true; // Do original
        }
    }
}
