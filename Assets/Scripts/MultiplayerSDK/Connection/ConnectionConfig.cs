using UnityEngine;

namespace MultiplayerSDK.Connection
{
    [CreateAssetMenu(fileName = "Connection Config", menuName = "Configs/Connection")]
    public class ConnectionConfig : ScriptableObject
    {
        [SerializeField] public string ServerAddress;
        [SerializeField] public ushort ServerListenPort;
        [SerializeField] public ushort WssConnectPort;
        [SerializeField] public string WssServerName;
        [SerializeField] public bool UseEncryption;

        [SerializeField] public float ReconnectDelay;
        [SerializeField] public int ReconnectAttempts;
    }
}