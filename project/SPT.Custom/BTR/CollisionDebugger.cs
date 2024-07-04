using EFT.UI;
using UnityEngine;

namespace SPT.Custom.BTR
{
    public class CollisionDebugger : MonoBehaviour
    {
        private int _resetFrame = 10;
        private int _frame = 0;

        private void Update()
        {
            _frame = (_frame + 1) % _resetFrame;
        }

        private void OnCollisionEnter(Collision collision)
        {
            foreach (var contact in collision.contacts)
            {
                ConsoleScreen.LogWarning($"Collision between {gameObject.name} and {contact.otherCollider.gameObject.name}");
            }
        }

        private void OnCollisionStay(Collision collision)
        {
            if (_frame == 0)
            {
                foreach (var contact in collision.contacts)
                {
                    ConsoleScreen.LogWarning($"Collision between {gameObject.name} and {contact.otherCollider.gameObject.name}");
                }
            }
        }
    }
}
