using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace SPT.Custom.Models;

public class ClientMod
{
    [JsonProperty("modName")]
    public string Name { get; set; }

    [JsonProperty("modGUID")]
    public string GUID { get; set; }

    [JsonProperty("modVersion")]
    public Version Version { get; set; }
}

public class ClientModsRequest(List<ClientMod> activeMods)
{
    [JsonProperty("activeClientMods")]
    public List<ClientMod> ActiveClientMods = activeMods;
}
