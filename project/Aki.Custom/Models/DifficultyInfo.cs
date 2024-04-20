using System.Runtime.Serialization;

namespace Aki.Custom.Models
{
    [DataContract]
    public struct DifficultyInfo
    {
        [DataMember(Name = "role")]
        public string Role;

        [DataMember(Name = "difficulty")]
        public string Difficulty;

        [DataMember(Name = "data")]
        public string Data;

        public DifficultyInfo(string role, string difficulty, string data)
        {
            Role = role;
            Difficulty = difficulty;
            Data = data;
        }
    }
}