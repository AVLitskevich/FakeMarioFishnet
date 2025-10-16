using MultiplayerSDK.StateMachine;

namespace GameStateMachine.States
{
    public class WaitForPlayersState : NetworkState<GameStateType>
    {
        public override GameStateType Type => GameStateType.WaitForPlayers;
        
        
    }
}