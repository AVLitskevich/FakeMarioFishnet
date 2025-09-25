using FishNet.Managing;
using Networking;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace DefaultNamespace
{
    public class ApplicationStarter : LifetimeScope
    {
        [SerializeField] private NetworkManager _networkManager;
        [SerializeField] private ConnectionConfig _connectionConfig;
        [SerializeField] private RaceManager _raceManager;
        
        protected override void Configure(IContainerBuilder builder)
        {
            builder.RegisterInstance(_networkManager);
            builder.RegisterInstance(_connectionConfig);
            builder.RegisterInstance(_raceManager);

            builder.Register<ConnectionManager>(Lifetime.Singleton);
            builder.RegisterEntryPoint<LayerProvider>();
            builder.RegisterEntryPoint<NetworkObjectsSpawnHandler>();
        }
    }
}