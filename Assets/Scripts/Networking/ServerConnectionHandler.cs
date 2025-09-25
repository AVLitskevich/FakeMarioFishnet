using FishNet.Connection;
using FishNet.Managing;
using FishNet.Transporting;
using FishNet.Transporting.UTP;
using UnityEngine;
using VContainer;

namespace Networking
{
    public class ServerConnectionHandler : IConnectionHandler
    {
        [Inject] private readonly NetworkManager _networkManager;
        [Inject] private readonly ConnectionConfig _connectionConfig;
        
        public void Connect()
        {
            var transport = _networkManager.TransportManager.GetTransport<UnityTransport>();
            transport.SetPort(_connectionConfig.ServerHost.ServerPort);
            transport.UseEncryption = false;

            _networkManager.ServerManager.OnServerConnectionState += OnConnectionState;
            _networkManager.ServerManager.OnRemoteConnectionState += OnClientConnectionState;
            _networkManager.ServerManager.StartConnection();
        }

        private void OnClientConnectionState(NetworkConnection clientConnection, RemoteConnectionStateArgs state)
        {
            Debug.Log($"Client {clientConnection.ClientId} connection state: {state.ConnectionState}");
        }

        private void OnConnectionState(ServerConnectionStateArgs state)
        {
            Debug.Log($"Server connection state: {state.ConnectionState}");
        }

        public void Disconnect()
        {
            _networkManager.ServerManager.OnServerConnectionState -= OnConnectionState;
            _networkManager.ServerManager.OnRemoteConnectionState -= OnClientConnectionState;
            _networkManager.ServerManager.StopConnection(true);
        }
    }
}