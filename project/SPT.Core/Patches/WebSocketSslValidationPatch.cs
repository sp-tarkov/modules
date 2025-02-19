﻿using HarmonyLib;
using SPT.Reflection.Patching;
using System.Reflection;

namespace SPT.Core.Patches
{
    public class WebSocketSslValidationPatch : ModulePatch
    {
        /**
         * The purpose of this patch is to force Unity to ignore SSL validation failures.
         * This has to be done by injecting directly into Unity's ValidateCertificate method,
         * as the WebSocket handler does not make any external calls for validation, so we can't
         * just set properties on the ClientWebSocket class
         * 
         * The validity of this patch can be tested by completing a quest, and verifying that the
         * mail from the trader shows up (Both a notification, and the mail) without a client restart
         */
        protected override MethodBase GetTargetMethod()
        {
            var desiredType = AccessTools.TypeByName("Mono.Net.Security.MobileTlsContext");
            var desiredMethod = AccessTools.FirstMethod(desiredType, IsTargetMethod);

            Logger.LogDebug($"{this.GetType().Name} Type: {desiredType?.Name}");
            Logger.LogDebug($"{this.GetType().Name} Method: {desiredMethod?.Name}");

            return desiredMethod;
        }

        private bool IsTargetMethod(MethodInfo method)
        {
            ParameterInfo[] parameters = method.GetParameters();

            return method.Name == "ValidateCertificate" &&
                parameters.Length == 2;
        }

        [PatchPrefix]
        public static bool PatchPrefix(ref bool __result)
        {
            // All certs are valid
            __result = true;

            // Skip original
            return false;
        }
    }
}
