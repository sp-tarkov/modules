using System.Collections.Generic;

namespace Aki.SinglePlayer.Models.Healing
{
    public class PlayerHealth
    {
        private readonly Dictionary<EBodyPart, BodyPartHealth> _health;
        public IReadOnlyDictionary<EBodyPart, BodyPartHealth> Health => _health;
        public bool IsAlive { get; set; }
        public float Hydration { get; set; }
        public float Energy { get; set; }
        public float Temperature { get; set; }

        public PlayerHealth()
        {
            IsAlive = true;
            _health = new Dictionary<EBodyPart, BodyPartHealth>()
            {
                { EBodyPart.Head, new BodyPartHealth() },
                { EBodyPart.Chest, new BodyPartHealth() },
                { EBodyPart.Stomach, new BodyPartHealth() },
                { EBodyPart.LeftArm, new BodyPartHealth() },
                { EBodyPart.RightArm, new BodyPartHealth() },
                { EBodyPart.LeftLeg, new BodyPartHealth() },
                { EBodyPart.RightLeg, new BodyPartHealth() }
            };
        }
    }
}
