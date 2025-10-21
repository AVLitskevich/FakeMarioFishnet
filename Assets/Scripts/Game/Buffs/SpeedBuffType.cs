using System;
using FishNet.Object.Synchronizing;

namespace Game.Buffs
{
    public enum SpeedBuffType
    {
        Add,
        Multiply
    }

    [Serializable]
    public struct SpeedBuffData
    {
        public int Id;
        public SpeedBuffType BuffType;
        public float Value;
        public float StartTime;
        public float Duration;

        public float EndTime => StartTime + Duration;
        
        private static int _nextId;
        
        public static SpeedBuffData Create(SpeedBuffType buffType, float value, float duration, float startTime)
        {
            return new SpeedBuffData
            {
                Id = ++_nextId,
                BuffType = buffType,
                Value = value,
                Duration = duration,
                StartTime = startTime
            };
        }
        
        public bool IsExpired(float now)
        {
            return Duration > 0f && now >= EndTime;
        }
    }
}