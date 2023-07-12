using Aki.Reflection.Patching;
using EFT;
using EFT.InventoryLogic;
using System;
using System.Reflection;

namespace Aki.SinglePlayer.Patches.Quests
{
    public class DogtagPatch : ModulePatch
    {
        private static BindingFlags _flags;
        private static PropertyInfo _getEquipmentProperty;

        static DogtagPatch()
        {
            _ = nameof(EquipmentClass.GetSlot);
            _ = nameof(DamageInfo.Weapon);

            _flags = BindingFlags.Instance | BindingFlags.NonPublic;
            _getEquipmentProperty = typeof(Player).GetProperty("Equipment", _flags);
        }

        protected override MethodBase GetTargetMethod()
        {
            return typeof(Player).GetMethod("OnBeenKilledByAggressor", _flags);
        }

        [PatchPostfix]
        private static void PatchPostfix(Player __instance, Player aggressor, DamageInfo damageInfo)
        {
            if (__instance.Profile.Info.Side == EPlayerSide.Savage)
            {
                return;
            }

            var equipment = (EquipmentClass)_getEquipmentProperty.GetValue(__instance);
            var dogtagSlot = equipment.GetSlot(EquipmentSlot.Dogtag);
            var dogtagItem = dogtagSlot.ContainedItem;
            
            if (dogtagItem == null)
            {
                Logger.LogError($"DogtagPatch error > DogTag slot item on {__instance.Profile?.Info?.Nickname} is null somehow.");
                return;
            }

            var itemComponent = dogtagItem.GetItemComponent<DogtagComponent>();

            if (itemComponent == null)
            {
                Logger.LogError("DogtagPatch error > DogTagComponent on dog tag slot is null. Something went horrifically wrong!");
                return;
            }

            var victimProfileInfo = __instance.Profile.Info;

            itemComponent.AccountId = __instance.Profile.AccountId;
            itemComponent.ProfileId = __instance.Profile.Id;
            itemComponent.Nickname = victimProfileInfo.Nickname;
            itemComponent.Side = victimProfileInfo.Side;
            itemComponent.KillerName = aggressor.Profile.Info.Nickname;
            itemComponent.Time = DateTime.Now;
            itemComponent.Status = "Killed by ";
            itemComponent.KillerAccountId = aggressor.Profile.AccountId;
            itemComponent.KillerProfileId = aggressor.Profile.Id;
            itemComponent.WeaponName = damageInfo.Weapon.Name;

            if (__instance.Profile.Info.Experience > 0)
            {
                itemComponent.Level = victimProfileInfo.Level;
            }
        }
    }
}
