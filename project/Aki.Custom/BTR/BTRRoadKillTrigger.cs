using EFT;
using EFT.Interactive;
using UnityEngine;

namespace Aki.Custom.BTR
{
    public class BTRRoadKillTrigger : DamageTrigger
    {
        public override bool IsStatic => false;

        public override void AddPenalty(GInterface94 player)
        {
        }

        public override void PlaySound()
        {
        }

        public override void ProceedDamage(GInterface94 player, BodyPartCollider bodyPart)
        {
            bodyPart.ApplyInstantKill(new DamageInfo()
            {
                Damage = 9999f,
                Direction = Vector3.zero,
                HitCollider = bodyPart.Collider,
                HitNormal = Vector3.zero,
                HitPoint = Vector3.zero,
                DamageType = EDamageType.Btr,
                HittedBallisticCollider = bodyPart,
                Player = null
            });
        }

        public override void RemovePenalty(GInterface94 player)
        {
        }
    }
}
