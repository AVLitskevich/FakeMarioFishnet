using System;
using System.Collections.Generic;
using FishNet.Connection;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using FishNet.Transporting;
using MultiplayerSDK.Common;

namespace FishNetAdapter
{
    public class FishNetPlayerDataService : NetworkBehaviour
    {
        public event Action<int, PlayerData> OnPlayerAdded; 
        public event Action<int, PlayerData> OnPlayerUpdated; 
        public event Action<int, PlayerData> OnPlayerRemoved;

        public IReadOnlyDictionary<int, PlayerData> PlayerData => _playerData;
        
        private readonly SyncDictionary<int, PlayerData> _playerData = new(new SyncTypeSettings(WritePermission.ServerOnly));

        public override void OnStartNetwork()
        {
            base.OnStartNetwork();
            _playerData.OnChange += OnPlayerDataChanged;

            if (IsServerInitialized)
                NetworkManager.ServerManager.OnRemoteConnectionState += OnClientConnectionState;
        }

        public override void OnStopNetwork()
        {
            base.OnStopNetwork();
            _playerData.OnChange -= OnPlayerDataChanged;
            
            if (IsServerInitialized)
                NetworkManager.ServerManager.OnRemoteConnectionState -= OnClientConnectionState;
        }

        public bool TryGetData(int id, out PlayerData playerData) => _playerData.TryGetValue(id, out playerData);
        public void SetData(int id, PlayerData playerData) => _playerData[id] = playerData;

        [ServerRpc(RequireOwnership = true)]
        public void UpdateData(PlayerData playerData, NetworkConnection connection = null)
        {
            if (connection == null)
                return;

            _playerData[connection.ClientId] = playerData;
        }

        private void OnClientConnectionState(NetworkConnection connection, RemoteConnectionStateArgs stateArgs)
        {
            if (stateArgs.ConnectionState == RemoteConnectionState.Stopped)
                _playerData.Remove(connection.ClientId);
        }

        private void OnPlayerDataChanged(SyncDictionaryOperation op, int key, PlayerData value, bool asServer)
        {
            if (op == SyncDictionaryOperation.Add)
                OnPlayerAdded?.Invoke(key, value);
            else if (op == SyncDictionaryOperation.Set)
                OnPlayerUpdated?.Invoke(key, value);
            else if (op == SyncDictionaryOperation.Remove)
                OnPlayerRemoved?.Invoke(key, value);
        }
    }
}