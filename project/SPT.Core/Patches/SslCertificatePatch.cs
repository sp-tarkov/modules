using System.Reflection;
using System.Security.Cryptography.X509Certificates;
using SPT.Reflection.Patching;
using HarmonyLib;

namespace SPT.Core.Patches
{
	public class SslCertificatePatch : ModulePatch
	{
		protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(SslCertPatchClass), nameof(SslCertPatchClass.ValidateCertificate), new[] { typeof(X509Certificate) });
		}

		[PatchPrefix]
		private static bool PatchPrefix(ref bool __result)
		{
			__result = true;
			return false; // Skip original
		}
	}
}
