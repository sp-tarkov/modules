﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using Aki.Reflection.Patching;
using Aki.Reflection.Utils;
using HarmonyLib;

namespace Aki.Core.Patches
{
    public class TransportPrefixPatch : ModulePatch
    {
        public TransportPrefixPatch()
        {
            try
            {
                var type = PatchConstants.EftTypes.Single(t => t.Name == "Class229");
                var value = Traverse.Create(type).Field("TransportPrefixes").GetValue<Dictionary<ETransportProtocolType, string>>();
                value[ETransportProtocolType.HTTPS] = "http://";
                value[ETransportProtocolType.WSS] = "ws://";
            }
            catch (Exception ex)
            {
                Logger.LogError($"{nameof(TransportPrefixPatch)}: {ex}");
                throw;
            }
        }

        protected override MethodBase GetTargetMethod()
        {
            return PatchConstants.EftTypes.Single(t => t.GetMethods().Any(m => m.Name == "CreateFromLegacyParams"))
                .GetMethod("CreateFromLegacyParams", BindingFlags.Static | BindingFlags.Public);
        }

        [PatchPrefix]
        private static bool PatchPrefix(ref GStruct22 legacyParams)
        {
            //Console.WriteLine($"Original url {legacyParams.Url}");
            legacyParams.Url = legacyParams.Url
                .Replace("https://", "")
                .Replace("http://", "");
            //Console.WriteLine($"Edited url {legacyParams.Url}");
            return true; // do original method after
        }

        [PatchTranspiler]
        private static IEnumerable<CodeInstruction> PatchTranspile(ILGenerator generator, IEnumerable<CodeInstruction> instructions)
        {
            var codes = new List<CodeInstruction>(instructions);

            var searchCode = new CodeInstruction(OpCodes.Ldstr, "https://");
            var searchIndex = -1;

            for (var i = 0; i < codes.Count; i++)
            {
                if (codes[i].opcode == searchCode.opcode && codes[i].operand == searchCode.operand)
                {
                    searchIndex = i;
                    break;
                }
            }

            if (searchIndex == -1)
            {
                Logger.LogError($"{nameof(TransportPrefixPatch)} failed: Could not find reference code.");
                return instructions;
            }

            codes[searchIndex] = new CodeInstruction(OpCodes.Ldstr, "http://");

            return codes.AsEnumerable();
        }
    }
}