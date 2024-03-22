using Aki.Reflection.Patching;
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

            if (damageInfo.Weapon is Weapon weapon && weapon.Chambers[0].ContainedItem is BulletClass bullet)
            {
                float newDamage = (float)Math.Round(bullet.Damage - damageInfo.Damage);
                damageInfo.Player.iPlayer.Profile.EftStats.SessionCounters.AddFloat(newDamage, SessionCounterTypesAbstractClass.CauseArmorDamage);
            }
        }
    }
}
