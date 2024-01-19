using Aki.Common.Http;
using Comfort.Common;
using EFT;
using EFT.InventoryLogic;
using HarmonyLib;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using static BackendConfigSettingsClass;
using TraderServiceClass = GClass1789;

namespace Aki.Debugging.BTR.Utils
{
    public static class BTRUtil
    {
        public static readonly string BTRTraderId = Profile.TraderInfo.BTR_TRADER_ID;
        public static readonly string BTRMachineGunWeaponTplId = "657857faeff4c850222dff1b"; // BTR PKTM machine gun
        public static readonly string BTRMachineGunAmmoTplId = "5e023d34e8a400319a28ed44"; // 7.62x54mmR BT

        static BTRUtil()
        {
            // Sanity checks for compile time failure in the event the GClass changes
            _ = nameof(TraderServiceClass.CanAfford);
            _ = nameof(TraderServiceClass.WasPurchasedInThisRaid);
        }

        /// <summary>
        /// Used to create an instance of the item in-raid.
        /// </summary>
        public static Item CreateItem(string tplId)
        {
            var id = Guid.NewGuid().ToString("N").Substring(0, 24);
            return Singleton<ItemFactory>.Instance.CreateItem(id, tplId, null);
        }
    }
}
