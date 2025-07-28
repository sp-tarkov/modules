using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Comfort.Common;
using EFT;
using EFT.Interactive;
using Newtonsoft.Json;
using SPT.Reflection.Patching;
using UnityEngine;

namespace SPT.Debugging.Patches;

public class StaticLootDumper : ModulePatch
{
    public static string DumpFolder = Path.Combine(Environment.CurrentDirectory, "Dumps");

    protected override MethodBase GetTargetMethod()
    {
        return typeof(GameWorld).GetMethod(nameof(GameWorld.OnGameStarted));
    }

    [PatchPrefix]
    public static void PatchPreFix()
    {
        InitDirectory();

        var gameWorld = Singleton<GameWorld>.Instance;
        string mapName = gameWorld.MainPlayer.Location.ToLower();

        var containersData = new SPTContainersData();

        Resources
            .FindObjectsOfTypeAll(typeof(LootableContainersGroup))
            .ExecuteForEach(obj =>
            {
                var containersGroup = (LootableContainersGroup)obj;
                var sptContainersGroup = new SPTContainersGroup
                {
                    minContainers = containersGroup.Min,
                    maxContainers = containersGroup.Max,
                };
                if (containersData.containersGroups.ContainsKey(containersGroup.Id))
                {
                    Logger.LogError(
                        $"Container group ID {containersGroup.Id} already exists in dictionary!"
                    );
                }
                else
                {
                    containersData.containersGroups.Add(containersGroup.Id, sptContainersGroup);
                }
            });

        Resources
            .FindObjectsOfTypeAll(typeof(LootableContainer))
            .ExecuteForEach(obj =>
            {
                var container = (LootableContainer)obj;

                // Skip empty ID containers
                if (container.Id.Length == 0)
                {
                    return;
                }

                if (containersData.containers.ContainsKey(container.Id))
                {
                    Logger.LogError($"Container {container.Id} already exists in dictionary!");
                }
                else
                {
                    containersData.containers.Add(
                        container.Id,
                        new SPTContainer { groupId = container.LootableContainersGroupId }
                    );
                }
            });

        string jsonString = JsonConvert.SerializeObject(containersData, Formatting.Indented);
        string outputFile = Path.Combine(DumpFolder, $"{mapName}", $"statics.json");
        Directory.CreateDirectory(Path.GetDirectoryName(outputFile));
        if (File.Exists(outputFile))
        {
            File.Delete(outputFile);
        }
        File.Create(outputFile).Dispose();
        StreamWriter streamWriter = new StreamWriter(outputFile);
        streamWriter.Write(jsonString);
        streamWriter.Flush();
        streamWriter.Close();
    }

    public static void InitDirectory()
    {
        Directory.CreateDirectory(DumpFolder);
    }
}

public class SPTContainer
{
    public string groupId;
}

public class SPTContainersGroup
{
    public int minContainers;
    public int maxContainers;
}

public class SPTContainersData
{
    public Dictionary<string, SPTContainersGroup> containersGroups =
        new Dictionary<string, SPTContainersGroup>();
    public Dictionary<string, SPTContainer> containers = new Dictionary<string, SPTContainer>();
}