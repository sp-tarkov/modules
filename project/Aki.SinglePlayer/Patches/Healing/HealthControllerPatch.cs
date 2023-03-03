using Aki.Reflection.Patching;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Aki.SinglePlayer.Patches.Healing
{
    /// <summary>
    /// HealthController used by post-raid heal screen and health listenen class are different, this patch
    /// ensures effects (fracture/bleeding) on body parts stay in sync
    /// </summary>
    public class HealthControllerPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return typeof(HealthControllerClass).GetMethod("ApplyTreatment", BindingFlags.Public | BindingFlags.Instance);
        }

        [PatchPrefix]
        private static void PatchPrefix(object healthObserver)
        {
            var property = healthObserver.GetType().GetProperty("Effects");
            if (property != null)
            {
                var effects = property.GetValue(healthObserver);
                if (effects != null && effects.GetType().GetGenericTypeDefinition() == typeof(List<>))
                {
                    var parsedEffects = ((IList)effects).Cast<object>().ToList();
                    parsedEffects.ForEach(effect =>
                    {
                        var parsedEffect = effect as IEffect;
                        try
                        {
                            // I tried using reflections to raise the event, I also tried replacing the effect
                            // health controller using reflections but they are different types than the one the actual
                            // player class uses so it cant be replaced.
                            // Unfortunately, the easiest way to deal with this is just manually triggering the HealthListener method
                            Utils.Healing.HealthListener.Instance.OnEffectRemovedEvent(parsedEffect);
                        }
                        catch (Exception ex)
                        {
                            Logger.LogError($"Exception!\n{ex.Message}\n{ex.StackTrace}");
                        }
                    });
                }
            }
            else
            {
                Logger.LogDebug("No effects found to heal on the observer!");
            }
        }
    }
}
