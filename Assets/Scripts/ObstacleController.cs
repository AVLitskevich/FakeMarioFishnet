using FishNet.Component.Prediction;
using FishNet.Utility.Template;
using UnityEngine;

namespace DefaultNamespace
{
    public class ObstacleController : TickNetworkBehaviour
    {
        [SerializeField] public float Damage;
        [SerializeField] private NetworkTrigger2D _trigger;

        private void OnEnable()
        {
            _trigger.OnEnter += OnTrigger;
        }

        private void OnDisable()
        {
            _trigger.OnEnter -= OnTrigger;
        }

        private void OnTrigger(Collider2D obj)
        {
            if (obj.TryGetComponent(out PlayerController playerController))
                playerController.TakeDamage(this);
        }
    }
}