using FishNet.Object;
using VContainer;

namespace MultiplayerSDK.Common
{
    public class PlayerLayerController : NetworkBehaviour
    {
        [Inject] private readonly LayerProvider _layerProvider;
        
        private int _defaultLayer;
        private int? _layer;

        private void Awake()
        {
            _defaultLayer = gameObject.layer;
        }

        public override void OnStartNetwork()
        {
            base.OnStartNetwork();
            this.InjectToMe();
            
            _layerProvider.CollisionsState.OnChange += OnCollisionStateChanged;
            if (!_layerProvider.CollisionsState.Value)
                GetLayer();
        }

        public override void OnStopNetwork()
        {
            base.OnStopNetwork();
            _layerProvider.CollisionsState.OnChange -= OnCollisionStateChanged;
            ReturnLayer();
        }

        private void OnCollisionStateChanged(bool prev, bool next, bool asServer)
        {
            ReturnLayer();
            if (!next)
                GetLayer();
        }
        
        private void GetLayer()
        {
            if (_layerProvider.TryGetLayer(out int layer))
            {
                gameObject.SetLayerWithChildren(layer);
                _layer = layer;
            }
        }
        
        private void ReturnLayer()
        {
            if (_layer.HasValue)
            {
                _layerProvider.ReturnLayer(_layer.Value);
                _layer = null;
                gameObject.SetLayerWithChildren(_defaultLayer);
            }
        }
    }
}