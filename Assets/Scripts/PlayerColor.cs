using FishNet.Object.Synchronizing;
using UnityEngine;

namespace DefaultNamespace
{
    public class PlayerColor
    {
        [SerializeField] private int _paletteIndex;
        [SerializeField] private Color _newColor = new Color(0.2f, 0.6f, 1f, 1f);

        private ColorSwap_HeroKnight _colorSwap;
        
        [SyncVar(OnChange = nameof(OnColorChanged))]
        private Color _playerColor
    }
}