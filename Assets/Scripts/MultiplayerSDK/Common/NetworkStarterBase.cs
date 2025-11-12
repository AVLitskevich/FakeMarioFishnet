using System.Collections;
using MultiplayerSDK.Connection;
using UnityEngine;
using VContainer;

namespace MultiplayerSDK.Common
{
    public enum ConnectionMode
    {
        None,
        Client,
        Server
    }
    
    public class NetworkStarterBase : MonoBehaviour
    {
        [Header("Platform auto connection settings")]
        [SerializeField] private ConnectionMode _dedicatedServerMode;
        [SerializeField] private ConnectionMode _webGlMode;
        
        [Header("Default connection settings")]
        [SerializeField] private ConnectionMode _connectionMode;
        [SerializeField] private ConnectionConfig _config;

        [Inject] private readonly IConnectionController _connectionController;
        
        private void Start()
        {
            if (_config == null)
            {
                Debug.LogError("[NetworkStarterBase] Config is null");
                return;
            }
            
#if !UNITY_EDITOR && UNITY_SERVER
            _connectionMode = _dedicatedServerMode;
#elif !UNITY_EDITOR && UNITY_WEBGL
            _connectionMode = _webGlMode;
#endif

            if (_connectionMode != ConnectionMode.None)
                StartCoroutine(WaitAndStart());
            else
                InitManualConnection();
        }
        
        protected virtual void InitManualConnection() { }
        protected virtual void OnConnectionStarted(ConnectionMode mode) { }

        private IEnumerator WaitAndStart()
        {
            yield return null;

            if (_connectionMode == ConnectionMode.Server)
                StartServer();
            else if (_connectionMode == ConnectionMode.Client)
                StartClient();
        }

        protected void StartServer()
        {
            _connectionController.StartServer(_config);
            OnConnectionStarted(ConnectionMode.Server);
        }

        protected void StartClient()
        {
            _connectionController.StartClient(_config);
            OnConnectionStarted(ConnectionMode.Client);
        }
    }
}