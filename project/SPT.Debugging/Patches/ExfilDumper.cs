﻿using SPT.Reflection.Patching;
using Comfort.Common;
using EFT;
using EFT.Interactive;
using EFT.InventoryLogic;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace SPT.Debugging.Patches
{
    public class ExfilDumper : ModulePatch
    {
        public static string DumpFolder = Path.Combine(Environment.CurrentDirectory, "Dumps");

        protected override MethodBase GetTargetMethod()
        {
            return typeof(ExfiltrationControllerClass).GetMethod(nameof(ExfiltrationControllerClass.InitAllExfiltrationPoints));
        }

        [PatchPostfix]
        public static void PatchPreFix(LocationExitClass[] settings)
        {
            var gameWorld = Singleton<GameWorld>.Instance;
            string mapName = gameWorld.MainPlayer.Location.ToLower();

            var pmcExfilPoints = ExfiltrationControllerClass.Instance.ExfiltrationPoints;

            // Both scav and PMC lists include shared, so remove them from the scav list
            var scavExfilPoints = ExfiltrationControllerClass.Instance.ScavExfiltrationPoints.Where(x => !(x is SharedExfiltrationPoint));

            var exfils = new List<SPTExfilData>();

            foreach (var exfil in pmcExfilPoints.Concat(scavExfilPoints))
            {
                LocationExitClass exitSettings = settings.FirstOrDefault(x => x.Name == exfil.Settings.Name);
                exfils.Add(new SPTExfilData(exfil, exitSettings));
            }

            string jsonString = JsonConvert.SerializeObject(exfils, Formatting.Indented);
            string outputFile = Path.Combine(DumpFolder, mapName, "allExtracts.json");
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

        public class SPTExfilData
        {
            public float Chance = 0;
            public float ChancePVE = 0;
            public int Count = 0;
            public int CountPve = 0;
            public string EntryPoints = "";
            public bool EventAvailable = false;
            public float ExfiltrationTime = 0;
            public float ExfiltrationTimePVE = 0;
            public EExfiltrationType ExfiltrationType;
            public string Id = "";
            public float MinTime = 0;
            public float MinTimePVE = 0;
            public float MaxTime = 0;
            public float MaxTimePVE = 0;
            public string Name = "";
            public ERequirementState PassageRequirement;
            public int PlayersCount = 0;
            public int PlayersCountPVE = 0;
            public EquipmentSlot RequiredSlot;
            public string RequirementTip = "";
            public string Side = "";

            public SPTExfilData(ExfiltrationPoint point, LocationExitClass settings)
            {
                // PMC and shared extracts, prioritize settings over the map data to match base behaviour
                if (settings != null && (!(point is ScavExfiltrationPoint) || point is SharedExfiltrationPoint))
                {
                    if (settings != null)
                    {
                        EntryPoints = settings.EntryPoints;
                        Chance = settings.Chance;
                        ChancePVE = settings.Chance;
                        EventAvailable = settings.EventAvailable;
                        ExfiltrationTime = settings.ExfiltrationTime;
                        ExfiltrationTimePVE = settings.ExfiltrationTime;
                        ExfiltrationType = settings.ExfiltrationType;
                        MaxTime = settings.MaxTime;
                        MaxTimePVE = settings.MaxTime;
                        MinTime = settings.MinTime;
                        MinTimePVE = settings.MinTime;
                        Name = settings.Name;
                        PlayersCount = settings.PlayersCount;
                        PlayersCountPVE = settings.PlayersCount;
                        CountPve = 0;
                    }
                }
                // Scav extracts, and those without settings use the point settings
                else
                {
                    EntryPoints = String.Join(",", point.EligibleEntryPoints);
                    Chance = point.Settings.Chance;
                    ChancePVE = point.Settings.Chance;
                    EventAvailable = point.Settings.EventAvailable;
                    ExfiltrationTime = point.Settings.ExfiltrationTime;
                    ExfiltrationTimePVE = point.Settings.ExfiltrationTime;
                    ExfiltrationType = point.Settings.ExfiltrationType;
                    MaxTime = point.Settings.MaxTime;
                    MinTime = point.Settings.MinTime;
                    Name = point.Settings.Name;
                    PlayersCount = point.Settings.PlayersCount;
                    PlayersCountPVE = point.Settings.PlayersCount;
                }

                // If there's settings, and the requirement is a reference, use that
                if (settings?.PassageRequirement == ERequirementState.Reference)
                {
                    PassageRequirement = ERequirementState.Reference;
                    Id = settings.Id;
                }
                // Otherwise use the point requirements
                else if (point.HasRequirements)
                {
                    Count = point.Requirements[0].Count;
                    Id = point.Requirements[0].Id;
                    PassageRequirement = point.Requirements[0].Requirement;
                    RequiredSlot = point.Requirements[0].RequiredSlot;
                    RequirementTip = point.Requirements[0].RequirementTip;
                }

                // Store the side
                if (point is SharedExfiltrationPoint)
                {
                    Side = "Coop";
                }
                else if (point is ScavExfiltrationPoint)
                {
                    Side = "Scav";
                }
                else
                {
                    Side = "Pmc";
                }
            }
        }
    }
}
