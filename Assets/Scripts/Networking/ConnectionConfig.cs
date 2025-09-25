using UnityEngine;

namespace Networking
{
    [CreateAssetMenu(fileName = "ConnectionConfig", menuName = "Configs/Connection")]
    public class ConnectionConfig : ScriptableObject
    {
        [SerializeField] public ConnectionHostData ConnectToHost;
        [SerializeField] public ConnectionHostData ServerHost;
    }
}