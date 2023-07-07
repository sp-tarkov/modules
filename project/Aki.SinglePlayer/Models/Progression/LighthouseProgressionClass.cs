using Comfort.Common;
using EFT;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Aki.SinglePlayer.Models.Progression
{
    public class LighthouseProgressionClass : MonoBehaviour
    {
        private bool _isScav;
        private GameWorld _gameWorld;
        private float _timer;
        private bool _addedToEnemy;
        private List<MineDirectionalColliders> _mines;
        private RecodableItemClass _transmitter;
        private List<IAIDetails> _bosses;
        private bool _aggressor;
        private bool _disabledDoor;
        private readonly string _transmitterId = "62e910aaf957f2915e0a5e36";

        public void Start()
        {
            _gameWorld = Singleton<GameWorld>.Instance;
            _bosses = new List<IAIDetails>();
            _mines = GameObject.FindObjectsOfType<MineDirectionalColliders>().ToList();

            if (_gameWorld == null || !string.Equals(_gameWorld.MainPlayer.Location, "lighthouse", System.StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            // if player is a scav, there is no need to continue this method.
            if (_gameWorld.MainPlayer.Side == EPlayerSide.Savage)
            {
                _isScav = true;
                return;
            }

            // Get the players Transmitter.
            _transmitter = (RecodableItemClass) _gameWorld.MainPlayer.Profile.Inventory.AllRealPlayerItems.FirstOrDefault(x => x.TemplateId == _transmitterId);

            if (_transmitter != null)
            {
                GameObject.Find("Attack").SetActive(false);

                // this zone was added in a newer version and the gameObject actually has a \
                GameObject.Find("CloseZone\\").SetActive(false);

                // Give access to the Lightkeepers door.
                _gameWorld.BufferZoneController.SetPlayerAccessStatus(_gameWorld.MainPlayer.ProfileId, true);
            }
        }

        public void Update()
        {
            if (_gameWorld == null || _addedToEnemy || _disabledDoor || _transmitter == null) return;

            _timer += Time.deltaTime;

            if (_timer < 10f)
            {
                return;
            }

            if (_bosses.Count == 0)
            {
                SetupBosses();
            }

            if (_isScav)
            {
                PlayerIsScav();
                return;
            }

            if (_gameWorld?.MainPlayer?.HandsController?.Item?.TemplateId == _transmitterId)
            {
                if (_transmitter?.RecodableComponent?.Status == RadioTransmitterStatus.Green)
                {
                    foreach (var mine in _mines)
                    {
                        if (mine.gameObject.activeSelf)
                        {
                            mine.gameObject.SetActive(false);
                        }
                    }
                }
            }
            else
            {
                foreach (var mine in _mines)
                {
                    if (!mine.gameObject.activeSelf)
                    {
                        mine.gameObject.SetActive(true);
                    }
                }
            }

            if (_aggressor)
            {
                PlayerIsAggressor();
            }
        }

        private void SetupBosses()
        {
            foreach (var aiBot in _gameWorld.AllAlivePlayersList)
            {
                if (!aiBot.IsYourPlayer)
                {
                    // Edge case of bossZryachiy not being hostile to player
                    if (aiBot.AIData.BotOwner.IsRole(WildSpawnType.bossZryachiy) || aiBot.AIData.BotOwner.IsRole(WildSpawnType.followerZryachiy))
                    {
                        // Subscribe to Bosses OnDeath event
                        aiBot.OnPlayerDeadOrUnspawn += player1 =>
                        {
                            if (player1?.KillerId == _gameWorld.MainPlayer.ProfileId)
                            {
                                _aggressor = true;
                            }
                        };

                        _bosses.Add(aiBot);
                    }
                }
            }
        }

        private void PlayerIsScav()
        {
            // If player is a scav, they must be added to the bosses enemy list otherwise they wont kill them
            foreach (var boss in _bosses)
            {
                boss.AIData.BotOwner.BotsGroup.AddEnemy(_gameWorld.MainPlayer);
            }

            _addedToEnemy = true;
        }

        private void PlayerIsAggressor()
        {
            // Disable access to Lightkeepers door for the player
            _gameWorld.BufferZoneController.SetPlayerAccessStatus(_gameWorld.MainPlayer.ProfileId, false);
            _transmitter?.RecodableComponent?.SetStatus(RadioTransmitterStatus.Yellow);
            _transmitter?.RecodableComponent?.SetEncoded(false);
            _disabledDoor = true;
        }
    }
}
