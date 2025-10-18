using System.Collections.Generic;
using FishNet.Connection;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using FishNet.Transporting;
using MultiplayerSDK.Common;
using UnityEngine;
using VContainer;

namespace MultiplayerSDK.FishNetAdapter
{
    public class FishNetPlayerDataService : NetworkBehaviour
    {
        public delegate void PlayerEvent(int clientId, PlayerData playerData);
        
        public event PlayerEvent OnPlayerAdded; 
        public event PlayerEvent OnPlayerUpdated; 
        public event PlayerEvent OnPlayerRemoved;

        public IReadOnlyDictionary<int, PlayerData> PlayerData => _playerData;
        public int LocalPlayerId => NetworkManager.ClientManager.Connection.ClientId;

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
            Debug.Log("Set local player data");
            UpdateData(playerData);
        }

        [Server]
        public void SetDataOnServer(PlayerData playerData, int clientId)
        {
            _playerData[clientId] = playerData;
        }

        [ServerRpc(RequireOwnership = false)]
        private void UpdateData(PlayerData playerData, NetworkConnection connection = null)
        {
            if (connection == null)
                return;

            // We need this, so players can't set InGame property from client. Only server should set this property
            playerData.InGame = _playerData.TryGetValue(connection.ClientId, out var existingData) &&
                                existingData.InGame;
            
            playerData.PlayerId = connection.ClientId;
            if (string.IsNullOrWhiteSpace(playerData.Nickname))
                playerData.Nickname = $"Player_{playerData.PlayerId}";

            Debug.Log("Set player data");
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
        }
    }
}