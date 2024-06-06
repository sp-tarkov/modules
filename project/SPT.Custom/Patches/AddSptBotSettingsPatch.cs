//using System.Collections.Generic;
//using System.Reflection;
//using SPT.PrePatch;
//using SPT.Reflection.Patching;
//using EFT;
//using HarmonyLib;

//namespace SPT.Custom.Patches
//{
//    public class AddSptBotSettingsPatch : ModulePatch
//    {
//        protected override MethodBase GetTargetMethod()
//        {
//            return AccessTools.Method(typeof(BotSettingsRepoClass), nameof(BotSettingsRepoClass.Init));
//        }

//        [PatchPrefix]
//        private static void PatchPrefix(ref Dictionary<WildSpawnType, BotSettingsValuesClass> ___dictionary_0)
//        {
//            if (___dictionary_0.ContainsKey((WildSpawnType)SPTPrePatcher.sptUsecValue))
//            {
//                return;
//            }

//            ___dictionary_0.Add((WildSpawnType)SPTPrePatcher.sptUsecValue, new BotSettingsValuesClass(false, false, false, EPlayerSide.Savage.ToStringNoBox()));
//            ___dictionary_0.Add((WildSpawnType)SPTPrePatcher.sptBearValue, new BotSettingsValuesClass(false, false, false, EPlayerSide.Savage.ToStringNoBox()));
//        }
//    }
//}