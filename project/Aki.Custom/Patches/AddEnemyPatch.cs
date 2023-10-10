using Aki.Reflection.Patching;
using EFT;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Aki.Custom.Patches
{
    /// <summary>
    /// If a bot being added has an ID found in list_1, it means its trying to add itself to its enemy list
    /// Dont add bot to enemy list if its in list_1 and skip the rest of the AddEnemy() function
    /// </summary>
    public class AddSelfAsEnemyPatch : ModulePatch
    {
        private static readonly string methodName = "AddEnemy";

        protected override MethodBase GetTargetMethod()
        {
            return typeof(BotZoneGroupsDictionary).GetMethod(methodName);
        }

        [PatchPrefix]
        private static bool PatchPrefix(BotZoneGroupsDictionary __instance, IPlayer person)
        {
            var botOwners = (List<BotOwner>)__instance.GetType().GetField("list_1", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(__instance);
            if (botOwners.Any(x => x.Id == person.Id))
            {
                return false;
            }

            return true;
        }
    }
}
