using FishNet.Managing;
using FishNet.Transporting.UTP;
using Game.GameStateMachine.States;
using MultiplayerSDK.DI;
using MultiplayerSDK.FishNetAdapter.PingService;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace MultiplayerSDK.FishNetAdapter
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