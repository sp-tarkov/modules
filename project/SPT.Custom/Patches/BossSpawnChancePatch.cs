using SPT.Reflection.Patching;
using SPT.Reflection.Utils;
using System.Linq;
using System.Reflection;

namespace SPT.Custom.Patches
{
    /// <summary>
    /// Boss spawn chance is 100%, all the time, this patch adjusts the chance to the maps boss wave value
    /// </summary>
    public class BossSpawnChancePatch : ModulePatch
    {
        private static float[] _bossSpawnPercent;

        protected override MethodBase GetTargetMethod()
        {
            var desiredType = PatchConstants.LocalGameType;
            var desiredMethod = desiredType
                .GetMethods(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly)
                .SingleOrDefault(IsTargetMethod);

            Logger.LogDebug($"{this.GetType().Name} Type: {desiredType.Name}");
            Logger.LogDebug($"{this.GetType().Name} Method: {desiredMethod?.Name ?? "NOT FOUND"}");

            return desiredMethod;
        }

        private static bool IsTargetMethod(MethodInfo mi)
        {
            var parameters = mi.GetParameters();
            return (parameters.Length == 3
                && parameters[0].Name == "isPVEOffline"
                && parameters[1].Name == "wavesSettings"
                && parameters[2].Name == "bossLocationSpawn");
        }

        [PatchPrefix]
        public static void PatchPrefix(BossLocationSpawn[] bossLocationSpawn)
        {
            _bossSpawnPercent = bossLocationSpawn.Select(s => s.BossChance).ToArray();
        }

        [PatchPostfix]
        public static void PatchPostfix(ref BossLocationSpawn[] __result)
        {
            if (__result.Length != _bossSpawnPercent.Length)
            {
                return;
            }

            for (var i = 0; i < _bossSpawnPercent.Length; i++)
            {
                __result[i].BossChance = _bossSpawnPercent[i];
            }
        }
    }
}
