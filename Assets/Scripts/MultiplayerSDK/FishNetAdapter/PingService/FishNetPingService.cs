using System;
using FishNet.Managing;
using MultiplayerSDK.Connection;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace MultiplayerSDK.FishNetAdapter.PingService
{
    public class FishNetPingService : IInitializable, IDisposable
    {
        [Inject] private readonly NetworkManager _networkManager;
        [Inject] private readonly IConnectionController _connectionController;
        [Inject] private readonly FishNetPlayerDataService _playerDataService;

        private bool _subscribed;
        
        public void Initialize()
        {
            Debug.Log("[FishNetPingService] Initialize called");
            _connectionController.OnStateChanged += OnConnectionStateChanged;
            if (_connectionController.ConnectionState == ConnectionState.Connected)
            {
                Debug.Log("[FishNetPingService] Already connected, subscribing to ping updates");
                _networkManager.TimeManager.OnRoundTripTimeUpdated += OnPingUpdated;
                _subscribed = true;
            }
        }

        public void Dispose()
        {
            Debug.Log("[FishNetPingService] Dispose called");
            _connectionController.OnStateChanged -= OnConnectionStateChanged;
            
            if (_networkManager.TimeManager != null)
                _networkManager.TimeManager.OnRoundTripTimeUpdated -= OnPingUpdated;
                
            _subscribed = false;
        }

        private void OnConnectionStateChanged(ConnectionState state)
        {
            Debug.Log($"[FishNetPingService] Connection state changed to: {state}");
            if (state == ConnectionState.Connected)
            {
                if (!_subscribed)
                {
                    Debug.Log("[FishNetPingService] Subscribing to ping updates");
                    _networkManager.TimeManager.OnRoundTripTimeUpdated += OnPingUpdated;
                    _subscribed = true;
                }
            }
            else
            {
                Debug.Log("[FishNetPingService] Unsubscribing from ping updates");
                if (_networkManager.TimeManager != null)
                    _networkManager.TimeManager.OnRoundTripTimeUpdated -= OnPingUpdated;
                
                _subscribed = false;
            }
        }

        private void OnPingUpdated(long ping)
        {
            Debug.Log($"[FishNetPingService] Ping updated: {ping}ms");
            if (_playerDataService.TryGetLocalClientData(out var playerData))
            {
                _playerDataService.SetDataOnLocalClient(playerData.WithPing(ping));
                Debug.Log($"[FishNetPingService] Local client data updated with ping: {ping}ms");
            }
            else
            {
                Debug.LogWarning("[FishNetPingService] Failed to get local client data");
            }
        }
    }
}