using System.Reflection;
using System.Security.Cryptography.X509Certificates;
using Aki.Reflection.Patching;
using Aki.Core.Utils;
using HarmonyLib;

namespace Aki.Core.Patches
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
			__result = ValidationUtil.Validate();
			return false; // Skip original
		}
	}
}
