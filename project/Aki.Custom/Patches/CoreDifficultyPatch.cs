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
			// fetch all bot difficulties to be used in BotDifficultyPatch
			// this is called here since core difficulties are fetched before bot-specific difficulties are
			DifficultyManager.Update();

			// update core difficulty
            __result = RequestHandler.GetJson("/singleplayer/settings/bot/difficulty/core/core");
			return string.IsNullOrWhiteSpace(__result);
        }
    }
}
