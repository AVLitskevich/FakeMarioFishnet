using FishNet.Managing;
using FishNet.Transporting;
using FishNet.Transporting.UTP;
using UnityEngine;
using VContainer;

namespace Networking
{
    public class ClientConnectionHandler : IConnectionHandler
    {
        [Inject] private readonly NetworkManager _networkManager;
        [Inject] private readonly ConnectionConfig _connectionConfig;
        
        public void Connect()
        {
            var host = _connectionConfig.ConnectToHost;
            var port = host.UseEncryption ? host.EncryptionConnectPort : host.ServerPort;
            Debug.Log($"Connecting to {host.IpAddress}:{port}");
            
            var transport = _networkManager.TransportManager.Transport;
            if (transport is UnityTransport unityTransport)
            {
                unityTransport.UseEncryption = host.UseEncryption;
                unityTransport.SetPort(port);
                unityTransport.SetClientAddress(host.IpAddress);
                unityTransport.SetClientSecrets(host.ServerName);
            }

            _networkManager.ClientManager.OnClientConnectionState += OnClientConnectionState;
            _networkManager.ClientManager.StartConnection();
        }

        public void Disconnect()
        {
            _networkManager.ClientManager.OnClientConnectionState -= OnClientConnectionState;
            _networkManager.ClientManager.StopConnection();
        }

        private void OnClientConnectionState(ClientConnectionStateArgs state)
        {
            Debug.Log($"Client connection state: {state.ConnectionState}");
        }
    }
}