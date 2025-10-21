using DefaultNamespace.Collectables;
using UnityEngine;
using UnityEngine.Serialization;

namespace Game.Buffs
{
    public class SpeedBuffPickup : CollectableBase
    {
        [FormerlySerializedAs("_operation")] [SerializeField] private SpeedBuffType _buffType = SpeedBuffType.Multiply;
        [SerializeField] private float _value = 1.25f;
        [SerializeField] private float _duration = 5f;
        
        public SpeedBuffType BuffType => _buffType;
        public float Value => _value;
        public float Duration => _duration;
    }
}