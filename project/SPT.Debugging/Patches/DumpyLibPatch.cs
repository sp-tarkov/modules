using System.Reflection;
using EFT;
using EFT.Bots;
using EFT.UI;
using EFT.Weather;
using HarmonyLib;
using JsonType;
using SPT.Reflection.Patching;
using SPT.Reflection.Utils;
using UnityEngine;

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
    public Class304 _session;
    public TarkovApplication _tarkovApplication;
    public FieldInfo _mainMenuController;
    public WaveInfo _wavesSettings;
    public LocalRaidSettings _localRaidSettings;
    public RaidSettings _raidSettings;
    public LocationSettingsClass _locationSettings;
    public GClass1952 _endRaidClass;
    public GClass1991 _completeProfile;
    public GClass822 _parsedDataProfile;
    // Class references are as of assembly 33420 - 26/12/2024

    private void Start()
    {
        _session = ClientAppUtils.GetClientApp().Session as Class304;
        _tarkovApplication = ClientAppUtils.GetMainApp();
        _mainMenuController = _tarkovApplication.GetType().GetField("mainMenuController"); // is null at this point so only get fieldinfo - TODO: fieldinfo came back as null
        _wavesSettings = new WaveInfo(2, WildSpawnType.assault, BotDifficulty.normal); // imitate loading json of wave settings
        _localRaidSettings = new LocalRaidSettings // this has changed from current repo json TODO: check dumped data for changes here
        {
            serverId = null,
            location = "Interchange",
            timeVariant = EDateTime.CURR,
            mode = ELocalMode.TRAINING,
            playerSide = ESideType.Pmc,
        };
        _raidSettings = new RaidSettings // this has changed from current repo json TODO: check dumped data for changes here
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
            PlayersSpawnPlace = EPlayersSpawnPlace.SamePlace
        };
        _locationSettings = _session.LocationSettings;
        _endRaidClass = new GClass1952
        {
            profile = null,
            result = ExitStatus.Left,
            killerId = null,
            killerAid = null,
            exitName = null,
            inSession = true,
            favorite = false,
            playTime = 33,
            InsuredItems = new GClass1321[] {},
            ProfileId = ""
        };
        _completeProfile = new GClass1991(_session.Profile, GClass2000.Instance);

        _parsedDataProfile = _completeProfile.ToUnparsedData();
        _endRaidClass.profile = _completeProfile.ToUnparsedData();
    }
}