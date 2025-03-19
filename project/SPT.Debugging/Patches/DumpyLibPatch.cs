using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using BepInEx.Logging;
using EFT;
using EFT.Bots;
using EFT.UI;
using EFT.Weather;
using HarmonyLib;
using JsonType;
using SPT.Reflection.Patching;
using SPT.Reflection.Utils;
using UnityEngine;
using Logger = BepInEx.Logging.Logger;

namespace SPT.Debugging.Patches;

/// <summary>
/// Used to debug dumpLib issues https://dev.sp-tarkov.com/SPT/AssemblyTool/src/branch/master/DumpLib/DumpyTool.cs
/// </summary>
public class DumpyLibPatch : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return AccessTools.Method(typeof(MenuScreen), nameof(MenuScreen.Awake));
    }

    [PatchPostfix]
    public static void PatchPostfix(MenuScreen __instance)
    {
        // attach Monobehaviour so we can interact with UE
        SPTDebuggingPlugin.HookObject.AddComponent<DumplyLibMono>();
    }
}

public class DumplyLibMono : MonoBehaviour
{
    public Class303 _session;
    public TarkovApplication _tarkovApplication;
    public FieldInfo _mainMenuController;
    public WaveInfoClass _wavesSettings;
    public LocalRaidSettings _localRaidSettings;
    public RaidSettings _raidSettings;
    public LocationSettingsClass _locationSettings;
    public List<LocationSettingsClass.Location> _locationSettingsDict;
    public RaidEndDescriptorClass _endRaidClass;
    public CompleteProfileDescriptorClass _completeProfile;
    public GClass828 _parsedDataProfile;
    public ManualLogSource _dumpLogger;
    // Class references are as of assembly 35328 - 28/02/2025

    private void Start()
    {
        _dumpLogger = Logger.CreateLogSource(nameof(DumplyLibMono));
        Task.Factory.StartNew(() => StartTask(), TaskCreationOptions.LongRunning);
    }

    private async Task StartTask()
    {
        await Task.Delay(5000);

        _session = ClientAppUtils.GetClientApp().Session as Class303;
        _tarkovApplication = ClientAppUtils.GetMainApp();
        _mainMenuController = _tarkovApplication.GetType().GetField("mainMenuController");
        _wavesSettings = new WaveInfoClass(2, WildSpawnType.assault, BotDifficulty.normal);
        _localRaidSettings = new LocalRaidSettings
        {
            serverId = null,
            location = "Interchange",
            timeVariant = EDateTime.CURR,
            mode = ELocalMode.TRAINING,
            playerSide = ESideType.Pmc,
        };
        _raidSettings = new RaidSettings
        {
            KeyId = null,
            LocationId = "Interchange",
            SelectedDateTime = EDateTime.CURR,
            MetabolismDisabled = false,
            TimeAndWeatherSettings = new TimeAndWeatherSettings
            {
                IsRandomTime = false,
                IsRandomWeather = false,
                CloudinessType = ECloudinessType.Clear,
                RainType = ERainType.NoRain,
                WindType = EWindSpeed.Light,
                FogType = EFogType.NoFog,
                TimeFlowType = ETimeFlowType.x1,
                HourOfDay = -1
            },
            BotSettings = new BotControllerSettings
            {
                IsScavWars = false,
                BotAmount = EBotAmount.AsOnline
            },
            WavesSettings = new WavesSettings
            {
                BotAmount = EBotAmount.AsOnline,
                BotDifficulty = EBotDifficulty.AsOnline,
                IsBosses = true,
                IsTaggedAndCursed = false
            },
            Side = ESideType.Pmc,
            RaidMode = ERaidMode.Online,
            PlayersSpawnPlace = EPlayersSpawnPlace.SamePlace,
            OnlinePveRaid = false
        };
        _locationSettingsDict = _session.LocationSettings.locations.Values.ToList();
        _locationSettings = _session.LocationSettings;
        _raidSettings.GetType()
            .GetField("_locationSettings", BindingFlags.NonPublic | BindingFlags.Instance)
            .SetValue(_raidSettings, _locationSettings);

        _endRaidClass = new RaidEndDescriptorClass
        {
            profile = null,
            result = ExitStatus.Left,
            killerId = null,
            killerAid = null,
            exitName = null,
            inSession = true,
            favorite = false,
            playTime = 33,
            InsuredItems = [],
            ProfileId = ""
        };
        _completeProfile = new CompleteProfileDescriptorClass(_session.Profile, GClass2011.Instance /* Has 3 methods */);

        _parsedDataProfile = _completeProfile.ToUnparsedData();
        _endRaidClass.profile = _completeProfile.ToUnparsedData();
    }

    private void TestMethod()
    {
        _raidSettings.UpdateOnlinePveRaidStates();
    }
}