using MultiplayerSDK.StateMachine;

namespace Game.GameStateMachine.States
{
    public class RunningState : NetworkState<GameStateType>
    {
        public override GameStateType Type => GameStateType.Running;
    }
}