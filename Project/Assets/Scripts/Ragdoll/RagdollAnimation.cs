using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RagdollAnimation : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private bool _useAnimations = true;

    // Player
    public PlayerPawn ParentPawn { get; set;}
    private CopyLimb[] _limbScripts;

    // Conditions
    public bool IsMovement { get; set; }
    private bool _hasWon;

    // Animator
    private Animator _animator;
    private string _isRunningString = "IsRunning";
    private string _hasWonString = "HasWon";


    // Start
    // -----
    void Start()
    {
        // Hide animatedMesh
        // -----------------
        Renderer[] animatedMeshRenderers = gameObject.GetComponentsInChildren<Renderer>();
        foreach (Renderer meshRenderer in animatedMeshRenderers)
        {
            meshRenderer.enabled = false;
        }

        // Get animator
        // ------------
        _animator = gameObject.GetComponentInChildren<Animator>();
        Debug.Assert(_animator != null, "Error: No animator found on the animatedMesh");

        // Get limb scripts
        // ----------------
        _limbScripts = gameObject.GetComponentsInChildren<CopyLimb>();

        // Events
        // ------
        GameSystem.Instance.GamemodeManager.RoundEnded += GameRoundEnded;
        SceneLoader.Instance.SceneLoadedEvent += SceneLoadedEvent;
    }

    // Events
    // ------
    private void SceneLoadedEvent(string sceneName)
    {
        _hasWon = false;
    }
    private void GameRoundEnded(short winningPlayerID)
    {
        if (winningPlayerID == ParentPawn.PlayerID)
        {
            _hasWon = true;
        }
    }

    // Update
    // ------
    private void Update()
    {
        if (_useAnimations == false) return;

        HandleTransitions();
    }

    private void HandleTransitions()
    {
        _animator.SetBool(_isRunningString, IsMovement);
        _animator.SetBool(_hasWonString, _hasWon);
    }

    // Death
    // -----
    public void OnDeath()
    {
        // Disable animatedMesh
        gameObject.SetActive(false);

        // Stop all the limb scripts
        foreach (CopyLimb script in _limbScripts)
        {
            script.enabled = false;
        }
    }
}
