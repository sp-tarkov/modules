using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Aki.Debugging.BTR.Models
{
    public class BtrConfigModel
    {
        [JsonProperty("moveSpeed")]
        public float MoveSpeed { get; set; }

        [JsonProperty("coverFireTime")]
        public float CoverFireTime { get; set; }

        [JsonProperty("pointWaitTime")]
        public BtrMinMaxValue PointWaitTime { get; set; }

        [JsonProperty("taxiWaitTime")]
        public float TaxiWaitTime { get; set; }
    }

    public class BtrMinMaxValue
    {
        [JsonProperty("min")]
        public float Min { get; set; }

        [JsonProperty("max")]
        public float Max { get; set; }
    }
}
