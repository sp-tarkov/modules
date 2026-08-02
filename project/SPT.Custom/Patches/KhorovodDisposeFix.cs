using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using EFT;
using EFT.Interactive;
using EFT.UI;
using HarmonyLib;
using SPT.Reflection.Patching;

namespace SPT.Custom.Patches;

public class KhorovodDisposeFix : ModulePatch
{
    private static readonly FieldInfo triggerField = AccessTools.Field(typeof(EventObject), "_trigger");

    protected override MethodBase GetTargetMethod()
    {
        return AccessTools.Method(typeof(RunddansController), nameof(RunddansController.Dispose));
    }

    [PatchPrefix]
    public static void PatchPrefix(RunddansController __instance)
    {
        var objects = __instance.Objects;

        if (objects != null && objects.Count > 0)
        {
            MonoBehaviourSingleton<GameUI>.Instance.EventStatePanel.Close();

            foreach (var eventObject in objects)
            {
                if (triggerField != null)
                {
                    var trigger = (EventObjectTrigger) triggerField.GetValue(eventObject.Value);

                    trigger.Inside = false;
                }
            }
        }
    }
}
