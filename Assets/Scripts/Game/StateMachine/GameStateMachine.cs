using Game.StateMachine.States;
using MultiplayerSDK.Common;
using MultiplayerSDK.FishNetAdapter;
using MultiplayerSDK.StateMachine;
using VContainer;

namespace Game.StateMachine
{
    public class GameStateMachine : NetworkStateMachine<GameStateType>
    {
        [Inject] private readonly FishNetPlayerDataService _playerDataService;
        
        public override void OnStartNetwork()
        {
            base.OnStartNetwork();
            _playerDataService.OnPlayerRemoved += OnPlayerRemoved;
        }

        public override void OnStopNetwork()
        {
            base.OnStopNetwork();
            _playerDataService.OnPlayerRemoved -= OnPlayerRemoved;
        }

        private void OnPlayerRemoved(int clientId, PlayerData playerData)
        {
            if (!IsServerInitialized)
                return;
                
            int inGamePlayers = 0;
            int winnerId = -1;
            foreach (var entry in _playerDataService.PlayerData)
            {
                if (entry.Value.InGame)
                {
                    winnerId = entry.Key;
                    inGamePlayers++;
                }
            }
            
            if (inGamePlayers == 0)
                SetStateServer(GameStateType.WaitForPlayers);
            else if (inGamePlayers == 1 && CurrentState != GameStateType.Finished)
                SetStateServer(GameStateType.Finished, new FinishedStateData(winnerId, FinishReason.TechnicalWin));
        }

        protected override void SetInitialState()
        {
            if (!IsServerInitialized)
                return;
            
            SetInitialStateInternal(GameStateType.WaitForPlayers);
        }
    }
}