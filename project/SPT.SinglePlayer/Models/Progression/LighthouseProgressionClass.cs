using BepInEx.Logging;
using Comfort.Common;
using EFT;
using SPT.Reflection.Patching;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace SPT.SinglePlayer.Models.Progression
{
    public class LighthouseProgressionClass : MonoBehaviour
    {
        /// <summary>
        /// Flag to disable mines and AI data that instructs Zryachiy to attack you if the main player is authorized to enter Lightkeeper Island
        /// </summary>
        public static bool MainPlayerControlsIslandAccessForEveryone { get; set; } = true;

        /// <summary>
        /// Flag indicating if the Lightkeeper-Island bridge mines and AI data that instructs Zryachiy to attack you have been disabled for everyone
        /// </summary>
        public bool IsIslandOpenForEveryone { get; private set; } = false;

        private static readonly string _transmitterId = "62e910aaf957f2915e0a5e36";
        private static readonly string _lightKeeperTid = "638f541a29ffd1183d187f57";

        private GameWorld _gameWorld;
        private ManualLogSource _logger;
        private List<IPlayer> lightkeeperFriendlyPlayers = new List<IPlayer>();
        private List<Player> playersOnIsland = new List<Player>();

        /// <summary>
        /// PMC's that have been reported to be Lightkeeper-friendly
        /// </summary>
        public IReadOnlyList<IPlayer> LightkeeperFriendlyPlayers => lightkeeperFriendlyPlayers.AsReadOnly();

        /// <summary>
        /// PMC's that have been reported to be on Lightkeeper Island
        /// </summary>
        public IReadOnlyList<Player> LightkeeperFriendlyPlayersOnIsland => playersOnIsland.AsReadOnly();

        public void Start()
        {
            _gameWorld = Singleton<GameWorld>.Instance;

            if (_gameWorld == null || _gameWorld.MainPlayer == null)
            {
                return;
            }

            _logger = BepInEx.Logging.Logger.CreateLogSource(nameof(ModulePatch));

            // Watch for Zryachiy and his followers to spawn
            Singleton<IBotGame>.Instance.BotsController.BotSpawner.OnBotCreated += botCreated;

            // Exit if transmitter does not exist and isnt green
            if (CheckAndAddLightkeeperFriendlyPlayer(_gameWorld.MainPlayer) && MainPlayerControlsIslandAccessForEveryone)
            {
                AllowEveryoneAccessToLightkeeperIsland();
            }
        }

        /// <summary>
        /// Check if the player has been added to the Lightkeeper-friendly PMC list
        /// </summary>
        public bool IsALightkeeperFriendlyPlayer(IPlayer player)
        {
            return player != null && lightkeeperFriendlyPlayers.Contains(player);
        }

        /// <summary>
        /// Check if the player has been added to the list of Lightkeeper-friendly PMC's on Lightkeeper Island
        /// </summary>
        public bool IsLightkeeperFriendlyPlayerOnIsland(IPlayer player)
        {
            return player != null && playersOnIsland.Contains(player);
        }

        /// <summary>
        /// Checks if the player has an active transmitter in its inventory, and if so add it to the Lightkeeper-friendly PMC list
        /// </summary>
        /// <returns>True if the player was added to the Lightkeeper-friendly PMC list</returns>
        public bool CheckAndAddLightkeeperFriendlyPlayer(IPlayer player)
        {
            if (PlayerHasActiveTransmitterInInventory(player))
            {
                return AddLightkeeperFriendlyPlayer(player);
            }

            return false;
        }

        /// <summary>
        /// Add the player to the Lightkeeper-friendly PMC list
        /// </summary>
        /// <returns>True if the player was added to the Lightkeeper-friendly PMC list</returns>
        public bool AddLightkeeperFriendlyPlayer(IPlayer player)
        {
            if (player == null)
            {
                return false;
            }

            if (lightkeeperFriendlyPlayers.Contains(player))
            {
                _logger.LogWarning($"{player.Profile.Nickname} is already a registered Lightkeeper-friendly player");
                return false;
            }

            lightkeeperFriendlyPlayers.Add(player);

            // Give access to Lightkeepers door
            _gameWorld.BufferZoneController.SetPlayerAccessStatus(player.ProfileId, true);

            return true;
        }

        /// <summary>
        /// Remove the player from the Lightkeeper-friendly PMC list
        /// </summary>
        /// <returns>True if the player was removed from the Lightkeeper-friendly PMC list</returns>
        public bool RemoveLightkeeperFriendlyPlayer(IPlayer player)
        {
            if (player == null)
            {
                return false;
            }

            if (!lightkeeperFriendlyPlayers.Contains(player))
            {
                _logger.LogWarning($"{player.Profile.Nickname} is not a registered Lightkeeper-friendly player");
                return false;
            }

            lightkeeperFriendlyPlayers.Remove(player);

            // Revoke access to Lightkeepers door
            _gameWorld.BufferZoneController.SetPlayerAccessStatus(player.ProfileId, false);

            return true;
        }

        /// <summary>
        /// Add the player to the list of PMC's that are on Lightkeeper Island
        /// </summary>
        public void LightkeeperFriendlyPlayerEnteredIsland(Player player)
        {
            if (playersOnIsland.Contains(player))
            {
                _logger.LogWarning($"{player.name} is already a registered player on Lightkeeper Island");
                return;
            }

            playersOnIsland.Add(player);
            player.OnPlayerDead += OnLightkeeperFriendlyPlayerDead;
        }

        /// <summary>
        /// Remove the player from the list of PMC's that are on Lightkeeper Island
        /// </summary>
        public void LightkeeperFriendlyPlayerLeftIsland(Player player)
        {
            if (!playersOnIsland.Contains(player))
            {
                _logger.LogWarning($"{player.name} is not a registered player on Lightkeeper Island");
                return;
            }

            playersOnIsland.Remove(player);
            player.OnPlayerDead -= OnLightkeeperFriendlyPlayerDead;
        }

        /// <summary>
        /// Disables brige mines, disables AI data to instruct Zryachiy to attack you, and watch for Zryachiy and his followers to spawn
        /// </summary>
        public void AllowEveryoneAccessToLightkeeperIsland()
        {
            if (IsIslandOpenForEveryone)
            {
                return;
            }

            DisableAIPlaceInfoForZryachiy();

            // Set mines to be non-active
            SetBridgeMinesStatus(false);

            IsIslandOpenForEveryone = true;
        }

        /// <summary>
        /// Disable the "Attack" and "CloseZone" AIPlaceInfo objects that instruct Zryachiy and his followers to attack you
        /// </summary>
        public void DisableAIPlaceInfoForZryachiy()
        {
            var places = Singleton<IBotGame>.Instance.BotsController.CoversData.AIPlaceInfoHolder.Places;

            places.First(x => x.name == "Attack").gameObject.SetActive(false);

            // Zone was added in a newer version and the gameObject actually has a \
            places.First(y => y.name == "CloseZone\\").gameObject.SetActive(false);
        }

        /// <summary>
        /// Gets transmitter from players inventory
        /// </summary>
        public RecodableItemClass GetTransmitterFromInventory(IPlayer player)
        {
            if (player == null)
            {
                return null;
            }

            return (RecodableItemClass)player.Profile.Inventory.AllRealPlayerItems.FirstOrDefault(x => x.TemplateId == _transmitterId);
        }

        /// <summary>
        /// Checks for transmitter status and exists in players inventory
        /// </summary>
        public bool PlayerHasActiveTransmitterInInventory(IPlayer player)
        {
            RecodableItemClass transmitter = GetTransmitterFromInventory(player);
            return IsTransmitterActive(transmitter);
        }

        /// <summary>
        /// Check if the transmitter allows access to the island
        /// </summary>
        public bool IsTransmitterActive(RecodableItemClass transmitter)
        {
            return transmitter != null && transmitter?.RecodableComponent?.Status == RadioTransmitterStatus.Green;
        }

        /// <summary>
        /// Set all brdige mines to desire state
        /// </summary>
        /// <param name="desiredMineState">What state should bridge mines be set to</param>
        public void SetBridgeMinesStatus(bool desiredMineState)
        {
            // Find mines with opposite state of what we want
            var mines = _gameWorld.MineManager.Mines
                .Where(mine => IsLighthouseBridgeMine(mine) && mine.gameObject.activeSelf == !desiredMineState);
            
            foreach (var mine in mines)
            {
				mine.gameObject.SetActive(desiredMineState);
            }
		}

        /// <summary>
        /// Check if the mine is on the Lightkeeper Island bridge
        /// </summary>
        /// <returns>True if the mine is on the Lightkeeper Island bridge</returns>
        public static bool IsLighthouseBridgeMine(MineDirectional mine)
        {
            if (mine == null)
            {
                return false;
            }

            return mine.transform.parent.gameObject.name == "Directional_mines_LHZONE";
        }

        /// <summary>
        /// Set aggression + standing loss when Zryachiy/follower or a Lightkeeper-friendly PMC is killed by the main player
        /// </summary>
        /// <param name="player">The player that was killed</param>
        public void OnLightkeeperFriendlyPlayerDead(Player player, IPlayer lastAggressor, DamageInfo damageInfo, EBodyPart part)
        {
            foreach (Player lightkeeperFriendlyPlayer in lightkeeperFriendlyPlayers)
            {
                // Check if a Lightkeeper-friendly player was the killer
                if ((lightkeeperFriendlyPlayer == null) || (lightkeeperFriendlyPlayer.ProfileId != player?.KillerId))
                {
                    continue;
                }

                // A Lightkeeper-friendly player killed Zryachiy or one of his followers
                if (isZryachiyOrFollower(player))
                {
                    playerKilledLightkeeperFriendlyPlayer(lastAggressor);
                    break;
                }

                // A Lightkeeper-friendly player killed another Lightkeeper-friendly player when they were both on the island
                if (playersOnIsland.Any(x => x?.Id == player?.Id) && playersOnIsland.Any(x => x?.Id == lastAggressor?.Id))
                {
                    playerKilledLightkeeperFriendlyPlayer(lastAggressor);
                    break;
                }
            }
        }

        private void playerKilledLightkeeperFriendlyPlayer(IPlayer player)
        {
            if (player == null)
            {
                return;
            }

            // Set players Lk standing to negative (allows access to quest chain (Making Amends))
            player.Profile.TradersInfo[_lightKeeperTid].SetStanding(-0.01);

            // Disable access to Lightkeepers door for the player
            _gameWorld.BufferZoneController.SetPlayerAccessStatus(player.ProfileId, false);

            RecodableItemClass transmitter = GetTransmitterFromInventory(player);
            if ((transmitter != null) && IsTransmitterActive(transmitter))
            {
                transmitter.RecodableComponent.SetStatus(RadioTransmitterStatus.Yellow);
                transmitter.RecodableComponent.SetEncoded(false);
            }

            RemoveLightkeeperFriendlyPlayer(player);

            _logger.LogInfo($"Removed Lightkeeper access for {player.Profile.Nickname}");
        }

        private void botCreated(BotOwner bot)
        {
            // Make sure the bot is Zryachiy or one of his followers
            if (bot.Side != EPlayerSide.Savage)
            {
                return;
            }

            // Check if the bot is Zryachiy or one of his followers
            if (isZryachiyOrFollower(bot))
            {
                // Subscribe to bots OnDeath event
                bot.GetPlayer.OnPlayerDead += OnLightkeeperFriendlyPlayerDead;
            }
        }

        private static bool isZryachiyOrFollower(IPlayer player)
        {
            if (player == null || !player.IsAI)
            {
                return false;
            }

            return player.AIData.BotOwner.IsRole(WildSpawnType.bossZryachiy) || player.AIData.BotOwner.IsRole(WildSpawnType.followerZryachiy);
        }
    }
}
