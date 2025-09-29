using System;

namespace MultiplayerSDK.Connection
{
    public interface IConnectionController
    {
        event Action<ConnectionState> OnStateChanged;
        event Action<DisconnectionReason> OnDisconnected;
        
        ConnectionState ConnectionState { get; }
        ConnectionConfig ActiveConfig { get; }

        void StartServer(ConnectionConfig config);
        void StartClient(ConnectionConfig config);
        void Disconnect();
    }
}