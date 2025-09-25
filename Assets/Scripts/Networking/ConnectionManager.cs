using UnityEngine;
using VContainer;

namespace Networking
{
    public class ConnectionManager
    {
        [Inject] private readonly IObjectResolver _objectResolver;

        private IConnectionHandler _currentHandler;

        public void StartServer()
        {
            if (_currentHandler != null)
            {
                Debug.Log("Trying to start server when already connected");
                return;
            }
            
            _currentHandler = new ServerConnectionHandler();
            _objectResolver.Inject(_currentHandler);
            _currentHandler.Connect();
        }

        public void StartClient()
        {
            if (_currentHandler != null)
            {
                Debug.Log("Trying to start client when already connected");
                return;
            }
            
            _currentHandler = new ClientConnectionHandler();
            _objectResolver.Inject(_currentHandler);
            _currentHandler.Connect();
        }

        public void Disconnect()
        {
            if (_currentHandler == null)
            {
                Debug.Log("Trying to disconnect when not connected");
                return;
            }
            
            _currentHandler.Disconnect();
            _currentHandler = null;
        }
    }
}