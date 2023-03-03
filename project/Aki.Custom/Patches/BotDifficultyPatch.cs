using Aki.Reflection.Patching;
using Aki.Reflection.Utils;
using Aki.Common.Http;
using EFT;
using System.Linq;
using System.Reflection;

namespace Aki.Custom.Patches
{
    public class BotDifficultyPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            var methodName = "LoadDifficultyStringInternal";
			var flags = BindingFlags.Public | BindingFlags.Static;

			return PatchConstants.EftTypes.Single(x => x.GetMethod(methodName, flags) != null)
                .GetMethod(methodName, flags);
        }

        [PatchPrefix]
        private static bool PatchPrefix(ref string __result, BotDifficulty botDifficulty, WildSpawnType role)
        {
            __result = RequestHandler.GetJson($"/singleplayer/settings/bot/difficulty/{role}/{botDifficulty}");
            return string.IsNullOrWhiteSpace(__result);
        }
    }
}
