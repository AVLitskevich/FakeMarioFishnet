using FishNet.Managing;
using MultiplayerSDK.Connection;
using UnityEngine;
using UnityEngine.UI;
using VContainer;

namespace DefaultNamespace
{
    public class NetworkStarter : MonoBehaviour
    {
        [SerializeField] private NetworkManager _networkManager;
        [SerializeField] private Button _startServerButton;
        [SerializeField] private Button _connectButton;
        [SerializeField] private GameObject _inGamePanel;
        [SerializeField] private ConnectionConfig _config;

        [Inject] private readonly IConnectionController _connectionController;

        private void Start()
        {
            _inGamePanel.SetActive(false);
            _startServerButton.onClick.AddListener(StartServer);
            _connectButton.onClick.AddListener(StartClient);
        }
        
        private void StartServer()
        {
            _connectionController.StartServer(_config);
        }

        private void StartClient()
        {
            _connectionController.StartClient(_config);
            _startServerButton.gameObject.SetActive(false);
            _connectButton.gameObject.SetActive(false);
            _inGamePanel.SetActive(true);
        }
    }
}