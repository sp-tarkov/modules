using HarmonyLib;
using SPT.Reflection.Patching;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Security;
using System.Net.WebSockets;
using System.Reflection;
using System.Reflection.Emit;
using System.Security.Cryptography.X509Certificates;
using SPT.Reflection.CodeWrapper;

namespace SPT.Core.Patches
{
    public class WebSocketPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(UriParamsClass), nameof(UriParamsClass.method_0));
        }

        // This is a pass through postfix and behaves a little differently than usual
        // https://harmony.pardeike.net/articles/patching-postfix.html#pass-through-postfixes
        [PatchPostfix]
        private static Uri PatchPostfix(Uri __result)
        {
            return new Uri(__result.ToString().Replace("wss:", "ws:"));
        }
    }
    
    public class WebSocketPatch23 : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(Class1921.Struct725), nameof(Class1921.Struct725.MoveNext));
        }
        
        [PatchTranspiler]
        public static IEnumerable<CodeInstruction> PatchTranspile(ILGenerator generator, IEnumerable<CodeInstruction> instructions)
        {
            var codes = new List<CodeInstruction>(instructions);
    
            Logger.LogInfo($"------------------------------ before ------------------------");
            foreach (var code in codes)
            {
                Logger.LogInfo($"{code.opcode.ToString()} - {code?.operand?.ToString() ?? ""}");
            }
            Logger.LogInfo($"------------------------------ before ------------------------");
            
            // Search for code where "Tls" is loaded.
            var searchCode = new CodeInstruction(OpCodes.Ldstr, "Tls");
            var searchIndex = -1;
            
            for (var i = 0; i < codes.Count; i++)
            {
                if (codes[i].opcode == searchCode.opcode && codes[i].operand == searchCode.operand)
                {
                    searchIndex = i;
                    break;
                }
            }
            
            // Patch Failed
            if (searchIndex == -1)
            {
                Logger.LogError($"Patch {MethodBase.GetCurrentMethod()} failed: Could not find reference code.");
                return instructions;
            }
            
            // Move back by 3. This is the start of IL chain that we're interested in.
            searchIndex -= 3;
            
            
            Logger.LogInfo("----------------------------------------------- found inst -------------------");
            Logger.LogInfo($"{codes[searchIndex].opcode.ToString()} - {codes[searchIndex]?.operand?.ToString() ?? ""}");
            Logger.LogInfo("----------------------------------------------- found inst -------------------");
            
            var newCodes = CodeGenerator.GenerateInstructions(new List<Code>()
            {
                new Code(OpCodes.Ldfld, typeof(Class1921.Struct725), "clientWebSocket_0"),
                new Code(OpCodes.Callvirt, typeof(ClientWebSocket), "get_Options"),
                new Code(OpCodes.Ldnull),
                new Code(OpCodes.Ldftn, typeof(WebSocketPatch23), "ValidateFunc", new []{ typeof(object), typeof(X509Certificate), typeof(X509Chain), typeof(SslPolicyErrors) }),
                new Code(OpCodes.Newobj, typeof(RemoteCertificateValidationCallback), ".ctor", new []{ typeof(object), typeof(IntPtr) }),
                new Code(OpCodes.Callvirt, typeof(ClientWebSocketOptions), "set_RemoteCertificateValidationCallback", new []{ typeof(RemoteCertificateValidationCallback) })
            });
            
            codes.InsertRange(searchIndex, newCodes);
            
            Logger.LogInfo($"------------------------------ After insertion ------------------------");
            foreach (var code in codes)
            {
                Logger.LogInfo($"{code.opcode.ToString()} - {code?.operand?.ToString() ?? ""}");
            }
            Logger.LogInfo($"------------------------------ After insertion ------------------------");
            
            return codes.AsEnumerable();
        }
    
        public static bool ValidateFunc(object _, X509Certificate __, X509Chain ___, SslPolicyErrors ____)
        {
            return true;
        }
    }
}
