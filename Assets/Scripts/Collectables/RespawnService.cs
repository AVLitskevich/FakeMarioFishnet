using System.Collections.Generic;
using FishNet.Object;
using VContainer;

namespace DefaultNamespace.Collectables
{
    public class RespawnService : NetworkBehaviour
    {
        [Inject] private IReadOnlyList<ISpawnService> _spawnServices;
        
        public void Initialize(IReadOnlyList<ISpawnService> spawnServices)
        {
            _spawnServices = spawnServices;
        }

        [Server]
        public void RespawnAll()
        {
            if (!IsServerInitialized) return;

            foreach (var spawner in _spawnServices)
            {
                spawner.Respawn();
            }
        }
    }
}