using FishNet.Object;
using UnityEngine;

namespace DefaultNamespace
{
    public class FinishLine : NetworkBehaviour
    {
        [SerializeField] private float _detectionRange = 1f;

        private void Update()
        {
            if (!IsServerInitialized)
                return;

            var gameManager = FindAnyObjectByType<RaceManager>();
            if (gameManager == null || gameManager.CurrentState != GameState.Race)
                return;
            
            var players = FindObjectsByType<PlayerController>(FindObjectsSortMode.None);
            foreach (var player in players)
            {
                float distance = Vector3.Distance(player.transform.position, transform.position);
                if (distance <= _detectionRange)
                {
                    gameManager.PlayerFinished(player);
                }
            }
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(transform.position, _detectionRange);
        }
    }
}