using UnityEngine;

namespace Networking
{
    [CreateAssetMenu(fileName = "ConnectionHostData", menuName = "Configs/Host Data")]
    public class ConnectionHostData : ScriptableObject
    {
        [SerializeField] public string IpAddress;
        [SerializeField] public bool UseEncryption;
        [SerializeField] public string ServerName;
        [SerializeField] public ushort ServerPort;
        [SerializeField] public ushort EncryptionConnectPort;
    }
}