using HarmonyLib;
using SPT.Reflection.Patching;
using SPT.Reflection.Utils;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;

namespace SPT.SinglePlayer.Patches.Progression
{
    /// <summary>
    /// Transpiler used to resolve BSG checking quest conditions prior to quest condition handlers being connected.
    /// 
    /// In Tarkov 0.15.5, BSG refactored quest initialization, and added a call to `CheckForStatusChange` during the
    /// init process. This happened prior to the quest condition handlers being setup, so quests would end up failing
    /// even if you didn't meet the fail criteria in some cases. The most obvious case is the "Getting Acquainted" quest
    /// failing on raid start/relog after accepting it
    /// 
    /// Their fix (And the below fix) is to remove the call to `CheckForStatusChange` from the quest loading code
    /// 
    /// This patch is only necessary for SPT targetting client version 0.15.5, the bug is fixed in 0.16
    /// </summary>
    internal class QuestLoadStatusChangePatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            var methodName = "SetQuestStatusData";
            var flags = BindingFlags.Public | BindingFlags.Instance;

            var desiredType = PatchConstants.EftTypes.SingleCustom(x => x.GetMethod(methodName, flags) != null);
            var desiredMethod = AccessTools.FirstMethod(desiredType, IsTargetMethod);

            Logger.LogDebug($"{this.GetType().Name} Type: {desiredType?.Name}");
            Logger.LogDebug($"{this.GetType().Name} Method: {desiredMethod?.Name}");

            return desiredMethod;
        }

        private bool IsTargetMethod(MethodInfo method)
        {
            ParameterInfo[] parameters = method.GetParameters();

            return method.Name.StartsWith("method_")
                && method.ReturnType == typeof(void)
                && parameters.Length == 1
                && parameters[0].ParameterType == typeof(RawQuestClass);
        }

        [PatchTranspiler]
        public static IEnumerable<CodeInstruction> Transpile(IEnumerable<CodeInstruction> instructions)
        {
            var codeList = new List<CodeInstruction>(instructions);
            for (var i = 0; i < codeList.Count; i++)
            {
                // We're looking for a `Callvirt` opcode, so skip anything else
                if (codeList[i].opcode != OpCodes.Callvirt) continue;

                // We want to find a call to `CheckForStatusChange`, so skip anything else
                var stringOperand = codeList[i].operand.ToString();
                if (stringOperand == null || !stringOperand.Contains("CheckForStatusChange")) continue;

                // We've found our call, NOP it out. We need to NOP out the current code, and previous 3 to fully remove it
                Logger.LogDebug($"QuestLoadStatusChangePatch Code: |{codeList[i]?.opcode}| |{codeList[i].operand}|");
                codeList[i].opcode = OpCodes.Nop;
                codeList[i - 1].opcode = OpCodes.Nop;
                codeList[i - 2].opcode = OpCodes.Nop;
                codeList[i - 3].opcode = OpCodes.Nop;

                // There should only be one, so we can break out early
                break;
            }

            return codeList;
        }
    }
}
