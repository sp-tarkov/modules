using Newtonsoft.Json;

namespace SPT.Common.Utils
{
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
    }
}
