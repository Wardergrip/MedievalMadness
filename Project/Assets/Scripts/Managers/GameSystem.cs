using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameSystem : Singleton<GameSystem>
{
    // Managers
    private PlayerManager _playerManager;
    private ScoreManager _scoreManager;
    private UIManager _uiManager;
    private ColorManager _colorManager;
    private GamemodeManager _gamemodeManager;
    private PauseMenu _pauseMenu;
    private ControlSchemeDisplay _controlSchemeDisplay;
    private ScreenShake _screenShake;

    public bool IsBeingDestroyed { get; private set; }

    // Start
    // -----
    private new void Awake()
    {
        // Get Managers
        _playerManager = FindObjectOfType<PlayerManager>();
        _scoreManager = FindObjectOfType<ScoreManager>();
        _uiManager = FindObjectOfType<UIManager>();
        _colorManager = FindObjectOfType<ColorManager>();
        _gamemodeManager = FindObjectOfType<GamemodeManager>();
        _pauseMenu = FindObjectOfType<PauseMenu>();
        _controlSchemeDisplay = FindObjectOfType<ControlSchemeDisplay>();

        // Late start
        StartCoroutine(LateStart(0.5f));
    }
    private IEnumerator LateStart(float seconds)
    {
        yield return new WaitForSeconds(seconds);
        _screenShake = GetComponentInChildren<ScreenShake>();
    }

    // Destruction
    // -----------
    private void OnDestroy()
    {
        // Set bool
        IsBeingDestroyed = true;

        // Destroy all managers
        Destroy(_screenShake);
        Destroy(_scoreManager);
        Destroy(_pauseMenu);
        Destroy(_controlSchemeDisplay);
        Destroy(_uiManager);
        Destroy(_colorManager);
        Destroy(_gamemodeManager);
        Destroy(_playerManager);
    }

    // Getters
    // -------
    public PlayerManager PlayerManager
    {
        get 
        {
            if (IsBeingDestroyed) return null;
            else                  return _playerManager; 
        }
    }
    public ScoreManager ScoreManager
    {
        get 
        {
            if (IsBeingDestroyed) return null;
            else                  return _scoreManager; 
        }
    }
    public UIManager UIManager
    {
        get 
        {
            if (IsBeingDestroyed) return null;
            else                  return _uiManager; 
        }
    }
    public ColorManager ColorManager
    {
        get 
        {
            if (IsBeingDestroyed) return null;
            else                  return _colorManager; 
        }
    }
    public GamemodeManager GamemodeManager
    {
        get 
        {
            if (IsBeingDestroyed) return null;
            else                  return _gamemodeManager; 
        }
    }
    public PauseMenu PauseMenu
    {
        get 
        {
            if (IsBeingDestroyed) return null;
            else                  return _pauseMenu; 
        }
    }
    public ScreenShake ScreenShake
    {
        get 
        {
            if (IsBeingDestroyed) return null;
            else                  return _screenShake; 
        }
    }
    public ControlSchemeDisplay ControlSchemeDisplay
    {
        get 
        {
            if (IsBeingDestroyed) return null;
            else                  return _controlSchemeDisplay; 
        }
    }
}
