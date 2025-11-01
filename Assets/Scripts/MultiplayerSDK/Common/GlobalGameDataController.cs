using MultiplayerSDK.Connection;
using VContainer;
using VContainer.Unity;

namespace MultiplayerSDK.Common
{
    public class GlobalGameDataController : IInitializable
    {
        [Inject] private readonly WebBridge _webBridge;
        [Inject] private readonly IConnectionController _connectionController;
        
        public void Initialize()
        {
            _webBridge.TriggerLoaded();
            _connectionController.OnStateChanged += OnConnectionStateChanged;
        }

        private void OnConnectionStateChanged(ConnectionState connectionState)
        {
            if (connectionState == ConnectionState.Connected)
                _webBridge.TriggerConnected();
        }
    }
}