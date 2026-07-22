using Newtonsoft.Json;

namespace SPT.PrePatch;

public sealed class EnumEntryDefinition
{
    [JsonProperty("enumType")]
    public string EnumType { get; set; }

    [JsonProperty("constantName")]
    public string ConstantName { get; set; }

    [JsonProperty("jsonEnumName")]
    public string JsonEnumName { get; set; }

    [JsonProperty("constantValue")]
    public long ConstantValue { get; set; }
}
