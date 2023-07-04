using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class TimerPanel : MonoBehaviour
{
    [Header("Object references")]
    [SerializeField] private GameObject _panelObj;
    [SerializeField] private GameObject _timerTextObj;
    private TextMeshProUGUI _timerText;

    [Header("Values")]
    [SerializeField] private float _updateTimer = 0.1f;
    public float UpdateTimer { get { return _updateTimer; } }

    [Header("Watch values")]
    [SerializeField] private short _assignedPlayerId = short.MinValue;
    public short GetAssignedPlayerId { get { return _assignedPlayerId; } }
    public bool IsPlayerAssigned { get { return _assignedPlayerId != short.MinValue; } }

    private void Start()
    {
        Debug.Assert(_panelObj, "PanelObj not assigned"); 
        Debug.Assert(_timerTextObj, "TimerTextObj not assigned");
        _timerText = _timerTextObj.GetComponent<TextMeshProUGUI>();
        _timerTextObj.SetActive(false);
    }

    public void AssignPlayer(short? playerId)
    {
        if (playerId == null)
        {
            if (_assignedPlayerId != short.MinValue) 
            {
                var player = GameSystem.Instance.PlayerManager.GetPlayer(_assignedPlayerId);
                if (player)
                {
                    player.DeathEvent -= PlayerDied;
                }
            }
            _assignedPlayerId = short.MinValue;
            _panelObj.SetActive(false);
            return;
        }
        Debug.Assert(playerId.Value < 4 && playerId.Value > -1, "Player ID is in an unexpected range");
        _assignedPlayerId = playerId.Value;
        GameSystem.Instance.PlayerManager.GetPlayer(playerId.Value).DeathEvent += PlayerDied;
    }

    public void SetTimerText(string text, bool addSecondsAbriv = true)
    {
        _timerText.text = text + (addSecondsAbriv ? "s" : "");
    }

    public void Hide()
    {
        _timerTextObj.SetActive(false);
    }

    public void Show()
    {
        _timerTextObj.SetActive(true);
    }

    private void PlayerDied(PlayerController player)
    {
        if (player.Lives.CanRespawn)
        {
            StartCoroutine(UpdateDeathTimer_Coroutine());
        }
    }

    private IEnumerator UpdateDeathTimer_Coroutine()
    {
        float timer = SettingsManager.Instance.PlayerSettings.spawnTimer;
        float waitTime = Mathf.Min(UpdateTimer, timer);
        Show();
        while (timer > 0)
        {
            SetTimerText(timer.ToString("0.00"));
            timer -= waitTime;
            yield return new WaitForSeconds(waitTime);
        }
        Hide();
    }
}
