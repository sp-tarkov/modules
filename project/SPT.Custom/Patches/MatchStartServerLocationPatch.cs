using EFT;
using HarmonyLib;
using JsonType;
using MonoMod.Cil;
using SPT.Reflection.CodeWrapper;
using SPT.Reflection.Patching;
using SPT.Reflection.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

namespace SPT.Custom.Patches
{
    /**
     * The purpose of this patch is to use the Location data returned from a `/client/match/local/start` call
     * as the authoritative location data during match start, instead of the data cached at session start
     */
    internal class MatchStartServerLocationPatch : ModulePatch
    {
        private static Type nestedType = typeof(TarkovApplication).GetNestedTypes(PatchConstants.PublicDeclaredFlags).SingleCustom(IsTargetNestedType);
        private static Type desiredType = typeof(TarkovApplication).GetNestedTypes(PatchConstants.PublicDeclaredFlags).SingleCustom(IsDesiredType);

        protected override MethodBase GetTargetMethod()
        {
            var desiredMethod = desiredType.GetMethods(PatchConstants.PublicDeclaredFlags)
                .FirstOrDefault(x => x.Name == "MoveNext");

            Logger.LogDebug($"{this.GetType().Name} Type: {desiredType?.Name}");
            Logger.LogDebug($"{this.GetType().Name} Method: {desiredMethod?.Name}");
            Logger.LogDebug($"{this.GetType().Name} Nested Type: {nestedType?.Name}");

            return desiredMethod;
        }

        [PatchTranspiler]
        public static IEnumerable<CodeInstruction> PatchTranspile(ILGenerator generator, IEnumerable<CodeInstruction> instructions)
        {
            var codes = new List<CodeInstruction>(instructions);

            // Search for the code where `result.serverId` gets assigned
            var searchCode = new CodeInstruction(OpCodes.Stfld, AccessTools.Field(typeof(LocalRaidSettings), nameof(LocalRaidSettings.serverId)));
            var searchIndex = -1;
            for (var i = 0; i < codes.Count; i++)
            {
                if (codes[i].opcode == searchCode.opcode && codes[i].operand == searchCode.operand)
                {
                    // Jump ahead one to get past the current assignment
                    searchIndex = i + 1;
                    break;
                }
            }

            // Failed to find the target code
            if (searchIndex == -1)
            {
                Logger.LogError($"Patch {MethodBase.GetCurrentMethod()} failed: Could not find reference code.");
                return instructions;
            }

            // Find the target field (The @class variable)
            var targetField = AccessTools.GetDeclaredFields(desiredType).FirstOrDefault(x => x.FieldType == nestedType);
            if (targetField == null)
            {
                Logger.LogError($"Patch {MethodBase.GetCurrentMethod()} failed: Could not find target field.");
                return instructions;
            }

            // Generate and inject the new IL
            var newCodes = CodeGenerator.GenerateInstructions(new List<Code>()
            {
                /**
                 * Inject the following assignment:
                 *   @class.location = result.locationLoot;
                 */
                new Code(OpCodes.Ldarg_0), // ldarg.0 is `@class`
                new Code(OpCodes.Ldfld, desiredType, targetField.Name),
                new Code(OpCodes.Ldloc_2), // ldloc.2 is `localSettings`
                new Code(OpCodes.Ldfld, typeof(LocalSettings), nameof(LocalSettings.locationLoot)),
                new Code(OpCodes.Stfld, nestedType, "location"),
            });
            codes.InsertRange(searchIndex, newCodes);

            return codes.AsEnumerable();
        }

        private static bool IsDesiredType(Type type)
        {
            return type.GetField("timeAndWeather") != null &&
                   type.GetField("tarkovApplication_0") != null &&
                   type.GetField("gameWorld") != null &&
                   type.Name.Contains("Struct");
        }

        private static bool IsTargetNestedType(Type nestedType)
        {
            return nestedType.GetMethods(PatchConstants.PublicDeclaredFlags).Any() &&
                   nestedType.GetFields().Length == 5 &&
                   nestedType.GetField("savageProfile") != null &&
                   nestedType.GetField("profile") != null;
        }
    }
}
