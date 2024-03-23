using Aki.Reflection.Patching;
using Comfort.Common;
using EFT;
using EFT.InventoryLogic;
using HarmonyLib;
using System;
using System.Reflection;

namespace Aki.SinglePlayer.Patches.MainMenu
{
    public class ArmorDamageCounterPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(Player), nameof(Player.ApplyDamageInfo));
        }

        [PatchPostfix]
        private static void PatchPostfix(DamageInfo damageInfo)
        {
            if (damageInfo.Player == null || damageInfo.Player.iPlayer == null || !damageInfo.Player.iPlayer.IsYourPlayer)
            {
                return;
            }

            if (damageInfo.Weapon is Weapon)
            {
                if (!Singleton<ItemFactory>.Instance.ItemTemplates.TryGetValue(damageInfo.SourceId, out var template))
                {
                    return;
                }

                if(template is AmmoTemplate bulletTemplate)
                {
                    float absorbedDamage = (float)Math.Round(bulletTemplate.Damage - damageInfo.Damage);
                    damageInfo.Player.iPlayer.Profile.EftStats.SessionCounters.AddFloat(absorbedDamage, SessionCounterTypesAbstractClass.CauseArmorDamage);
                }
            }
        }
    }
}
