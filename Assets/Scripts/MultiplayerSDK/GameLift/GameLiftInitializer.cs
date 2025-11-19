
#if UNITY_SERVER
using System.Collections.Generic;
using Aws.GameLift.Server;
using Aws.GameLift.Server.Model;
#endif
using MultiplayerSDK.Connection;
using UnityEngine;

namespace MultiplayerSDK.GameLift
{
    public class GameLiftInitializer : MonoBehaviour
    {
        [SerializeField] private ConnectionConfig _config;
        
#if UNITY_SERVER
        private void Start()
        {
            Debug.Log("[GameLiftInitializer] Server connected, initializing game lift services");
            InitializeGameLiftServer();
        }

        private void InitializeGameLiftServer()
        {
            var initSdkResult = GameLiftServerAPI.InitSDK();
            if (!initSdkResult.Success)
            {
                Debug.LogError($"[GameLiftInitializer] Error initializing game lift: {initSdkResult.Error}");
                return;
            }

            Debug.Log("[GameLiftInitializer] Sdk initialized");
            var logFileNames = new List<string>
            {
                "Local/game/logs/serverLog.txt"
            };
            var processParameters = new ProcessParameters(OnStartSession,
                OnUpdateSession,
                OnProcessTerminate,
                OnHealthCheck,
                _config.ServerListenPort,
                new LogParameters(logFileNames));
            
            var processReadyResult = GameLiftServerAPI.ProcessReady(processParameters);
            if (!processReadyResult.Success)
            {
                Debug.LogError($"[GameLiftInitializer] Error setting process ready: {processReadyResult.Error}");
                return;
            }

            Debug.Log("[GameLiftInitializer] Server process is ready!");
        }

        private void OnStartSession(GameSession gameSession)
        {
            GameLiftServerAPI.ActivateGameSession();
            GameLiftServerAPI.UpdatePlayerSessionCreationPolicy(PlayerSessionCreationPolicy.ACCEPT_ALL);
        }

        private void OnUpdateSession(UpdateGameSession updateGameSession)
        {
        }

        private void OnProcessTerminate()
        {
            GameLiftServerAPI.ProcessEnding();
        }

        private bool OnHealthCheck()
        {
            return true;
        }
        
        private void OnDestroy()
        {
            GameLiftServerAPI.Destroy();
        }
#endif
    }
}