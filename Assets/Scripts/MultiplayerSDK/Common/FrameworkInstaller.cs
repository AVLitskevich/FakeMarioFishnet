using MultiplayerSDK.DI;
using VContainer;
using VContainer.Unity;

namespace MultiplayerSDK.Common
{
    public class FrameworkInstaller : MonoInstaller
    {
        public override void Install(IContainerBuilder builder)
        {
            builder.RegisterEntryPoint<ContainerSingletonWrapper>();
            builder.Register<GlobalGameData>(Lifetime.Singleton);
        }
    }
}