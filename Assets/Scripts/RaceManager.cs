using System;
using System.Collections;
using System.Collections.Generic;
using FishNet;
using FishNet.Connection;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using UnityEngine;

namespace DefaultNamespace
{
    public enum GameState
    {
        Waiting,
        Countdown,
        Race,
        Finished
    }
    
    public class RaceManager : NetworkBehaviour
    {
        public static RaceManager Instance { get; private set; }

        public event Action OnStateChanged;
        
        [SerializeField] private float _countDownTime = 3f;
        [SerializeField] private float _resetTime = 5f;
        [SerializeField] private Vector3 _startPosition = Vector3.zero;
        
        private readonly SyncVar<float> _raceStartTime = new SyncVar<float>(new SyncTypeSettings(writePermissions: WritePermission.ServerOnly));
        private readonly SyncVar<GameState> _state = new SyncVar<GameState>(GameState.Waiting, new SyncTypeSettings(writePermissions: WritePermission.ServerOnly));
        private readonly SyncVar<int> _countdown = new SyncVar<int>(new SyncTypeSettings(writePermissions: WritePermission.ServerOnly));
        private readonly SyncDictionary<int, float> _playerFinishTimes = new(new SyncTypeSettings(writePermissions: WritePermission.ServerOnly));
        private readonly List<NetworkConnection> _finishedPlayers = new();

        public GameState CurrentState => _state.Value;

        private void Awake()
        {
            Instance = this;
        }

        private void Update()
        {
            if (_state.Value != GameState.Waiting && NetworkManager.ServerManager.Clients.Count == 0)
                ResetRace();
        }

        public override void OnStartNetwork()
        {
            _state.OnChange += OnGameStateChanged;
        }

        private void OnGameStateChanged(GameState prev, GameState next, bool asServer)
        {
            Debug.Log($"Race state changed from {prev} to {next}");
            OnStateChanged?.Invoke();
        }

        [ServerRpc(RequireOwnership = false)]
        public void RequestStartRace()
        {
            if (_state.Value != GameState.Waiting)
                return;

            StartRace();
        }

        [Server]
        private void StartRace()
        {
            if (_state.Value != GameState.Waiting)
                return;

            // Reset race data
            _finishedPlayers.Clear();
            _playerFinishTimes.Clear();

            // Teleport all players to start
            TeleportAllPlayersToStart();

            // Start countdown
            _state.Value = GameState.Countdown;
            StartCoroutine(CountdownCoroutine());
        }

        [Server]
        private void TeleportAllPlayersToStart()
        {
            Debug.Log("Teleporting players to start");
            var players = FindObjectsByType<PlayerController>(FindObjectsSortMode.None);
            foreach (var player in players)
            {
                // Reset player position and velocity
                var rb = player.GetComponent<Rigidbody2D>();
                if (rb != null)
                {
                    rb.position = _startPosition;
                    rb.linearVelocity = Vector2.zero;
                }
                else
                {
                    player.transform.position = _startPosition;
                }
            }
        }

        [Server]
        private IEnumerator CountdownCoroutine()
        {
            for (int i = (int)_countDownTime; i > 0; i--)
            {
                _countdown.Value = i;
                InstanceFinder.ServerManager.Broadcast(new CountdownMessage { CountdownValue = i });
                yield return new WaitForSeconds(1f);
            }

            // Race starts
            _state.Value = GameState.Race;
            _raceStartTime.Value = InstanceFinder.TimeManager.Tick * (float)InstanceFinder.TimeManager.TickDelta;
            InstanceFinder.ServerManager.Broadcast(new RaceStartedMessage { StartTime = _raceStartTime.Value });
        }

        [Server]
        public void PlayerFinished(PlayerController player)
        {
            if (_state.Value != GameState.Race)
                return;
            
            Debug.Log($"Player {player.OwnerId} finished");
            var owner = player.Owner;
            if (_finishedPlayers.Contains(owner))
                return; // Player already finished

            float currentTime = InstanceFinder.TimeManager.Tick * (float)InstanceFinder.TimeManager.TickDelta;
            float finishTime = currentTime - _raceStartTime.Value;
            
            _finishedPlayers.Add(owner);
            _playerFinishTimes[owner.ClientId] = finishTime;

            // Check if this is the first player (winner)
            if (_finishedPlayers.Count != 1) return;
            
            _state.Value = GameState.Finished;
            InstanceFinder.ServerManager.Broadcast(new RaceEndedMessage
            {
                Winner = owner,
                WinTime = finishTime
            });
            
            StartCoroutine(ResetCoroutine());
        }

        [Server]
        private IEnumerator ResetCoroutine()
        {
            Debug.Log("Start countdown");
            for (int i = (int)_countDownTime; i > 0; i--)
            {
                _countdown.Value = i;
                InstanceFinder.ServerManager.Broadcast(new CountdownMessage { CountdownValue = i });
                yield return new WaitForSeconds(1f);
            }

            ResetRace();
        }

        [Server]
        private void ResetRace()
        {
            Debug.Log("Set state to waiting");
            _state.Value = GameState.Waiting;
            _finishedPlayers.Clear();
            _playerFinishTimes.Clear();
            _raceStartTime.Value = 0f;
            _countdown.Value = 0;
            Debug.Log("Send reset message");
            InstanceFinder.ServerManager.Broadcast(new RaceResetMessage());
        }
    }
}