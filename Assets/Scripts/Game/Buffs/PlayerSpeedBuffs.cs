using DefaultNamespace.Collectables;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using Game.Player;
using UnityEngine;

namespace Game.Buffs
{
    [RequireComponent(typeof(PlayerMovement))]
    [RequireComponent(typeof(PlayerCollector))]
    public class PlayerSpeedBuffs : NetworkBehaviour
    {
        private readonly SyncList<SpeedBuffData> _activeBuffs = new(new SyncTypeSettings(writePermissions: WritePermission.ServerOnly));
        
        private PlayerMovement _movement;
        private PlayerCollector _collector;
        
        private void Awake()
        {
            _movement = GetComponent<PlayerMovement>();
            _collector = GetComponent<PlayerCollector>();
        }

        public override void OnStartNetwork()
        {
            base.OnStartNetwork();
            _activeBuffs.OnChange += OnActiveBuffsChanged;
            if (IsServerInitialized)
            {
                _collector.OnCollectedServer += OnCollectedServer;
            }
            Recalculate();
        }
        
        public override void OnStopNetwork()
        {
            base.OnStopNetwork();

            _activeBuffs.OnChange -= OnActiveBuffsChanged;

            if (IsServerInitialized && _collector != null)
            {
                _collector.OnCollectedServer -= OnCollectedServer;
            }
        }

        private void Update()
        {
            if (!IsServerInitialized)
            {
                return;
            }
            
            float now = Time.time;
            for (int i = _activeBuffs.Count - 1; i >= 0; i--)
            {
                if (_activeBuffs[i].IsExpired(now))
                {
                    _activeBuffs.RemoveAt(i);
                }
            }
        }

        private void OnCollectedServer(PlayerCollector collector, CollectableBase collectable, CollectableType type)
        {
            if (collectable is not SpeedBuffPickup pickup)
            {
                return;
            }
            
            var data = SpeedBuffData.Create(pickup.BuffType, pickup.Value, pickup.Duration, Time.time);
            
            _activeBuffs.Add(data);
        }

        private void OnActiveBuffsChanged(SyncListOperation op, int index, SpeedBuffData oldItem, SpeedBuffData newItem, bool asServer)
        {
            Recalculate();
        }

        private void Recalculate()
        {
            if (_movement == null)
            {
                return;
            }
            
            var values = _movement.GetRuntimeValues();
            
            values.ResetToConfig(_movement.Config);
            
            float multiplier = 1f;
            float addition = 0f;

            foreach (var buff in _activeBuffs)
            {
                if (buff.BuffType == SpeedBuffType.Multiply)
                {
                    multiplier *= buff.Value;
                }
                else if (buff.BuffType == SpeedBuffType.Add)
                {
                    addition += buff.Value;
                }
            }
            
            values.Speed = values.Speed * multiplier + addition;
            
            _movement.SetRuntimeValues(values);
        }
    }
}