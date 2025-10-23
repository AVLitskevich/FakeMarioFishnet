using FishNet.Object;
using UnityEngine;

namespace DefaultNamespace.Collectables
{
    public enum CollectableType
    {
        Unknown = 0
    }
    [RequireComponent(typeof(Collider2D))]
    public class CollectableBase : NetworkBehaviour
    {
        [SerializeField] private CollectableType _type = CollectableType.Unknown;
        public CollectableType Type => _type;
        public bool Collected { get; private set; }
        public virtual bool CanBeCollectedServer(PlayerCollector playerCollector) => true;

        public bool TryCollectServer(PlayerCollector playerCollector)
        {
            if (!IsServerInitialized || Collected)
            {
                return false;
            }
            Collected = true;
            return true;
        }
        
        public virtual void OnCollectedServer(PlayerCollector playerCollector)
        {
            if (!IsServerInitialized) return;

            if (NetworkObject != null && NetworkObject.IsSpawned)
            {
                NetworkObject.Despawn();
            }
            else
            {
                Destroy(gameObject);
            }
        }
    }
}