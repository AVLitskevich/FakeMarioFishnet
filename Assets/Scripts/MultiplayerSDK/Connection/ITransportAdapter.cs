using System;

namespace MultiplayerSDK.Connection
{
    public interface ITransportAdapter
    {
        event Action OnConnected;
        event Action<DisconnectionReason> OnDisconnected;
        
        bool IsConnected { get; }
        bool IsServer { get; }
        bool IsClient { get; }
        
        void StartServer(ConnectionConfig config);
        void StartClient(ConnectionConfig config);
        void Disconnect();
    }
}