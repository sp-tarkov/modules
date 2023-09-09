using Aki.Common.Http;
using Aki.Reflection.Patching;
using Aki.Reflection.Utils;
using EFT;
using EFT.UI;
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
            var resultIsNullEmpty = string.IsNullOrWhiteSpace(__result);
            if (resultIsNullEmpty)
            {
                ConsoleScreen.LogError($"Unable to get difficulty settings for {role} {botDifficulty}");
            }

            return resultIsNullEmpty; // Server data returned = false = skip original method
        }
    }
}
