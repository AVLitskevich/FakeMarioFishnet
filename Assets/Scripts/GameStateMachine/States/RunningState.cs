using MultiplayerSDK.StateMachine;

namespace GameStateMachine.States
{
    public class RunningState : NetworkState<GameStateType>
    {
        public override GameStateType Type => GameStateType.Running;
    }
}