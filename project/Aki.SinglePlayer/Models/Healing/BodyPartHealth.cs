using System.Collections.Generic;

namespace Aki.SinglePlayer.Models.Healing
{
    public class BodyPartHealth
    {
        private readonly Dictionary<EBodyPartEffect, float> _effects = new Dictionary<EBodyPartEffect, float>();
        public float Maximum { get; private set; }
        public float Current { get; private set; }

        public IReadOnlyDictionary<EBodyPartEffect, float> Effects => _effects;

        public void Initialize(float current, float maximum)
        {
            Maximum = maximum;
            Current = current;
        }

        public void ChangeHealth(float diff)
        {
            Current += diff;
        }

        public void AddEffect(EBodyPartEffect bodyPartEffect, float time = -1)
        {
            _effects[bodyPartEffect] = time;
        }

        public void UpdateEffect(EBodyPartEffect bodyPartEffect, float time)
        {
            _effects[bodyPartEffect] = time;
        }


        public void RemoveAllEffects()
        {
            _effects.Clear();
        }

        public void RemoveEffect(EBodyPartEffect bodyPartEffect)
        {
            if (_effects.ContainsKey(bodyPartEffect))
            {
                _effects.Remove(bodyPartEffect);
            }
        }
    }
}
