using HarmonyLib;
using SPT.Reflection.Patching;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace SPT.Custom.Patches
{
    /// <summary>
    /// If Scav war is turned on Botsgroup can be null for some reason if null return early to not softlock player.
    /// </summary>
    public class FixBotgroupMarkofTheUnknown : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(BotsGroupMarkOfUnknown), nameof(BotsGroupMarkOfUnknown.Dispose));
        }
        [PatchPrefix]
        public static bool PatchPrefix(BotsGroup ____groups)
        {
            if (____groups == null)
            {
                return false;
            }
            return true;
        }
    }
}
