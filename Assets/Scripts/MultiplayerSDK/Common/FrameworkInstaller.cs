using MultiplayerSDK.DI;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace MultiplayerSDK.Common
{
    public class FrameworkInstaller : MonoInstaller
    {
        [SerializeField] private LayerProvider _layerProvider;
        
        public override void Install(IContainerBuilder builder)
        {
            builder.RegisterEntryPoint<ContainerSingletonWrapper>();
            builder.Register<GlobalGameData>(Lifetime.Singleton);
            builder.RegisterInstance(_layerProvider);
        }
    }
}