using System;
using FishNet.Managing;
using FishNet.Transporting;
using FishNet.Transporting.UTP;
using MultiplayerSDK.Connection;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace FishNetAdapter
{
    public class FishNetTransportAdapter : ITransportAdapter, IInitializable, IDisposable
    {
        public event Action OnConnected;
        public event Action<DisconnectionReason> OnDisconnected;

        public bool IsConnected => _networkManager.IsClientStarted || _networkManager.IsServerStarted;
        public bool IsServer => _networkManager.IsServerStarted;
        public bool IsClient =>  _networkManager.IsClientStarted;

        [Inject] private NetworkManager _networkManager;
        [Inject] private UnityTransport _transport;

        public void Initialize()
        {
            _networkManager.ClientManager.OnClientConnectionState += OnClientConnectionState;
            _networkManager.ServerManager.OnServerConnectionState += OnServerConnectionState;
        }

        public void Dispose()
        {
            _networkManager.ClientManager.OnClientConnectionState -= OnClientConnectionState;
            _networkManager.ServerManager.OnServerConnectionState -= OnServerConnectionState;
        }

        private void OnClientConnectionState(ClientConnectionStateArgs stateArgs)
        {
            if (stateArgs.ConnectionState == LocalConnectionState.Started)
            {
                OnConnected?.Invoke();
            }
            else if (stateArgs.ConnectionState == LocalConnectionState.Stopped)
            {
                OnDisconnected?.Invoke(GetDisconnectReason());
            }
        }

        private void OnServerConnectionState(ServerConnectionStateArgs stateArgs)
        {
            if (stateArgs.ConnectionState == LocalConnectionState.Started)
            {
                OnConnected?.Invoke();
            }
            else if (stateArgs.ConnectionState == LocalConnectionState.Stopped)
            {
                OnDisconnected?.Invoke(DisconnectionReason.ServerStopped);
            }
        }

        public void StartClient(ConnectionConfig config)
        {
            var port = config.UseEncryption ? config.WssConnectPort : config.ServerListenPort;

            Debug.Log($"[FishNetTransportAdapter] Connecting to {config.ServerAddress}:{port}, secure: {config.UseEncryption}");
            
            _transport.UseEncryption = config.UseEncryption;
            _transport.SetPort(port);
            _transport.SetClientAddress(config.ServerAddress);
            _transport.SetClientSecrets(config.WssServerName);
            _networkManager.ClientManager.StartConnection();
        }

        public void StartServer(ConnectionConfig config)
        {
            _transport.SetPort(config.ServerListenPort);
            _transport.UseEncryption = false;
            
            _networkManager.ServerManager.StartConnection();
        }

        public void Disconnect()
        {
            if (_networkManager.IsServerStarted)
                _networkManager.ServerManager.StopConnection(true);

            if (_networkManager.IsClientStarted)
                _networkManager.ClientManager.StopConnection();
        }

        private DisconnectionReason GetDisconnectReason()
        {
            return DisconnectionReason.ConnectionLost; // TODO: check last state and set reason
        }
    }
}