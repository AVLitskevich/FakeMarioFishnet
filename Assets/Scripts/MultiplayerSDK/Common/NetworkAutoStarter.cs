using MultiplayerSDK.Connection;
using UnityEngine;
using VContainer;

namespace MultiplayerSDK.Common
{
    public enum AutoConnectionMode
    {
        None,
        Client,
        Server
    }
    
    public class NetworkAutoStarter : MonoBehaviour
    {
        [SerializeField] private AutoConnectionMode _dedicatedServerMode;
        [SerializeField] private AutoConnectionMode _webGlMode;
        
        [SerializeField] private AutoConnectionMode _autoConnectionMode;
        [SerializeField] private ConnectionConfig _config;

        [Inject] private readonly IConnectionController _connectionController;

        private void Start()
        {
            if (_config == null) return;
            
#if !UNITY_EDITOR && UNITY_SERVER
            _autoConnectionMode = _dedicatedServerMode;
#elif !UNITY_EDITOR && UNITY_WEBGL
            _autoConnectionMode = _webGlMode;
#endif
            
            if (_autoConnectionMode == AutoConnectionMode.Server)
                _connectionController.StartServer(_config);
            else if (_autoConnectionMode == AutoConnectionMode.Client)
                _connectionController.StartClient(_config);
        }
    }
}