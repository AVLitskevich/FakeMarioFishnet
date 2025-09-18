using FishNet.Broadcast;
using FishNet.Connection;

namespace DefaultNamespace
{
    public struct CountdownMessage : IBroadcast
    {
        public int CountdownValue;
    }

    public struct RaceStartedMessage : IBroadcast
    {
        public float StartTime;
    }

    public struct RaceEndedMessage : IBroadcast
    {
        public NetworkConnection Winner;
        public float WinTime;
    }
    
    public struct RaceResetMessage : IBroadcast
    {
        public bool Value;
    }
}