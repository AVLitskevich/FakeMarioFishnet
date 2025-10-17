using System.Linq;
using Game.GameStateMachine.States;
using MultiplayerSDK.StateMachine;
using UnityEngine;

namespace Game.GameStateMachine
{
    public class GameStateMachine : NetworkStateMachine<GameStateType>
    {
        protected override void SetInitialState()
        {
            if (!IsServerInitialized)
                return;
            
            SetInitialStateInternal(GameStateType.WaitForPlayers);
        }

        private void Update()
        {
            if (!IsServerInitialized)
                return;
            
            if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                SetStateServer(GameStateType.WaitForPlayers);
            }
            else if (Input.GetKeyDown(KeyCode.Alpha2))
            {
                SetStateServer(GameStateType.Running);
            }
            else if (Input.GetKeyDown(KeyCode.Alpha3))
            {
                int winnerId = NetworkManager.ClientManager.Clients.Count > 0 ? NetworkManager.ClientManager.Clients.First().Key : -1;
                SetStateServer(GameStateType.Finished, new FinishedStateData { WinnerId = winnerId });
            }
        }
    }
}