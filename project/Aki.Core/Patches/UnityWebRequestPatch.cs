using System.Reflection;
using UnityEngine.Networking;
using Aki.Reflection.Patching;
using Aki.Core.Models;

namespace Aki.Core.Patches
{
    public class UnityWebRequestPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return typeof(UnityWebRequestTexture).GetMethod(nameof(UnityWebRequestTexture.GetTexture), new[] { typeof(string) });
        }

        [PatchPostfix]
        private static void PatchPostfix(UnityWebRequest __result)
        {
            __result.certificateHandler = new FakeCertificateHandler();
            __result.disposeCertificateHandlerOnDispose = true;
            __result.timeout = 15000;
        }
    }
}
