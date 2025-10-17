using MultiplayerSDK.StateMachine;

namespace Game.GameStateMachine.States
{
    public class WaitForPlayersState : NetworkState<GameStateType>
    {
        public override GameStateType Type => GameStateType.WaitForPlayers;
        
        
    }
}