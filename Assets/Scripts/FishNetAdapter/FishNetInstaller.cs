using MultiplayerSDK.DI;
using FishNet.Managing;
using FishNet.Transporting.UTP;
using FishNetAdapter.PingService;
using GameStateMachine.States;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace FishNetAdapter
{
    public class FishNetInstaller : MonoInstaller
    {
        [SerializeField] private NetworkManager _networkManager;
        [SerializeField] private UnityTransport _unityTransport;
        [SerializeField] private FishNetConnectionController _connectionController;
        [SerializeField] private FishNetPlayerDataService _playerDataService;
        
        public override void Install(IContainerBuilder builder)
        {
            builder.RegisterInstance(_networkManager);
            builder.RegisterInstance(_unityTransport);
            builder.RegisterInstance(_connectionController).AsSelf().AsImplementedInterfaces();
            builder.RegisterInstance(_playerDataService);

            builder.RegisterEntryPoint<FishNetTransportAdapter>().AsSelf();
            builder.RegisterEntryPoint<FishNetPingService>();

            builder.Register<WaitForPlayersState>(Lifetime.Singleton).AsImplementedInterfaces();
            builder.Register<RunningState>(Lifetime.Singleton).AsImplementedInterfaces();
            builder.Register<FinishedState>(Lifetime.Singleton).AsImplementedInterfaces();
        }
    }
}