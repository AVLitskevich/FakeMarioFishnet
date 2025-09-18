using System.Collections.Generic;
using FishNet.Managing;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace DefaultNamespace
{
    public class LayerProvider : NetworkBehaviour
    {
        public readonly SyncVar<bool> CollisionsState = new SyncVar<bool>(new SyncTypeSettings(writePermissions: WritePermission.ServerOnly));
        
        public static LayerProvider Instance;

        [SerializeField] private bool _defaultCollisionsState;
        [SerializeField] private int _layerCount;
        [SerializeField] private NetworkManager _networkManager;
        [SerializeField] private Button _toggleCollisionsButton;
        [SerializeField] private TMP_Text _collisionsStateText;

        private Stack<int> _availableLayerMasks;

        [ServerRpc(RequireOwnership = false)]
        private void RequestToggleCollisions()
        {
            CollisionsState.Value = !CollisionsState.Value;
        }
        
        private void Awake()
        {
            Instance = this;
            
            _availableLayerMasks = new Stack<int>();
            for (int i = 0; i < _layerCount; i++)
            {
                int layerId = i + 1;
                string layerName = $"Player{layerId}";
                int layer = LayerMask.NameToLayer(layerName);
                _availableLayerMasks.Push(layer);
            }
        }

        public override void OnStartNetwork()
        {
            if (IsServerInitialized)
                CollisionsState.Value = _defaultCollisionsState;
            
            if (!IsClientInitialized)
                return;
            
            _toggleCollisionsButton.onClick.AddListener(ToggleCollisions);
            CollisionsState.OnChange += OnCollisionsChanged;
        }

        private void OnCollisionsChanged(bool prev, bool next, bool asServer)
        {
            _collisionsStateText.text = next ? "On" : "Off";
        }

        private void ToggleCollisions()
        {
            if (IsClientInitialized)
                RequestToggleCollisions();
        }

        public bool TryGetLayer(out int layer) => _availableLayerMasks.TryPop(out layer);
        public void ReturnLayer(int layer) => _availableLayerMasks.Push(layer);
    }
}