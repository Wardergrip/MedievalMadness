using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenu : MonoBehaviour
{
    [SerializeField] private GameObject _pauseMenu;
    [SerializeField] private TextMeshProUGUI _leaveButton;

    // Scenes
    private string _mainMenuName = "MainMenuScene";
    private string _titleScreenName = "TitleScreenScene";

    // Bools
    private bool _isPaused = false;
    private bool _isInMainMenu = false;

    // Leave text
    private string _leaveText = "Leave";
    private string _quitText = "Quit";

    // Start
    // -----
    private void Start()
    {
        // PauseMenu object
        Debug.Assert(_pauseMenu != null, "No pauseMenu found in the pauseMenu script");
        _pauseMenu.SetActive(false);

        Debug.Assert(_leaveButton != null, "No leaveButton found in the pauseMenu script");

        // Scene
        if (SceneManager.GetActiveScene().name == _mainMenuName)
        {
            _isInMainMenu = true;
            _leaveButton.text = _quitText;
        }

        // Event
        SceneLoader.Instance.SceneLoadedEvent += SceneLoadedEvent;
    }

    private void SceneLoadedEvent(string sceneName)
    {
        // If in mainMenu
        if (sceneName == _mainMenuName)
        {
            _isInMainMenu = true;
            _leaveButton.text = _quitText;
        }
        // Else
        else
        {
            _isInMainMenu = false;
            _leaveButton.text = _leaveText;
        }
    }

    public void OnPause()
    {
        if (_isPaused)
        {
            OnResume();
        }
        else
        {
            _isPaused = true;

            // Show menu and stop time
            _pauseMenu.SetActive(true);
            Time.timeScale = 0.0f;
        }
    }
    public void OnResume()
    {
        _isPaused = false;

        // Remove menu and re-enable time
        _pauseMenu.SetActive(false);
        Time.timeScale = 1.0f;
    }
    public void OnLeave()
    {
        // If in mainMenu
        if (_isInMainMenu)
        {
            SceneLoader.Instance.LoadScene(_titleScreenName);
        }
        // Else
        else
        {
            GameSystem.Instance.PlayerManager.Players.ForEach( x =>
                {
                    x.Lives.ResetLives();
                    x.PlayerPawn?.SmashHealth?.ResetHealth();
                    x.PlayerPawn?.DropWeapon();
                }
            );
            // Load mainMenu
            SceneLoader.Instance.LoadScene(_mainMenuName);
        }

        // Remove menu and re-enable time
        _pauseMenu.SetActive(false);
        Time.timeScale = 1.0f;
    }
}
