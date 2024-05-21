using EFT;
using EFT.Vehicle;
using HarmonyLib;
using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using UnityEngine;

namespace SPT.Custom.BTR.Utils
{
    public static class BTRReflectionHelper
    {
        private static Type _btrControllerType = typeof(BTRControllerClass);
        private static Type _firearmControllerType = typeof(Player.FirearmController);

        private static MethodInfo _initBtrControllerMethod = AccessTools.GetDeclaredMethods(_btrControllerType).Single(IsInitBtrControllerMethod);
        private static MethodInfo _updateTaxiPriceMethod = AccessTools.GetDeclaredMethods(_btrControllerType).Single(IsUpdateTaxiPriceMethod);

        private static MethodInfo _playWeaponSoundMethod = AccessTools.GetDeclaredMethods(_firearmControllerType).Single(IsPlayWeaponSoundMethod);

        public static Task InitBtrController(this BTRControllerClass controller)
        {
            return (Task)_initBtrControllerMethod.Invoke(controller, null);
        }

        public static void UpdateTaxiPrice(this BTRControllerClass controller, PathDestination destinationPoint, bool isFinal)
        {
            _updateTaxiPriceMethod.Invoke(controller, new object[] { destinationPoint, isFinal });
        }

        public static void PlayWeaponSound(this Player.FirearmController controller, WeaponSoundPlayer weaponSoundPlayer, BulletClass ammo, Vector3 shotPosition, Vector3 shotDirection, bool multiShot)
        {
            _playWeaponSoundMethod.Invoke(controller, new object[] { weaponSoundPlayer, ammo, shotPosition, shotDirection, multiShot });
        }

        // Find `BTRControllerClass.method_1()`
        private static bool IsInitBtrControllerMethod(MethodInfo method)
        {
            return method.ReturnType == typeof(Task)
                && method.GetParameters().Length == 0;
        }

        // Find `BTRControllerClass.method_9(PathDestination currentDestinationPoint, bool lastRoutePoint)`
        private static bool IsUpdateTaxiPriceMethod(MethodInfo method)
        {
            ParameterInfo[] parameters = method.GetParameters();

            return parameters.Length == 2
                && parameters[0].ParameterType == typeof(PathDestination);
        }

        // Find `Player.FirearmController.method_54(WeaponSoundPlayer weaponSoundPlayer, BulletClass ammo, Vector3 shotPosition, Vector3 shotDirection, bool multiShot)`
        private static bool IsPlayWeaponSoundMethod(MethodInfo method)
        {
            ParameterInfo[] parameters = method.GetParameters();

            return parameters.Length == 5
                && parameters[0].ParameterType == typeof(WeaponSoundPlayer)
                && parameters[1].ParameterType == typeof(BulletClass)
                && parameters[2].ParameterType == typeof(Vector3)
                && parameters[3].ParameterType == typeof(Vector3)
                && parameters[4].ParameterType == typeof(bool);
        }
    }
}
