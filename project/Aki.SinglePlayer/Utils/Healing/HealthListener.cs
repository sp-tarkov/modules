using System;
using System.Collections.Generic;
using UnityEngine;
using Aki.Common.Http;
using Aki.Reflection.Utils;
using Aki.SinglePlayer.Models.Healing;
using System.Linq;
using BepInEx.Logging;

namespace Aki.SinglePlayer.Utils.Healing
{
    public class HealthSynchronizer : MonoBehaviour
    {
        public bool IsEnabled = false;
        public bool IsSynchronized = false;
        float _sleepTime = 10f;
        float _timer = 0f;

        public void Update()
        {
            _timer += Time.deltaTime;

            if (_timer <= _sleepTime)
            {
                return;
            }

            _timer -= _sleepTime;

            if (IsEnabled && !IsSynchronized)
            {
                RequestHandler.PostJson("/player/health/sync", HealthListener.Instance.CurrentHealth.ToJson());
                IsSynchronized = true;
            }
        }
    }

    public class Disposable : IDisposable
    {
        private Action _onDispose;

        public Disposable(Action onDispose)
        {
            _onDispose = onDispose ?? throw new ArgumentNullException(nameof(onDispose));
        }

        public void Dispose()
        {
            _onDispose();
        }
    }

    public class HealthListener
    {
        private static HealthListener _instance;
        private IHealthController _healthController;
        private IDisposable _disposable = null;
        private HealthSynchronizer _simpleTimer;
        public PlayerHealth CurrentHealth { get; }

        protected static ManualLogSource Logger { get; private set; }

        public static HealthListener Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new HealthListener();
                }

                return _instance;
            }
        }

        static HealthListener()
        {
            _ = nameof(IEffect.BodyPart);
            _ = nameof(IHealthController.HydrationChangedEvent);
            _ = nameof(DamageInfo.Weapon);
        }

        private HealthListener()
        {
            if (CurrentHealth == null)
            {
                CurrentHealth = new PlayerHealth();
            }

            if (Logger == null)
            {
                Logger = BepInEx.Logging.Logger.CreateLogSource(nameof(HealthListener));
            }

            _simpleTimer = HookObject.AddOrGetComponent<HealthSynchronizer>();
        }

        public void Init(IHealthController healthController, bool inRaid)
        {
            try
            {
                // cleanup
                if (_disposable != null)
                {
                    _disposable.Dispose();
                }

                // init dependencies
                _healthController = healthController;
                _simpleTimer.IsEnabled = !inRaid;
                CurrentHealth.IsAlive = true;

                // init current health
                SetCurrentHealth(_healthController, CurrentHealth.Health, EBodyPart.Head);
                SetCurrentHealth(_healthController, CurrentHealth.Health, EBodyPart.Chest);
                SetCurrentHealth(_healthController, CurrentHealth.Health, EBodyPart.Stomach);
                SetCurrentHealth(_healthController, CurrentHealth.Health, EBodyPart.LeftArm);
                SetCurrentHealth(_healthController, CurrentHealth.Health, EBodyPart.RightArm);
                SetCurrentHealth(_healthController, CurrentHealth.Health, EBodyPart.LeftLeg);
                SetCurrentHealth(_healthController, CurrentHealth.Health, EBodyPart.RightLeg);

                CurrentHealth.Energy = _healthController.Energy.Current;
                CurrentHealth.Hydration = _healthController.Hydration.Current;
                CurrentHealth.Temperature = _healthController.Temperature.Current;

                // subscribe to events
                _healthController.DiedEvent += OnDiedEvent;
                _healthController.HealthChangedEvent += OnHealthChangedEvent;

                _healthController.EffectAddedEvent += OnEffectAddedEvent;
                _healthController.EffectResidualEvent += OnEffectRemovedEvent;
                _healthController.EffectUpdatedEvent += OnEffectUpdatedEvent;

                // _healthController.EffectRemovedEvent += OnEffectRemovedEvent;

                _healthController.HydrationChangedEvent += OnHydrationChangedEvent;
                _healthController.EnergyChangedEvent += OnEnergyChangedEvent;
                _healthController.TemperatureChangedEvent += OnTemperatureChangedEvent;

                // don't forget to unsubscribe
                _disposable = new Disposable(() =>
                {
                    _healthController.DiedEvent -= OnDiedEvent;
                    _healthController.HealthChangedEvent -= OnHealthChangedEvent;

                    _healthController.EffectAddedEvent -= OnEffectAddedEvent;
                    _healthController.EffectResidualEvent -= OnEffectRemovedEvent;
                    _healthController.EffectUpdatedEvent -= OnEffectUpdatedEvent;

                    // _healthController.EffectRemovedEvent -= OnEffectRemovedEvent;

                    _healthController.HydrationChangedEvent -= OnHydrationChangedEvent;
                    _healthController.EnergyChangedEvent -= OnEnergyChangedEvent;
                    _healthController.TemperatureChangedEvent -= OnTemperatureChangedEvent;
                });
            }
            catch (Exception ex)
            {
                Logger.LogError($"Exception thrown!\n{ex.Message}\n{ex.StackTrace}");
            }
        }

        private void SetCurrentHealth(
            IHealthController healthController,
            IReadOnlyDictionary<EBodyPart, BodyPartHealth> bodyParts,
            EBodyPart bodyPart)
        {
            var bodyPartHealth = healthController.GetBodyPartHealth(bodyPart);
            bodyParts[bodyPart].Initialize(bodyPartHealth.Current, bodyPartHealth.Maximum);

            // Set effects
            var effectsOnPart = healthController.GetAllActiveEffects(bodyPart);
            if (effectsOnPart.Any())
            {
                foreach (var effect in effectsOnPart)
                {
                    // Get the enum of the effect to add, returns UNKNOWN for effects we dont want to store
                    EBodyPartEffect bodyPartEffect = GetBodyPartEffectFromIEffect(effect);
                    Logger.LogDebug($"SetCurrentHealth() - Found {effect.GetType().Name} on {effect.BodyPart} with time {effect.TimeLeft}");
                    // Only add effect to body part if it has a value and timeleft isnt infinity (happens in-raid)
                    if (bodyPartEffect != EBodyPartEffect.Unknown
                        && !float.IsInfinity(effect.TimeLeft))
                    {
                        bodyParts[bodyPart].RemoveEffect(bodyPartEffect);
                        Logger.LogDebug($"SetCurrentHealth() - Adding {bodyPartEffect} to {effect.BodyPart} with time {effect.TimeLeft}");
                        bodyParts[bodyPart].AddEffect(bodyPartEffect, effect.TimeLeft);
                    }
                }
            }
        }

        private void OnEffectAddedEvent(IEffect effect)
        {
            try
            {
                if (effect == null)
                {
                    Logger.LogDebug($"OnEffectAddedEvent() - null event");
                    return;
                }

                // Get the enum of the effect to add, returns UNKNOWN for effects we dont want to store
                var bodyPartEffect = GetBodyPartEffectFromIEffect(effect);
                Logger.LogDebug($"OnEffectAddedEvent() - Found {effect.GetType().Name} on {effect.BodyPart} with time {effect.TimeLeft}");
                if (bodyPartEffect != EBodyPartEffect.Unknown)
                {
                    var time = float.IsInfinity(effect.TimeLeft)
                        ? -1
                        : effect.TimeLeft;

                    Logger.LogDebug($"OnEffectAddedEvent() - Adding {bodyPartEffect} to {effect.BodyPart} with time {time}");
                    CurrentHealth.Health[effect.BodyPart].AddEffect(bodyPartEffect, time);
                    _simpleTimer.IsSynchronized = false;
                }
            }
            catch (Exception ex)
            {
                Logger.LogError($"Exception thrown!\n{ex.Message}\n{ex.StackTrace}");
            }
        }

        private void OnEffectUpdatedEvent(IEffect effect)
        {
            try
            {
                if (effect == null)
                {
                    Logger.LogDebug($"OnEffectUpdatedEvent() - null event");
                    return;
                }
                Logger.LogDebug($"OnEffectUpdatedEvent() - Found {effect.GetType().Name} on {effect.BodyPart} with time {effect.TimeLeft}");
                // Get the enum of the effect to add, returns UNKNOWN for effects we dont want to store
                var bodyPartEffect = GetBodyPartEffectFromIEffect(effect);
                if (bodyPartEffect != EBodyPartEffect.Unknown)
                {
                    var time = float.IsInfinity(effect.TimeLeft)
                        ? -1
                        : effect.TimeLeft;

                    Logger.LogDebug($"OnEffectUpdatedEvent() - Adding {bodyPartEffect} to {effect.BodyPart} with time {time}");
                    CurrentHealth.Health[effect.BodyPart].UpdateEffect(bodyPartEffect, time);
                    _simpleTimer.IsSynchronized = false;
                }
            }
            catch (Exception ex)
            {
                Logger.LogError($"Exception thrown!\n{ex.Message}\n{ex.StackTrace}");
            }
        }

        public void OnEffectRemovedEvent(IEffect effect)
        {
            try
            {
                if (effect == null)
                {
                    Logger.LogDebug($"OnEffectRemovedEvent() - null event");
                    return;
                }

                Logger.LogDebug($"OnEffectRemovedEvent() - found {effect.GetType().Name} on {effect.BodyPart} with time {effect.TimeLeft}");
                var bodyPartEffect = GetBodyPartEffectFromIEffect(effect);
                if (bodyPartEffect != EBodyPartEffect.Unknown)
                {
                    Logger.LogDebug($"OnEffectRemovedEvent() - removing {bodyPartEffect} on {effect.BodyPart}");
                    CurrentHealth.Health[effect.BodyPart].RemoveEffect(bodyPartEffect);
                    _simpleTimer.IsSynchronized = false;
                }
            }
            catch (Exception ex)
            {
                Logger.LogError($"Exception thrown!\n{ex.Message}\n{ex.StackTrace}");
            }
        }

        /// <summary>
        /// Map an IEffect object to an EBodyPartEffect value
        /// </summary>
        /// <param name="effect">negative effect object</param>
        /// <returns></returns>
        private EBodyPartEffect GetBodyPartEffectFromIEffect(IEffect effect)
        {
            return Enum.TryParse<EBodyPartEffect>(effect.GetType().Name, out var result)
                ? result
                : EBodyPartEffect.Unknown;
        }

        private void OnHealthChangedEvent(EBodyPart bodyPart, float diff, DamageInfo effect)
        {
            try
            {
                Logger.LogDebug($"OnHealthChangedEvent() - changing {bodyPart} diff {diff} effect {effect}");
                CurrentHealth.Health[bodyPart].ChangeHealth(diff);
                _simpleTimer.IsSynchronized = false;
            }
            catch (Exception ex)
            {
                Logger.LogError($"Exception thrown!\n{ex.Message}\n{ex.StackTrace}");
            }
        }

        private void OnHydrationChangedEvent(float diff)
        {
            CurrentHealth.Hydration += diff;
            _simpleTimer.IsSynchronized = false;
        }

        private void OnEnergyChangedEvent(float diff)
        {
            CurrentHealth.Energy += diff;
            _simpleTimer.IsSynchronized = false;
        }

        private void OnTemperatureChangedEvent(float diff)
        {
            CurrentHealth.Temperature += diff;
            _simpleTimer.IsSynchronized = false;
        }

        private void OnDiedEvent(EFT.EDamageType obj)
        {
            CurrentHealth.IsAlive = false;
        }
    }
}
