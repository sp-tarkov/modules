using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace SPT.Common.Utils;

public static class Json
{
    public static string Serialize<T>(T data)
    {
        return JsonConvert.SerializeObject(data);
    }

    public static T Deserialize<T>(string json)
    {
        return JsonConvert.DeserializeObject<T>(json);
    }

    public static T Deserialize<T>(JObject json)
    {
        return json.ToObject<T>();
    }
}
