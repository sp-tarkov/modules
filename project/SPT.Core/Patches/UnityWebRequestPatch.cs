using System.Reflection;
using UnityEngine.Networking;
using SPT.Reflection.Patching;
using SPT.Core.Models;

namespace SPT.Core.Patches
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
