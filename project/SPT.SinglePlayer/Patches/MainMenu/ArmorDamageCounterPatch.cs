using System;
using System.Reflection;
using Comfort.Common;
using EFT;
using EFT.Ballistics;
using EFT.Counters;
using EFT.InventoryLogic;
using EFT.Quests;
using HarmonyLib;
using SPT.Reflection.Patching;

namespace SPT.SinglePlayer.Patches.MainMenu;

public class ArmorDamageCounterPatch : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return AccessTools.Method(typeof(Player), nameof(Player.ApplyDamageInfo));
    }

    [PatchPostfix]
    public static void PatchPostfix(DamageInfo damageInfo)
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

            if (template is AmmoTemplate bulletTemplate)
            {
                float absorbedDamage = (float)Math.Round(bulletTemplate.Damage - damageInfo.Damage);
                damageInfo.Player.iPlayer.Profile.EftStats.SessionCounters.AddFloat(
                    absorbedDamage,
                    PredefinedCounters.CauseArmorDamage
                );
            }
        }
    }
}
