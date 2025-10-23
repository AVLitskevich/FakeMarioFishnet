using TMPro;
using UnityEngine;

namespace Game.GUI.Debug
{
    public class PlayerPingItem : MonoBehaviour
    {
        public string Username { get; private set; }
        
        [SerializeField] private TMP_Text _usernameText;
        [SerializeField] private TMP_Text _pingText;

        private int _lastShownPing = -1;
        
        public void SetUsername(string username)
        {
            _usernameText.text = username;
            Username = username;
        }
        
        public void SetPing(int ping)
        {
            if (_lastShownPing == ping)
                return;
            
            _lastShownPing = ping;
            _pingText.text = $"{ping}";
        }
    }
}