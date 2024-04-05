using Aki.Reflection.Patching;
using EFT;
using HarmonyLib;
using System.Reflection;
using UnityEngine;

namespace Aki.Custom.Patches
{
    /**
     * BSG passes the wrong target location into the TryCall method for BotCallForHelp, it's passing in
     * the bots current target instead of the target of the bot calling for help
     * 
     * This results in both an NRE, and the called bots target location being wrong
     */
    internal class BotCallForHelpCallBotPatch : ModulePatch
    {
        private static FieldInfo _originalPanicTypeField;

        protected override MethodBase GetTargetMethod()
        {
            _originalPanicTypeField = AccessTools.Field(typeof(BotCallForHelp), "_originalPanicType");

            return AccessTools.FirstMethod(typeof(BotCallForHelp), IsTargetMethod);
        }

        protected bool IsTargetMethod(MethodBase method)
        {
            var parameters = method.GetParameters();
            return (parameters.Length == 1
                    && parameters[0].Name == "calledBot");
        }

        [PatchPrefix]
        private static bool PatchPrefix(ref bool __result, BotCallForHelp __instance, BotOwner calledBot, BotOwner ___botOwner_0)
        {
            if (__instance.method_2(calledBot) && ___botOwner_0.Memory.GoalEnemy != null)
            {
                _originalPanicTypeField.SetValue(calledBot.CallForHelp, calledBot.DangerPointsData.PanicType);
                calledBot.DangerPointsData.PanicType = PanicType.none;
                calledBot.Brain.BaseBrain.CalcActionNextFrame();
                // Note: This differs from BSG's implementation in that we pass in botOwner_0's enemy pos instead of calledBot's enemy pos
                calledBot.CalledData.TryCall(new Vector3?(___botOwner_0.Memory.GoalEnemy.Person.Position), ___botOwner_0, true);
                __result = true;
            }
            else
            {
                __result = false;
            }

            // Skip original
            return false;
        }
    }
}
