using Aki.Reflection.Patching;
using Aki.Reflection.Utils;
using Aki.Common.Http;
using System.Reflection;
using Aki.Custom.Utils;

namespace Aki.Custom.Patches
{
    public class CoreDifficultyPatch : ModulePatch
	{
        protected override MethodBase GetTargetMethod()
        {
			var methodName = "LoadCoreByString";
			var flags = BindingFlags.Public | BindingFlags.Static;

			return PatchConstants.EftTypes.SingleCustom(x => x.GetMethod(methodName, flags) != null)
				.GetMethod(methodName, flags);
		}

		[PatchPrefix]
		private static bool PatchPrefix(ref string __result)
		{
			// core difficulty is requested before the others
			// so update the others now!
			DifficultyManager.Update();

			// update core difficulty
            __result = RequestHandler.GetJson("/singleplayer/settings/bot/difficulty/core/core");
			return string.IsNullOrWhiteSpace(__result);
        }
    }
}
