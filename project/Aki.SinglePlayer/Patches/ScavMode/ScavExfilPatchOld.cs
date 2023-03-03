using Aki.Reflection.CodeWrapper;
using Aki.Reflection.Patching;
using Aki.Reflection.Utils;
using EFT;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using UnityEngine;

namespace Aki.SinglePlayer.Patches.ScavMode
{
    public class ScavExfilPatchOld : ModulePatch
    {
        private static Type _profileType;
        private static Type _profileInfoType;
        private static Type _fenceTraderInfoType;

        static ScavExfilPatchOld()
        {
            _profileType = PatchConstants.EftTypes.Single(x => x.GetMethod("AddToCarriedQuestItems") != null);
            _profileInfoType = PatchConstants.EftTypes.Single(x => x.GetMethod("GetExperience") != null);
            _fenceTraderInfoType = PatchConstants.EftTypes.Single(x => x.GetMethod("NewExfiltrationPrice") != null);
        }

        protected override MethodBase GetTargetMethod()
        {
            var desiredType = PatchConstants.LocalGameType.BaseType;
            var desiredMethod = desiredType
                .GetMethods(BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly | BindingFlags.CreateInstance)
                .Single(IsTargetMethod);

            Logger.LogDebug($"{this.GetType().Name} Type: {desiredType?.Name}");
            Logger.LogDebug($"{this.GetType().Name} Method: {desiredMethod?.Name}");

            return desiredMethod;
        }

        private static bool IsTargetMethod(MethodInfo methodInfo)
        {
            return (methodInfo.IsVirtual
                && methodInfo.GetParameters().Length == 0
                && methodInfo.ReturnType == typeof(void)
                && methodInfo.GetMethodBody().LocalVariables.Count > 0);
        }

        [PatchTranspiler]
        private static IEnumerable<CodeInstruction> PatchTranspile(ILGenerator generator, IEnumerable<CodeInstruction> instructions)
        {
            var codes = new List<CodeInstruction>(instructions);
            var searchCode = new CodeInstruction(OpCodes.Callvirt, AccessTools.Method(PatchConstants.ExfilPointManagerType, "EligiblePoints", new System.Type[] { typeof(Profile) }));
            var searchIndex = -1;

            for (var i = 0; i < codes.Count; i++)
            {
                if (codes[i].opcode == searchCode.opcode && codes[i].operand == searchCode.operand)
                {
                    searchIndex = i;
                    break;
                }
            }

            // Patch failed.
            if (searchIndex == -1)
            {
                Logger.LogError(string.Format("Patch {0} failed: Could not find reference code.", MethodBase.GetCurrentMethod()));
                return instructions;
            }

            searchIndex -= 3;

            var brFalseLabel = generator.DefineLabel();
            var brLabel = generator.DefineLabel();
            var newCodes = CodeGenerator.GenerateInstructions(new List<Code>()
            {
                new Code(OpCodes.Ldarg_0),
                new Code(OpCodes.Call, PatchConstants.LocalGameType.BaseType, "get_Profile_0"),
                new Code(OpCodes.Ldfld, typeof(Profile), "Info"),
                new Code(OpCodes.Ldfld, _profileInfoType, "Side"),
                new Code(OpCodes.Ldc_I4_4),
                new Code(OpCodes.Ceq),
                new Code(OpCodes.Brfalse, brFalseLabel),
                new Code(OpCodes.Call, PatchConstants.ExfilPointManagerType, "get_Instance"),
                new Code(OpCodes.Ldarg_0),
                new Code(OpCodes.Ldfld, PatchConstants.LocalGameType.BaseType, "gparam_0"),
                new Code(OpCodes.Box, typeof(PlayerOwner)),
                new Code(OpCodes.Callvirt, typeof(PlayerOwner), "get_Player"),
                new Code(OpCodes.Callvirt, typeof(Player), "get_Position"),
                new Code(OpCodes.Ldarg_0),
                new Code(OpCodes.Call, PatchConstants.LocalGameType.BaseType, "get_Profile_0"),
                new Code(OpCodes.Ldfld, typeof(Profile), "Id"),
                new Code(OpCodes.Ldarg_0),
                new Code(OpCodes.Call, PatchConstants.LocalGameType.BaseType, "get_Profile_0"),
                new Code(OpCodes.Call, _profileType, "get_FenceInfo"),
                new Code(OpCodes.Call, _fenceTraderInfoType, "get_AvailableExitsCount"),
                new Code(OpCodes.Callvirt, PatchConstants.ExfilPointManagerType, "ScavExfiltrationClaim", new System.Type[]{ typeof(Vector3), typeof(string), typeof(int) }),
                new Code(OpCodes.Call, PatchConstants.ExfilPointManagerType, "get_Instance"),
                new Code(OpCodes.Call, PatchConstants.ExfilPointManagerType, "get_Instance"),
                new Code(OpCodes.Ldarg_0),
                new Code(OpCodes.Call, PatchConstants.LocalGameType.BaseType, "get_Profile_0"),
                new Code(OpCodes.Ldfld, typeof(Profile), "Id"),
                new Code(OpCodes.Callvirt, PatchConstants.ExfilPointManagerType, "GetScavExfiltrationMask"),
                new Code(OpCodes.Ldarg_0),
                new Code(OpCodes.Call, PatchConstants.LocalGameType.BaseType, "get_Profile_0"),
                new Code(OpCodes.Ldfld, typeof(Profile), "Id"),
                new Code(OpCodes.Callvirt, PatchConstants.ExfilPointManagerType, "ScavExfiltrationClaim", new System.Type[]{ typeof(int), typeof(string) }),
                new Code(OpCodes.Br, brLabel),
                new CodeWithLabel(OpCodes.Call, brFalseLabel, PatchConstants.ExfilPointManagerType, "get_Instance"),
                new Code(OpCodes.Ldarg_0),
                new Code(OpCodes.Call, PatchConstants.LocalGameType.BaseType, "get_Profile_0"),
                new Code(OpCodes.Callvirt, PatchConstants.ExfilPointManagerType, "EligiblePoints", new System.Type[]{ typeof(Profile) }),
                new CodeWithLabel(OpCodes.Stloc_2, brLabel)
            });

            codes.RemoveRange(searchIndex, 5);
            codes.InsertRange(searchIndex, newCodes);

            return codes.AsEnumerable();
        }
    }
}