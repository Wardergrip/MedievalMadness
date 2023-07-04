using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[RequireComponent(typeof(BoxCollider))]
public class LeaveGame : MonoBehaviour
{
    [Header("Object")]
    [SerializeField] private GameObject _curtain;

    [Header("Settings")]
    [Tooltip("How long it takes before anyone can leave")]
    [SerializeField] private float _cooldown = 3f;

    // Colliders
    private BoxCollider _collider;
    private List<PlayerController> _enteredPlayers = new List<PlayerController>();

    // Curtain
    private bool _isBusyChanging;
    private Curtain _curtainScript;

    // Start
    private void Start()
    {
        // Collider
        _collider = GetComponent<BoxCollider>();

        // Curtain
        Debug.Assert(_curtain != null, "Error: no curtain found on colorChanger");
        _curtainScript = _curtain.GetComponent<Curtain>();

        _curtainScript.CurtainChanged += CurtainEvent;
    }
    private void CurtainEvent(bool curtainClosed)
    {
        if (curtainClosed)
        {
            RemovePlayers();
        }
        else
        {
            StartCoroutine(ResetCooldown());
        }
    }

    // Update
    // ------
    private void Update()
    {
        CheckPlayers();
    }

    private void CheckPlayers()
    {
        GameSystem gameSystem = GameSystem.Instance;
        if (gameSystem == null) return;
        if (_isBusyChanging) return;

        // Get players
        PlayerManager playerManager = gameSystem.PlayerManager;
        if (playerManager == null) return;

        var players = playerManager.GetPlayers();

        // Check if player is inside collider
        foreach (var player in players)
        {
            if (player == null || player.PlayerPawn == null) continue;
            if (_collider.IsPositionInBox(player.PlayerPawn.GetPlayerPos(), false))
            {
                _enteredPlayers.Add(player);
            }
        }

        // If enteredPlayers
        if (_enteredPlayers.Count > 0)
        {
            // Close curtain
            _isBusyChanging = true;
            _curtainScript.CloseCurtain();
        }
    }

    private void RemovePlayers()
    {
        // Remove all players in collider
        foreach (var player in _enteredPlayers)
        {
            player.RemoveControllerFromGame();
        }
        _enteredPlayers.Clear();
    }
    private IEnumerator ResetCooldown()
    {
        yield return new WaitForSeconds(_cooldown);
        _isBusyChanging = false;
    }
}
