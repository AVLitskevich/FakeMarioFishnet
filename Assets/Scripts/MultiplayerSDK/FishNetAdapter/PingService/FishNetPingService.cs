using System;
using FishNet.Managing;
using MultiplayerSDK.Connection;
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
            _connectionController.OnStateChanged += OnConnectionStateChanged;
            if (_connectionController.ConnectionState == ConnectionState.Connected)
            {
                _networkManager.TimeManager.OnRoundTripTimeUpdated += OnPingUpdated;
                _subscribed = true;
            }
        }

        public void Dispose()
        {
            _connectionController.OnStateChanged -= OnConnectionStateChanged;
            
            if (_networkManager.TimeManager != null)
                _networkManager.TimeManager.OnRoundTripTimeUpdated -= OnPingUpdated;
                
            _subscribed = false;
        }

        private void OnConnectionStateChanged(ConnectionState state)
        {
            if (state == ConnectionState.Connected)
            {
                if (!_subscribed)
                {
                    _networkManager.TimeManager.OnRoundTripTimeUpdated += OnPingUpdated;
                    _subscribed = true;
                }
            }
            else
            {
                if (_networkManager.TimeManager != null)
                    _networkManager.TimeManager.OnRoundTripTimeUpdated -= OnPingUpdated;
                
                _subscribed = false;
            }
        }

        private void OnPingUpdated(long ping)
        {
            if (_playerDataService.TryGetLocalClientData(out var playerData))
                _playerDataService.SetDataOnLocalClient(playerData.WithPing(ping));
        }
    }
}