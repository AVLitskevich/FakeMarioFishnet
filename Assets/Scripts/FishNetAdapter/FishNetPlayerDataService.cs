using System;
using System.Collections.Generic;
using FishNet.Connection;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using FishNet.Transporting;
using MultiplayerSDK;
using MultiplayerSDK.Common;
using UnityEngine;
using VContainer;

namespace FishNetAdapter
{
    public class FishNetPlayerDataService : NetworkBehaviour
    {
        public event Action<int, PlayerData> OnPlayerAdded; 
        public event Action<int, PlayerData> OnPlayerUpdated; 
        public event Action<int, PlayerData> OnPlayerRemoved;

        public IReadOnlyDictionary<int, PlayerData> PlayerData => _playerData;

        [Inject] private readonly GlobalGameData _globalGameData;
        
        private readonly SyncDictionary<int, PlayerData> _playerData = new(new SyncTypeSettings(WritePermission.ServerOnly));

        public override void OnStartNetwork()
        {
            this.InjectToMe();
            base.OnStartNetwork();
            _playerData.OnChange += OnPlayerDataChanged;

            if (IsServerInitialized)
            {
                NetworkManager.ServerManager.OnRemoteConnectionState += OnRemoteConnectionState;
            }
            else if (IsClientInitialized)
            {
                SetDataOnLocalClient(new PlayerData
                {
                    IsReady = false,
                    Nickname = _globalGameData.Nickname,
                });
            }
        }

        public override void OnStopNetwork()
        {
            base.OnStopNetwork();
            _playerData.OnChange -= OnPlayerDataChanged;
            
            if (IsServerInitialized)
                NetworkManager.ServerManager.OnRemoteConnectionState -= OnRemoteConnectionState;
        }

        public bool TryGetData(int id, out PlayerData playerData) => _playerData.TryGetValue(id, out playerData);

        [Client]
        public bool TryGetLocalClientData(out PlayerData playerData) =>
            _playerData.TryGetValue(NetworkManager.ClientManager.Connection.ClientId, out playerData);

        [Client]
        public void SetDataOnLocalClient(PlayerData playerData)
        {
            UpdateData(playerData);
        }

        [Server]
        public void SetDataOnServer(PlayerData playerData, int clientId)
        {
            _playerData[clientId] = playerData;
        }

        [ServerRpc(RequireOwnership = true)]
        private void UpdateData(PlayerData playerData, NetworkConnection connection = null)
        {
            if (connection == null)
                return;

            playerData.PlayerId = connection.ClientId;
            _playerData[connection.ClientId] = playerData;
        }

        private void OnRemoteConnectionState(NetworkConnection connection, RemoteConnectionStateArgs stateArgs)
        {
            if (stateArgs.ConnectionState == RemoteConnectionState.Stopped)
                _playerData.Remove(connection.ClientId);
        }

        private void OnPlayerDataChanged(SyncDictionaryOperation op, int playerId, PlayerData playerData, bool asServer)
        {
            if (op == SyncDictionaryOperation.Add)
                OnPlayerAdded?.Invoke(playerId, playerData);
            else if (op == SyncDictionaryOperation.Set)
                OnPlayerUpdated?.Invoke(playerId, playerData);
            else if (op == SyncDictionaryOperation.Remove)
                OnPlayerRemoved?.Invoke(playerId, playerData);
            
            Debug.Log($"Received player data update for {playerId}, operation: {op}, as server: {asServer}, data: {playerData.ToString()}");
        }
    }
}