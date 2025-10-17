using MultiplayerSDK.StateMachine;
using UnityEngine;

namespace Game.GameStateMachine.States
{
    public class FinishedStateData
    {
        public int WinnerId;
    }
    
    public class FinishedState : NetworkState<GameStateType, FinishedStateData>
    {
        public override GameStateType Type => GameStateType.Finished;

        protected override void OnEnter(GameStateType prevState, FinishedStateData data)
        {
            Debug.Log($"[FinishedState] Enter finished state, winner: {data.WinnerId}");
        }
    }
}