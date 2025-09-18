using System.Collections;
using FishNet;
using FishNet.Connection;
using FishNet.Transporting;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace DefaultNamespace
{
    public class RaceUI : MonoBehaviour
    {
        [SerializeField] private Button _startRaceButton;
        [SerializeField] private TextMeshProUGUI _countdownText;
        [SerializeField] private TextMeshProUGUI _raceStatusText;
        [SerializeField] private TextMeshProUGUI _timerText;
        [SerializeField] private GameObject _winnerPanel;
        [SerializeField] private TextMeshProUGUI _winnerText;
        [SerializeField] private RaceManager _raceManager;
        
        private float _raceStartTime;
        private bool _raceActive;

        private void Start()
        {
            _startRaceButton.onClick.AddListener(StartRace);
            _winnerPanel.SetActive(false);
            _countdownText.gameObject.SetActive(false);

            _raceManager.OnStateChanged += UpdateUI;
            UpdateUI();
            RegisterBroadcasts();
        }

        private void OnDestroy()
        {
            _raceManager.OnStateChanged -= UpdateUI;
            UnregisterBroadcasts();
        }

        private void RegisterBroadcasts()
        {
            if (InstanceFinder.ClientManager != null)
            {
                InstanceFinder.ClientManager.RegisterBroadcast<CountdownMessage>(OnCountdownMessage);
                InstanceFinder.ClientManager.RegisterBroadcast<RaceStartedMessage>(OnRaceStartedMessage);
                InstanceFinder.ClientManager.RegisterBroadcast<RaceEndedMessage>(OnRaceEndedMessage);
                InstanceFinder.ClientManager.RegisterBroadcast<RaceResetMessage>(OnRaceResetMessage);
            }
        }

        private void UnregisterBroadcasts()
        {
            if (InstanceFinder.ClientManager != null)
            {
                InstanceFinder.ClientManager.UnregisterBroadcast<CountdownMessage>(OnCountdownMessage);
                InstanceFinder.ClientManager.UnregisterBroadcast<RaceStartedMessage>(OnRaceStartedMessage);
                InstanceFinder.ClientManager.UnregisterBroadcast<RaceEndedMessage>(OnRaceEndedMessage);
                InstanceFinder.ClientManager.UnregisterBroadcast<RaceResetMessage>(OnRaceResetMessage);
            }
        }

        private void Update()
        {
            if (_raceActive)
            {
                float currentTime = Time.time - _raceStartTime;
                _timerText.text = $"Time: {currentTime:F2}s";
            }
        }

        private void StartRace()
        {
            _raceManager.RequestStartRace();
        }

        private void OnCountdownMessage(CountdownMessage message, Channel channel)
        {
            _countdownText.gameObject.SetActive(true);
            _countdownText.text = message.CountdownValue.ToString();
            StartCoroutine(HideCountdownAfterDelay(1f));
            
            if (_raceManager.CurrentState is GameState.Waiting or GameState.Countdown)
                _raceStatusText.text = $"Race starting in {message.CountdownValue}...";
            else
                _raceStatusText.text = $"Race ends in {message.CountdownValue}...";
        }

        private void OnRaceStartedMessage(RaceStartedMessage message, Channel channel)
        {
            _raceStartTime = Time.time;
            _raceActive = true;

            _countdownText.text = "GO!";
            StartCoroutine(HideCountdownAfterDelay(1f));

            _raceStatusText.text = "Race in progress!";

            UpdateUI();
        }

        private void OnRaceEndedMessage(RaceEndedMessage message, Channel channel)
        {
            _raceActive = false;
            string winnerName = GetPlayerName(message.Winner);

            _winnerPanel.SetActive(true);
            if (InstanceFinder.ClientManager.Connection.ClientId == message.Winner.ClientId)
                _winnerText.text = $"You won! Time: {message.WinTime:F2}s";
            else
                _winnerText.text = $"Winner: {winnerName}\nTime: {message.WinTime:F2}s";

            _raceStatusText.text = $"Race finished! Winner: {winnerName} ({message.WinTime:F2}s)";
            UpdateUI();
        }

        private void OnRaceResetMessage(RaceResetMessage arg1, Channel arg2)
        {
            Debug.Log("Got race reset message");
            _countdownText.gameObject.SetActive(false);
            UpdateUI();
        }

        private IEnumerator HideCountdownAfterDelay(float delay)
        {
            yield return new WaitForSeconds(delay);
            _countdownText.gameObject.SetActive(false);
        }

        private void UpdateUI()
        {
            bool canStartRace = _raceManager != null && _raceManager.CurrentState == GameState.Waiting;
            _startRaceButton.interactable = canStartRace;
            Debug.Log($"Update ui, can start race: {canStartRace}");

            if (!_raceActive)
                _timerText.text = "Time: 0.00s";
        }

        private string GetPlayerName(NetworkConnection connection)
        {
            // Simple player name - could be enhanced with actual player names
            return $"Player {connection.ClientId}";
        }
    }
}