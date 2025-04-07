using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Comfort.Common;
using EFT;
using EFT.UI;
using UnityEngine;

namespace SPT.Debugging.Scripts;

public class BotmonitorScript : MonoBehaviour
{
    private GUIContent _guiContent;
    public GUIStyle TextStyle;
    private Player _player;
    private Camera _camera;
    private Dictionary<string, List<Player>> _zoneAndPlayers = new Dictionary<string, List<Player>>();
    private Dictionary<string, BotRoleAndDiffClass> _playerRoleAndDiff = new Dictionary<string, BotRoleAndDiffClass>();
    private List<BotZone> _zones;
    private GameWorld _gameWorld;
    private IBotGame _botGame;
    private Rect _rect;
    private Vector2 _guiSize;
    private float _distance;
    private StringBuilder _builder = new StringBuilder();

    public void Awake()
    {
        try
        {
            // Get GameWorld Instance
            _gameWorld = Singleton<GameWorld>.Instance;

            // Get BotGame Instance
            _botGame = Singleton<IBotGame>.Instance;

            // Get Player from GameWorld
            _player = _gameWorld.MainPlayer;

            // get camera of player
            _camera = CameraClass.Instance.Camera;

            // Make new rect to use for GUI
            _rect = new Rect(0, 60, 0, 0);

            // Get all BotZones - can get for MPT
            _zones = LocationScene.GetAllObjects<BotZone>().ToList();

            // Set up the Dictionary - can get for MPT
            foreach (var botZone in _zones)
            {
                _zoneAndPlayers.Add(botZone.name, new List<Player>());
            }

            // Add existing Players to list
            if (_gameWorld.AllAlivePlayersList.Count > 1)
            {
                foreach (var player in _gameWorld.AllAlivePlayersList)
                {
                    if (player.IsYourPlayer)
                    {
                        continue;
                    }

                    _playerRoleAndDiff.Add(player.ProfileId, GetBotRoleAndDiffClass(player.Profile.Info));
                    var theirZone = player.AIData.BotOwner.BotsGroup.BotZone.NameZone;
                    _zoneAndPlayers[theirZone].Add(player);
                }
            }

            // Sub to Event to get and add Bot when they spawn
            _botGame.BotsController.BotSpawner.OnBotCreated += OnBotCreatedHandler;

            // Sub to event to get and remove Bot when they despawn
            _botGame.BotsController.BotSpawner.OnBotRemoved += OnBotRemovedHandler;
        }
        catch (Exception e)
        {
            ConsoleScreen.LogError("Exception in BotMonitorScript.Awake");
            ConsoleScreen.LogError(e.Message);
        }
    }

    public void OnBotCreatedHandler(BotOwner owner)
    {
        var player = owner.GetPlayer;
        _zoneAndPlayers[owner.BotsGroup.BotZone.NameZone].Add(player);
        _playerRoleAndDiff.Add(player.ProfileId, GetBotRoleAndDiffClass(player.Profile.Info));
    }

    public void OnBotRemovedHandler(BotOwner owner)
    {
        var player = owner.GetPlayer;
        _zoneAndPlayers[owner.BotsGroup.BotZone.NameZone].Remove(player);
        _playerRoleAndDiff.Remove(player.ProfileId);
    }

    public BotRoleAndDiffClass GetBotRoleAndDiffClass(InfoClass info)
    {
        var settings = info.Settings;
        var role = settings.Role.ToString();
        var diff = settings.BotDifficulty.ToString();

        return new BotRoleAndDiffClass(string.IsNullOrEmpty(role) ? "" : role, string.IsNullOrEmpty(diff) ? "" : diff);
    }

    public void OnGUI()
    {
        try
        {
            // set basics on GUI
            if (TextStyle == null)
            {
                TextStyle = new GUIStyle(GUI.skin.box)
                {
                    alignment = TextAnchor.MiddleLeft,
                    fontSize = 14,
                    margin = new RectOffset(3, 3, 3, 3)
                };
            }

            // new GUI Content
            if (_guiContent == null)
            {
                _guiContent = new GUIContent();
            }

            _builder.Clear();

            _builder.Append($"Alive & Loading = {_botGame.BotsController.BotSpawner.AliveAndLoadingBotsCount}\n");
            _builder.Append($"Delayed Bots = {_botGame.BotsController.BotSpawner.BotsDelayed}\n");
            _builder.Append($"All Bots With Delayed = {_botGame.BotsController.BotSpawner.AllBotsWithDelayed}\n");

            foreach (var zone in _zoneAndPlayers)
            {
                if (_zoneAndPlayers[zone.Key].FindAll(x => x.HealthController.IsAlive).Count <= 0)
                {
                    continue;
                }

                _builder.Append($"{zone.Key} = {_zoneAndPlayers[zone.Key].FindAll(x => x.HealthController.IsAlive).Count}\n");

                foreach (var player in _zoneAndPlayers[zone.Key].Where(player => player.HealthController.IsAlive))
                {
                    _distance = Vector3.Distance(player.Transform.position, _camera.transform.position);
                    _builder.Append(
                        $"> [{_distance:n2}m] [{_playerRoleAndDiff.First(x => x.Key == player.ProfileId).Value.Role}] " +
                        $"[{player.Profile.Side}] [{_playerRoleAndDiff.First(x => x.Key == player.ProfileId).Value.Difficulty}] {player.Profile.Nickname}\n");
                }
            }

            _guiContent.text = _builder.ToString();

            _guiSize = TextStyle.CalcSize(_guiContent);

            _rect.x = Screen.width - _guiSize.x - 5f;
            _rect.width = _guiSize.x;
            _rect.height = _guiSize.y;

            GUI.Box(_rect, _guiContent, TextStyle);
        }
        catch (Exception e)
        {
            ConsoleScreen.LogError("Exception on BotMonitorScript.OnGui");
            ConsoleScreen.LogError(e.Message);
        }
    }

    public void DestorySelf()
    {
        Destroy(this);
    }
}

public class BotRoleAndDiffClass
{
    public BotRoleAndDiffClass(string role = "", string difficulty = "")
    {
        Role = role;
        Difficulty = difficulty;
    }

    public string Role { get; set; }
    public string Difficulty { get; set; }
}