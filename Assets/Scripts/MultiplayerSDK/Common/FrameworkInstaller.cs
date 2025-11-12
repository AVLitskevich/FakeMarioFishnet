using MultiplayerSDK.DI;
using MultiplayerSDK.WebRequests;
using UnityEngine;
using VContainer;
using VContainer.Unity;

#if !UNITY_WEBGL
using MultiplayerSDK.GameLift;
#endif

namespace MultiplayerSDK.Common
{
    public class FrameworkInstaller : MonoInstaller
    {
        [SerializeField] private LayerProvider _layerProvider;
        [SerializeField] private global::WebBridge _webBridge;
        
        public override void Install(IContainerBuilder builder)
        {
            builder.RegisterEntryPoint<ContainerSingletonWrapper>();
            builder.RegisterEntryPoint<GlobalGameDataController>();
#if !UNITY_WEBGL
            builder.RegisterEntryPoint<GameLiftInitializer>();
#endif
            builder.Register<GlobalGameData>(Lifetime.Singleton);
            
            builder.Register<WebRequester>(Lifetime.Singleton);

            builder.RegisterInstance(_layerProvider);
            builder.RegisterInstance(_webBridge);
        }
    }
}