using System;
using FishNet.Managing;
using FishNet.Object;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace DefaultNamespace
{
    public class NetworkObjectsSpawnHandler : IInitializable, IDisposable
    {
        [Inject] private readonly NetworkManager _networkManager;
        [Inject] private readonly IObjectResolver _objectResolver;
        
        public void Initialize()
        {
            Debug.Log("Subscribe to object added");
            _networkManager.ClientManager.Connection.OnObjectAdded += OnObjectAdded;
        }

        private void OnObjectAdded(NetworkObject networkObject)
        {
            Debug.Log($"Object added: {networkObject.gameObject.name}, owner: {networkObject.OwnerId}");
            if (!networkObject.gameObject.TryGetComponent(out PlayerController playerController))
                return;
            
            Debug.Log("Found player controller");
            _objectResolver.Inject(playerController);
        }

        public void Dispose()
        {
            
        }
    }
}