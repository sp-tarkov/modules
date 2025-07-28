using System;
using System.Reflection;
using Comfort.Common;
using EFT;
using EFT.InventoryLogic;
using HarmonyLib;
using SPT.Reflection.Patching;

namespace SPT.SinglePlayer.Patches.MainMenu
{
    public class ArmorDamageCounterPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(Player), nameof(Player.ApplyDamageInfo));
        }

        [PatchPostfix]
        public static void PatchPostfix(DamageInfoStruct damageInfo)
        {
            if (damageInfo.Player == null || damageInfo.Player.iPlayer == null || !damageInfo.Player.iPlayer.IsYourPlayer)
            {
                return;
            }

            if (damageInfo.Weapon is Weapon)
            {
                if (!Singleton<ItemFactoryClass>.Instance.ItemTemplates.TryGetValue(damageInfo.SourceId, out var template))
                {
                    return;
                }

                if (template is AmmoTemplate bulletTemplate)
                {
                    float absorbedDamage = (float) Math.Round(bulletTemplate.Damage - damageInfo.Damage);
                    damageInfo.Player.iPlayer.Profile.EftStats.SessionCounters.AddFloat(absorbedDamage, SessionCounterTypesAbstractClass.CauseArmorDamage);
                }
            }
        }
    }
}
