using System;
using UnityEngine;

public class PlayerLives : MonoBehaviour
{
    [Header("Watch values")]
    [SerializeField] private int _amountOfLives;

    public PlayerController PlayerController { get; set; }
    public event Action<PlayerController> LifesUpdatedEvent;

    private void Awake()
    {
        ResetLives();
    }

    public int Amount { get { return _amountOfLives; } }
    public int StartAmount { get { return _startAmount; } }
    private int _startAmount;

    public bool CanRespawn { get { return _amountOfLives > 0; } }

    public void ReduceLife()
    {
        --_amountOfLives;
        LifesUpdatedEvent?.Invoke(PlayerController);
    }

    public void ResetLives()
    {
        _startAmount = SettingsManager.Instance.PlayerSettings.startLives;
        _amountOfLives = _startAmount;

        LifesUpdatedEvent?.Invoke(PlayerController);
    }
}
