using FishNet.Component.Prediction;
using FishNet.Utility.Template;
using Game.Player;
using UnityEngine;

namespace Game.Level
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
            if (obj.TryGetComponent(out PlayerMovement movement))
                movement.TakeDamage(this);
        }
    }
}