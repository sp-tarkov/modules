using HarmonyLib;
using SPT.Reflection.CodeWrapper;
using SPT.Reflection.Patching;
using SPT.Reflection.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Security;
using System.Net.WebSockets;
using System.Reflection;
using System.Reflection.Emit;
using System.Security.Cryptography.X509Certificates;

namespace SPT.Core.Patches
{
    public class WebSocketSslValidationPatch : ModulePatch
    {
        private static Type containingClass = PatchConstants.EftTypes.SingleCustom(IsContainingClass);
        private static Type desiredType = containingClass.GetNestedTypes(PatchConstants.PublicDeclaredFlags).SingleCustom(IsDesiredType);

        protected override MethodBase GetTargetMethod()
        {
            var desiredMethod = AccessTools.Method(desiredType, "MoveNext");

            Logger.LogDebug($"{this.GetType().Name} Containing Class: {containingClass?.Name}");
            Logger.LogDebug($"{this.GetType().Name} Type: {desiredType?.Name}");
            Logger.LogDebug($"{this.GetType().Name} Method: {desiredMethod?.Name}");

            return desiredMethod;
        }

        private static bool IsDesiredType(Type type)
        {
            return type.GetField("clientWebSocket_0", PatchConstants.PrivateFlags) != null &&
                   type.Name.Contains("Struct");
        }

        private static bool IsContainingClass(Type type)
        {
            // Look for any class that has a method that takes a WebSocket as the first parameter
            return type.GetMethods().Any(
                method => method.GetParameters().Length > 0 && method.GetParameters()[0].ParameterType == typeof(WebSocket));
        }

        [PatchTranspiler]
        public static IEnumerable<CodeInstruction> PatchTranspile(ILGenerator generator, IEnumerable<CodeInstruction> instructions)
        {
            var codes = new List<CodeInstruction>(instructions);

            // Search for `clientWebSocket.Options.AddSubProtocol("Tls");`
            var searchCode = new CodeInstruction(OpCodes.Ldstr, "Tls");
            var searchIndex = -1;
            for (var i = 0; i < codes.Count; i++)
            {
                if (codes[i].opcode == searchCode.opcode && codes[i].operand == searchCode.operand)
                {
                    // Jump ahead by 2 to get right after the AddSubProtocol call
                    searchIndex = i + 2;
                    break;
                }
            }

            // Failed to find the target code
            if (searchIndex == -1)
            {
                Logger.LogError($"Patch {MethodBase.GetCurrentMethod()} failed: Could not find reference code.");
                return instructions;
            }

            var newCodes = CodeGenerator.GenerateInstructions(new List<Code>()
            {
                /// Insert code `ConfigureClientWebSocket(clientWebSocket_0)`
                new Code(OpCodes.Ldarg_0),
                new Code(OpCodes.Ldfld, desiredType, "clientWebSocket_0"),
                new Code(OpCodes.Call, typeof(WebSocketSslValidationPatch), nameof(ConfigureClientWebSocket), [typeof(ClientWebSocket)])
            });
            codes.InsertRange(searchIndex, newCodes);

            return codes.AsEnumerable();
        }

        public static bool ValidateFunc(object _, X509Certificate __, X509Chain ___, SslPolicyErrors ____)
        {
            return true;
        }

        public static void ConfigureClientWebSocket(ref ClientWebSocket webSocket)
        {
            webSocket.Options.RemoteCertificateValidationCallback = new RemoteCertificateValidationCallback(ValidateFunc);
        }
    }
}
