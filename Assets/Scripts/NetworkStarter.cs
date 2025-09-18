using FishNet.Managing;
using FishNet.Transporting.UTP;
using UnityEngine;
using UnityEngine.UI;

namespace DefaultNamespace
{
    public class NetworkStarter : MonoBehaviour
    {
        [SerializeField] private NetworkManager _networkManager;
        [SerializeField] private Button _startServerButton;
        [SerializeField] private Button _connectButton;
        [SerializeField] private GameObject _inGamePanel;
        
        [Header("Connection")]
        [SerializeField] private bool _remoteConnection;
        [SerializeField] private bool _useEncryption;
        [SerializeField] private string _remoteAddress;
        [SerializeField] private string _serverName;
        [SerializeField] private ushort _serverPort;
        [SerializeField] private ushort _wssServerPort;
        [SerializeField] private UnityTransport _transport;

        private void Start()
        {
            _inGamePanel.SetActive(false);
#if UNITY_SERVER && !UNITY_EDITOR
            StartServer();
            return;
#endif

#if UNITY_WEBGL && !UNITY_EDITOR
            _remoteConnection = true;
            _useEncryption = true;
            StartClient();
            return;
#endif
            
            _startServerButton.onClick.AddListener(StartServer);
            _connectButton.onClick.AddListener(StartClient);
        }
        
        private void StartServer()
        {
            Debug.Log($"Starting server at {_serverPort}");
            _transport.SetPort(_serverPort);
            _transport.UseEncryption = false;
            _networkManager.ServerManager.StartConnection();
        }

        private void StartClient()
        {
            _useEncryption &= _remoteConnection;
            string address = _remoteConnection ? _remoteAddress : "127.0.0.1";
            ushort port = _useEncryption ? _wssServerPort : _serverPort;
            
            Debug.Log($"Connecting to {address}:{port}");
            _transport.UseEncryption = _useEncryption;
            _transport.SetPort(port);
            _transport.SetClientAddress(address);
            _transport.SetClientSecrets(_serverName);
            _networkManager.ClientManager.StartConnection();
            
            _startServerButton.gameObject.SetActive(false);
            _connectButton.gameObject.SetActive(false);
            _inGamePanel.SetActive(true);
        }
    }
}