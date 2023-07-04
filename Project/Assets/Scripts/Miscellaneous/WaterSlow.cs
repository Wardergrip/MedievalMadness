using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaterSlow : MonoBehaviour
{
    //[SerializeField] private bool _debugLogs = false;

    // Colliders
    private BoxCollider[] _colliders;
    private List<PlayerController> _enteredPlayers = new List<PlayerController>();

    // Start
    // -----
    void Start()
    {
        // Get Colliders
        _colliders = GetComponentsInChildren<BoxCollider>();
        Debug.Assert(_colliders.Length != 0, "No colliders found on waterSlow");

        // Event
        SceneLoader.Instance.SceneLoadedEvent += SceneLoadedEvent;
    }

    private void SceneLoadedEvent(string sceneName)
    {
        // Remove slow
        foreach (var player in GameSystem.Instance.PlayerManager.GetPlayers())
        {
            if (player && player.PlayerPawn) player.PlayerPawn.RemoveSlow();
        }
    }

    private void OnDestroy()
    {
        GameSystem gameSystem = GameSystem.Instance;
        if (gameSystem == null) return;

        PlayerManager playerManager = gameSystem.PlayerManager;
        if (playerManager == null) return;

        // Remove slow
        foreach (var player in playerManager.GetPlayers())
        {
            if (player.PlayerPawn) player.PlayerPawn.RemoveSlow();
        }

        // Remove event
        SceneLoader.Instance.SceneLoadedEvent -= SceneLoadedEvent;
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

        // Get players
        PlayerManager playerManager = gameSystem.PlayerManager;
        if (playerManager == null) return;

        var players = playerManager.GetPlayers();

        // Loop through players
        foreach (var player in players)
        {
            // Continue if is null
            if (player == null || player.PlayerPawn == null) continue;

            // Check if player contains collider
            bool containsPlayer = _enteredPlayers.Contains(player);
            bool inCollider = false;
            foreach (var collider in _colliders)
            {
                if (collider.IsPositionInBox(player.PlayerPawn.GetPlayerPos(), false))
                {
                    inCollider = true;

                    // Add slow
                    player.PlayerPawn.AddSlow();
                    _enteredPlayers.Add(player);
                    break;
                }
            }

            // If was in list and isn't in collider anymore, remove
            if (containsPlayer && inCollider == false)
            {
                player.PlayerPawn.RemoveSlow();
                _enteredPlayers.Remove(player);
            }
        }
    }

    //private void OnTriggerEnter(Collider other)
    //{
    //    if (other.GetComponent<Weapon>()) return;

    //    PlayerPawn player = other.GetComponentInParent<PlayerPawn>();
    //    if (player)
    //    {
    //        if (_debugLogs)
    //        {
    //            Debug.Log($"{other.gameObject.name} entered water");
    //        }
    //        player.AddSlow();
    //    }
    //}

    //private void OnTriggerExit(Collider other)
    //{
    //    if (other.GetComponent<Weapon>()) return;

    //    PlayerPawn player = other.GetComponentInParent<PlayerPawn>();
    //    if (player)
    //    {
    //        if (_debugLogs)
    //        { 
    //            Debug.Log($"{other.gameObject.name} left water");
    //        }
    //        player.RemoveSlow();
    //    }
    //}
}
