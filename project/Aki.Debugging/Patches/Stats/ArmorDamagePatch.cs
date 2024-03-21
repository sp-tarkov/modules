using Aki.Reflection.Patching;
using EFT;
using EFT.InventoryLogic;
using EFT.UI;
using HarmonyLib;
using Newtonsoft.Json;
using System;
using System.Reflection;

namespace Aki.Debugging.Patches.Stats
{
    public class ArmorDamagePatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(Player), nameof(Player.ApplyDamageInfo));
        }

        [PatchPostfix]
        private static void PatchPostfix(DamageInfo damageInfo)
        {
            if (!damageInfo.Player.iPlayer.IsYourPlayer)
            {
                return;
            }

            if (damageInfo.Weapon is Weapon weapon && weapon.Chambers[0].ContainedItem is BulletClass bullet)
            {
                float newDamage = (float)Math.Round(bullet.Damage - damageInfo.Damage);
                damageInfo.Player.iPlayer.Profile.EftStats.SessionCounters.AddFloat(newDamage, GClass2200.CauseArmorDamage);
            }
        }
    }
}
