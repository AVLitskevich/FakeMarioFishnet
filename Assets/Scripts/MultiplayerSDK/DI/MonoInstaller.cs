using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace MultiplayerSDK.DI
{
    public abstract class MonoInstaller : MonoBehaviour, IInstaller
    {
        public abstract void Install(IContainerBuilder builder);
    }
}