using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private List<PlayerPanel> _playerPanels;
    [SerializeField] private List<TimerPanel> _timerPanels;
    [SerializeField] private Scoreboard _scoreboard;
    [SerializeField] private AnnouncementPanel _announcementPanel;

    public AnnouncementPanel AnnouncementPanel
    {
        get { return _announcementPanel; }
    }

    [Header("Player  Colors")]
    [Header("Red")]
    [SerializeField] private Sprite _redIcon;
    [SerializeField] private Sprite _redHeart;
    [SerializeField] private Color _redPanelColor;
    [Header("Purple")]
    [SerializeField] private Sprite _purpleIcon;
    [SerializeField] private Sprite _purpleHeart;
    [SerializeField] private Color _purplePanelColor;
    [Header("Green")]
    [SerializeField] private Sprite _greenIcon;
    [SerializeField] private Sprite _greenHeart;
    [SerializeField] private Color _greenPanelColor;
    [Header("Yellow")]
    [SerializeField] private Sprite _yellowIcon;
    [SerializeField] private Sprite _yellowHeart;
    [SerializeField] private Color _yellowPanelColor;

    public Sprite GetIcon(ColorManager.PlayerColors colorID)
    {
        switch (colorID) 
        {
            case ColorManager.PlayerColors.Red: return _redIcon;
            case ColorManager.PlayerColors.Purple: return _purpleIcon;
            case ColorManager.PlayerColors.Green: return _greenIcon;
            case ColorManager.PlayerColors.Yellow: return _yellowIcon;
        }
        return null;
    }

    public Sprite GetHeart(ColorManager.PlayerColors colorID) 
    {
        switch (colorID)
        {
            case ColorManager.PlayerColors.Red: return _redHeart;
            case ColorManager.PlayerColors.Purple: return _purpleHeart;
            case ColorManager.PlayerColors.Green: return _greenHeart;
            case ColorManager.PlayerColors.Yellow: return _yellowHeart;
        }
        return null;
    }

    public Color GetPanelColor(ColorManager.PlayerColors colorID)
    {
        switch (colorID)
        {
            case ColorManager.PlayerColors.Red: return _redPanelColor;
            case ColorManager.PlayerColors.Purple: return _purplePanelColor;
            case ColorManager.PlayerColors.Green: return _greenPanelColor;
            case ColorManager.PlayerColors.Yellow: return _yellowPanelColor;
        }
        return Color.black;
    }

    public Scoreboard Scoreboard { get { return _scoreboard; } }

    private void Start()
    {
        StartCoroutine(SubscribeManagers_Coroutine());
        SceneLoader.Instance.SceneLoadedEvent += OnSceneChange;
    }

    private IEnumerator SubscribeManagers_Coroutine()
    {
        var gameSys = GameSystem.Instance;
        while (gameSys.PlayerManager == null) 
        {
            yield return null;
        }
        gameSys.PlayerManager.AddPlayerEvent += RegisterPlayer;
        gameSys.PlayerManager.RemovePlayerEvent += UnregisterPlayer;
        while (gameSys.ScoreManager == null)
        {
            yield return null;
        }
        gameSys.ScoreManager.ScoreUpdatedEvent += UpdateScore;
    }

    private void OnDestroy()
    {
        // Remove events
        GameSystem gameSystem = GameSystem.Instance;
        if (gameSystem)
        {
            PlayerManager playerManager = gameSystem.PlayerManager;
            if (playerManager)
            {
                playerManager.AddPlayerEvent -= RegisterPlayer;
                playerManager.RemovePlayerEvent -= UnregisterPlayer;
            }

            ScoreManager scoreManager = gameSystem.ScoreManager;
            if (scoreManager) scoreManager.ScoreUpdatedEvent -= UpdateScore;
        }

        SceneLoader sceneLoader = SceneLoader.Instance;
        if (sceneLoader) sceneLoader.SceneLoadedEvent -= OnSceneChange;
    }

    private const string MAIN_MENU_SCENE_NAME = "MainMenuScene";
    private const string GAME_SCENE = "GameScene";
    public void OnSceneChange(string sceneName)
    {
        if (sceneName == MAIN_MENU_SCENE_NAME)
        {
            foreach (var playerPanel in _playerPanels)
            {
                if (playerPanel.IsPlayerAssigned)
                {
                    playerPanel.Hide();
                }
            }
            Debug.Log($"New scene is mainmenu, Previous scenename is {SceneLoader.Instance.PreviousSceneName}");
            if (SceneLoader.Instance.PreviousSceneName == GAME_SCENE && Scoreboard)
            {
                Scoreboard.SetScoreboardVisibility(5.0f);
            }
        }
        else if (sceneName == GAME_SCENE) 
        {
            foreach (var playerPanel in _playerPanels)
            {
                if (playerPanel.IsPlayerAssigned)
                {
                    playerPanel.Show();
                }
            }
        }
    }
    public void RegisterPlayer(PlayerController player)
    {
        _playerPanels.Find(x => !x.IsPlayerAssigned).AssignPlayer(player.PlayerID);
        _timerPanels.Find(x => !x.IsPlayerAssigned).AssignPlayer(player.PlayerID);
        _scoreboard.AssignPlayer(player.PlayerID);
        StartCoroutine(SubscribePlayer_Coroutine(player, 0.1f));
    }

    private IEnumerator SubscribePlayer_Coroutine(PlayerController player,float waitTime)
    {
        yield return new WaitForSeconds(waitTime);
        player.Lives.LifesUpdatedEvent += UpdateLives;
        player.PlayerPawn.SmashHealth.DamageEvent += UpdateHealth;
        player.PlayerPawn.InFireEvent += UpdateFire;

        UpdateHealth(player);
        UpdateScore(player.PlayerID);
        var playerPanel = _playerPanels.Find(x => x.AssignedPlayerId == player.PlayerID);
        if (playerPanel == null) yield return null;
        playerPanel.SetAmountOfLives(player.Lives.Amount);
    }

    public void UnregisterPlayer(PlayerController player) 
    {
        int idx = _playerPanels.FindIndex(x => x.AssignedPlayerId == player.PlayerID);
        if (idx > -1)
        {
            _playerPanels[idx].AssignPlayer(null);
            player.Lives.LifesUpdatedEvent -= UpdateLives;
        }
        idx = _timerPanels.FindIndex(x => x.GetAssignedPlayerId == player.PlayerID);
        if (idx > -1)
        {
            _timerPanels[idx].AssignPlayer(null);
        }
        _scoreboard.UnassignPlayer(player.PlayerID);
    }


    private void UpdateLives(PlayerController player)
    {
        var playerPanel = _playerPanels.Find(x => x.AssignedPlayerId == player.PlayerID);
        if (playerPanel == null) return;
        playerPanel.SetAmountOfLives(player.Lives.Amount);

        playerPanel.SetHealthTextSizeSmall();
        playerPanel.SetHealthText("dead", false);
        playerPanel.SetGreyOut(true);

        StartCoroutine(ResetHealthText_Coroutine(playerPanel,player));
    }

    private IEnumerator ResetHealthText_Coroutine(PlayerPanel panel, PlayerController player)
    {
        // +0.1f is necessary because Lives otherwise isn't initialised yet
        yield return new WaitForSeconds(SettingsManager.Instance.PlayerSettings.spawnTimer + 0.1f);
        if (player.Lives.CanRespawn)
        {
            panel.SetHealthTextSizeBig();
            while (player.PlayerPawn == null || player.PlayerPawn.SmashHealth == null)
            {
                yield return null;
            }
            panel.SetHealthText(((int)player.PlayerPawn.SmashHealth.Health).ToString());
            panel.SetGreyOut(false);
            player.PlayerPawn.SmashHealth.DamageEvent += UpdateHealth;
            player.PlayerPawn.InFireEvent += UpdateFire;
        }
    }

    private void UpdateScore(short playerId)
    {
        _playerPanels.Find(x => x.AssignedPlayerId == playerId)
            ?.SetScoreNumberText(GameSystem.Instance.ScoreManager.GetScore(playerId).ToString());
    }

    private void UpdateHealth(PlayerController player)
    {
        var playerPanel = _playerPanels.Find(x => x.AssignedPlayerId == player.PlayerID);
        if (playerPanel)
        {
            if (player.PlayerPawn && player.PlayerPawn.SmashHealth)
            {
                playerPanel.SetHealthText(((int)player.PlayerPawn.SmashHealth.Health).ToString());
            }
            playerPanel.HelmetShake?.Shake();
        }
    }

    private void UpdateFire(PlayerController player)
    {
        var playerPanel = _playerPanels.Find(x => x.AssignedPlayerId == player.PlayerID);
        playerPanel?.SetFireVisibility(true);
    }
}
