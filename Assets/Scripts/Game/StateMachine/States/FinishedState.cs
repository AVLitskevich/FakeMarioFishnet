using Game.GUI;
using MultiplayerSDK.FishNetAdapter;
using MultiplayerSDK.StateMachine;
using UnityEngine;
using VContainer;

namespace Game.StateMachine.States
{
    public enum FinishReason
    {
        ReachedFinish,
        TechnicalWin,
    }
    
    public class FinishedStateData
    {
        public int WinnerId;
        public FinishReason FinishReason;

        public FinishedStateData(int winnerId, FinishReason finishReason)
        {
            WinnerId = winnerId;
            FinishReason = finishReason;
        }
    }
    
    public class FinishedState : NetworkState<GameStateType, FinishedStateData>
    {
        public override GameStateType Type => GameStateType.Finished;

        [Inject] private readonly GameUi _gameUi;
        [Inject] private readonly GameConfig _gameConfig;
        [Inject] private readonly PlayerSpawner _playerSpawner;
        [Inject] private readonly FishNetPlayerDataService _playerDataService;

        private float _timer;

        protected override void OnEnter(GameStateType prevState, FinishedStateData data)
        {
            base.OnEnter(prevState, data);
            _timer = _gameConfig.WaitAfterFinishSeconds;

            if (IsClient)
                InitUi(data);
            
            if (IsServer)
                _playerSpawner.DespawnAllPlayers();
        }

        public override void OnExit(GameStateType nextState)
        {
            base.OnExit(nextState);
            if (IsClient)
                _gameUi.FinishedPanel.gameObject.SetActive(false);
        }

        public override void Update()
        {
            base.Update();
            _timer -= Time.deltaTime;

            if (IsServer && _timer <= 0)
                StateMachine.SetStateServer(GameStateType.WaitForPlayers);
        }

        private void InitUi(FinishedStateData data)
        {
            _gameUi.FinishedPanel.gameObject.SetActive(true);

            if (!_playerDataService.TryGetData(data.WinnerId, out var playerData))
            {
                Debug.LogError($"[FinishedState] Can't get winner player data {data.WinnerId}");
                return;
            }
            
            _gameUi.FinishedPanel.SetWinner(playerData.Nickname, data.FinishReason);
            _gameUi.FinishedPanel.SetUserState(_playerDataService.LocalPlayerId == data.WinnerId);
        }
    }
}