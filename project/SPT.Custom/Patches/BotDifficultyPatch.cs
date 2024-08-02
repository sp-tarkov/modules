using SPT.Custom.Utils;
using SPT.Reflection.Patching;
using SPT.Reflection.Utils;
using EFT;
using EFT.UI;
using System.Reflection;

namespace SPT.Custom.Patches
{
    public class BotDifficultyPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            var methodName = "LoadDifficultyStringInternal";
			var flags = BindingFlags.Public | BindingFlags.Static;

			return PatchConstants.EftTypes.SingleCustom(x => x.GetMethod(methodName, flags) != null)
                .GetMethod(methodName, flags);
        }

        [PatchPrefix]
        public static bool PatchPrefix(ref string __result, BotDifficulty botDifficulty, WildSpawnType role)
        {
            __result = DifficultyManager.Get(botDifficulty, role);
            var resultIsNullEmpty = string.IsNullOrWhiteSpace(__result);
            if (resultIsNullEmpty)
            {
                ConsoleScreen.LogError($"Unable to get difficulty settings for {role} {botDifficulty}");
            }

            return resultIsNullEmpty; // Server data returned = false = skip original method
        }
    }
}
