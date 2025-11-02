using MultiplayerSDK.Connection;
using MultiplayerSDK.FishNetAdapter;
using MultiplayerSDK.WebBridge;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace MultiplayerSDK.Common
{
    public class GlobalGameDataController : IInitializable
    {
        [Inject] private readonly global::WebBridge _webBridge;
        [Inject] private readonly GlobalGameData _globalGameData;
        [Inject] private readonly IConnectionController _connectionController;
        [Inject] private readonly FishNetPlayerDataService _playerDataService;
        
        public void Initialize()
        {
            _webBridge.OnPayloadReceived += ProcessPayload;
            _webBridge.TriggerLoaded();
            _connectionController.OnStateChanged += OnConnectionStateChanged;
        }

        private void ProcessPayload(WebPayload payload)
        {
            Debug.Log($"Received payload: matchId: {payload.MatchId}, playerId: {payload.PlayerId}, nickname: {payload.Nickname}");
            if (_playerDataService.TryGetLocalClientData(out var data))
            {
                _playerDataService.SetDataOnLocalClient(data
                    .WithMatchId(payload.MatchId)
                    .WithNickname(payload.Nickname)
                    .WithUserId(payload.PlayerId));
            }
            else
            {
                _globalGameData.MatchId = payload.MatchId;
                _globalGameData.Nickname = payload.Nickname;
                _globalGameData.PlayerId = payload.PlayerId;
            }
        }

        private void OnConnectionStateChanged(ConnectionState connectionState)
        {
            if (connectionState == ConnectionState.Connected)
                _webBridge.TriggerConnected();
        }
    }
}