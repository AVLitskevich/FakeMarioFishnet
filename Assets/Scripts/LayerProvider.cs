using System.Collections.Generic;
using FishNet.Managing;
using FishNet.Transporting.UTP;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace DefaultNamespace
{
    public class LayerProvider : IInitializable
    {
        [Inject] private readonly NetworkManager _networkManager;

        private Stack<int> _availableLayerMasks;

        public void Initialize()
        {
            var maxClients = _networkManager.TransportManager.GetTransport<UnityTransport>().GetMaximumClients();
            
            _availableLayerMasks = new Stack<int>();
            for (int i = 0; i < maxClients; i++)
            {
                int layerId = i + 1;
                string layerName = $"Player{layerId}";
                int layer = LayerMask.NameToLayer(layerName);
                _availableLayerMasks.Push(layer);
            }
        }

        public bool TryGetLayer(out int layer) => _availableLayerMasks.TryPop(out layer);

        public void ReturnLayer(int layer) => _availableLayerMasks.Push(layer);
    }
}