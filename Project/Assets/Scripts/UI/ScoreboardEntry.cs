using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Unity.VisualScripting;
using UnityEngine.SceneManagement;

public class ScoreboardEntry : MonoBehaviour
{
    private bool _bootingUp = true;

    [SerializeField] private Image _icon;
    [SerializeField] private TextMeshProUGUI _scoreNumber;

    private short _assignedPlayerId = short.MinValue;
    public short AssignedPlayerId { get { return _assignedPlayerId; } }
    public bool IsPlayerAssigned { get { return AssignedPlayerId != short.MinValue; } }

    public void AssignPlayer(short? playerId)
    {
        if (playerId == null)
        {
            if (_assignedPlayerId != short.MinValue)
            {
                // Unsubscribe
            }
            _assignedPlayerId = short.MinValue;
            gameObject.SetActive(false);
            return;
        }
        Debug.Assert(playerId.Value < 4 && playerId.Value > -1, "Player ID is in an unexpected range");
        _assignedPlayerId = playerId.Value;
        gameObject.SetActive(true);
    }

    private void Awake()
    {
        _scoreNumber.text = "0";
        gameObject.SetActive(false);
        _bootingUp = true;
    }

    private void Start()
    {
        // For some reason necessary otherwise the ColorManager is getting accessed too early
        _bootingUp = false;

        var scoreManager = GameSystem.Instance.ScoreManager;
        var colorManager = GameSystem.Instance.ColorManager;
        if (scoreManager) scoreManager.ScoreUpdatedEvent += UpdateScore;
        if (colorManager) colorManager.ColorChangeEvent += UpdateColor;
    }

    private void OnEnable()
    {
        if (_bootingUp) return;

        var scoreManager = GameSystem.Instance.ScoreManager;
        var colorManager = GameSystem.Instance.ColorManager;
        if (scoreManager) scoreManager.ScoreUpdatedEvent += UpdateScore;
        if (colorManager) colorManager.ColorChangeEvent += UpdateColor;
    }
    private void OnDisable()
    {
        if (_bootingUp) return;

        GameSystem gameSystem = GameSystem.Instance;

        if (gameSystem)
        {
            gameSystem.ScoreManager.ScoreUpdatedEvent -= UpdateScore;
            gameSystem.ColorManager.ColorChangeEvent -= UpdateColor;
        }
    }

    public void FirstUpdate()
    {
        if (!IsPlayerAssigned) return;

        UpdateScore(AssignedPlayerId);
        var playerPawn = GameSystem.Instance.PlayerManager.GetPlayer(AssignedPlayerId).PlayerPawn;
        UpdateColor(playerPawn,playerPawn.PawnColor);
    }

    private void UpdateScore(short playerId)
    {
        if (!(IsPlayerAssigned && playerId == AssignedPlayerId)) return;

        _scoreNumber.text = GameSystem.Instance.ScoreManager.GetScore(playerId).ToString();
    }

    private void UpdateColor(PlayerPawn playerPawn, ColorManager.PlayerColors colorID)
    {
        if (!(IsPlayerAssigned && playerPawn.PlayerID == AssignedPlayerId)) return;

        _icon.sprite = GameSystem.Instance.UIManager.GetIcon(colorID);
    }
}
