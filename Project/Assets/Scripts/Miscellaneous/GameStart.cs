using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(BoxCollider))]
public class GameStart : MonoBehaviour
{
    [Header("Objects")]
    [SerializeField] private GameObject _timerObject;

    [Header("Settings")]
    [Tooltip("How long it will take before the game starts after someone stand inside the collider")]
    [SerializeField] private float _timeBeforeStart = 10.5f;

    [Header("Events")]
    public UnityEvent StartTimerEvent;
    public UnityEvent StopTimerEvent;

    [Header("View variables")]
    [SerializeField] private float _currentTimeBeforeStart;

    // Timer
    private bool _startCountdown = false;
    private TextMeshProUGUI _textComponent;

    // Other
    public event Action<GameStart> GameStartedEvent;
    private PlayerNeed _playerNeedComponent;
    private BoxCollider _collider;

    // Start
    // -----
    private void Start()
    {
        // Get text component
        Debug.Assert(_timerObject != null, "GameStart needs timerObject to work");
        _textComponent = _timerObject.GetComponent<TextMeshProUGUI>();
        _timerObject.SetActive(false);

        // Set timer to max
        _currentTimeBeforeStart = _timeBeforeStart;

        // Get component
        _playerNeedComponent = gameObject.GetComponent<PlayerNeed>();
        Debug.Assert(_playerNeedComponent != null);

        // Get collider
        _collider = gameObject.GetComponent<BoxCollider>();
    }

    // Update
    // ------
    private void Update()
    {
        if (GameSystem.Instance == null) return;

        CheckPlayers();
        Countdown();  
    }

    private void CheckPlayers()
    {
        GameSystem gameSystem = GameSystem.Instance;
        if (gameSystem == null) return;

        // Get players
        PlayerManager playerManager = gameSystem.PlayerManager;
        if (playerManager == null) return;

        var players = playerManager.GetPlayers();

        // Check if player is inside collider
        int nrEnteredPlayers = 0;
        foreach (var player in players)
        {
            if (player == null || player.PlayerPawn == null) continue;
            if(_collider.IsPositionInBox(player.PlayerPawn.GetPlayerPos(), false))
            {
                ++nrEnteredPlayers;
            }
        }

        // If enough players, countdown
        if (nrEnteredPlayers >= GameSystem.Instance.PlayerManager.GetNrPlayers() / 2 && nrEnteredPlayers != 0)
        {
            bool wasCountdownActive = _startCountdown;
            _startCountdown = true;
            _timerObject.SetActive(true);
            if (!wasCountdownActive)
            {
                StartTimerEvent?.Invoke();
            }
        }
        // Else, stop countdown
        else
        {
            bool wasCountdownActive = _startCountdown;
            _startCountdown = false;
            _currentTimeBeforeStart = _timeBeforeStart;
            _timerObject.SetActive(false);
            if (wasCountdownActive)
            {
                StopTimerEvent?.Invoke();
            }
        }

        // Update text
        _playerNeedComponent.CurrentNrPlayers = nrEnteredPlayers;
        _playerNeedComponent.UpdateText();
    }
    private void Countdown()
    {
        // Countdown
        if (_startCountdown)
        {
            // Start game if reaches 0
            _currentTimeBeforeStart -= Time.deltaTime;
            if (_currentTimeBeforeStart < 0) StartGame();

            // Update textTimer
            _textComponent.SetText(((int)_currentTimeBeforeStart).ToString());
        }
    }

    // On Game Start
    // -------------
    private void StartGame()
    {
        // Reset score
        GameSystem.Instance.ScoreManager.ResetScore();

        // Send event
        GameStartedEvent?.Invoke(this);

        // Open new scene
        SceneLoader.Instance.LoadScene("GameScene");
    }
}
