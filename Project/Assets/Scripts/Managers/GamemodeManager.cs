using System.Linq;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using System;

public class GamemodeManager : MonoBehaviour
{
    [System.Serializable]
    public class WeightedGameRound
    {
        public GameRound Round;
        public float Weight;
    }
    [Header("GameRounds")]
    public WeightedGameRound[] GameRounds;

    [Header("RoundSettings")]
    [SerializeField] private int _amountOfRounds = 3;
    [SerializeField] private float _restAfterRound = 5.0f;

    [Header("Watch Values")]
    [SerializeField] private GameRound _activeRound;
    [SerializeField] private int _amountOfRoundsLeft = 3;

    private bool _isInit = true;
    public event Action<short> RoundEnded;

    private bool _roundHasEnded = false;
    public bool RoundHasEnded
    {
        get { return _roundHasEnded = false; }
        set { _roundHasEnded = value; }
    }


    // Source: https://docs.unity3d.com/ScriptReference/SceneManagement.SceneManager-sceneLoaded.html
    private const string MAINGAME_SCENE_NAME = "GameScene";

    // called first
    void OnEnable()
    {
        if (_isInit)
        {
            _isInit = false;
            var activeScene = SceneManager.GetActiveScene();
            if (activeScene.name == MAINGAME_SCENE_NAME)
            {
                OnSceneLoaded(activeScene.name);
            }
        }
        SceneLoader.Instance.SceneLoadedEvent += OnSceneLoaded;
        _amountOfRoundsLeft = _amountOfRounds;
    }

    // called second
    void OnSceneLoaded(string sceneName)
    {
        if (sceneName != MAINGAME_SCENE_NAME)
        {
            var winningPlayerId = GameSystem.Instance.ScoreManager.GetWinningPlayerId();
            if (winningPlayerId.HasValue)
            {
                var playerPawn = GameSystem.Instance.PlayerManager.GetPlayer(winningPlayerId.Value).PlayerPawn;
                var obj = Instantiate(Resources.Load("WinningPlayer") as GameObject);
                obj.GetComponent<WinningPlayer>().WinningPlayerPawn = playerPawn;
                obj.GetComponent<PositionFollower>().TransformToFollow = playerPawn.GetPlayerTransform();
            }
            return;
        }

        // We are at the start of the round

        _activeRound = GetRandGameRound();
        Debug.Assert(_activeRound, "ActiveRound is null. Check if there are rounds in the GamemodeManager and the weights are correct.");
        _activeRound.Init();
        Debug.Log($"Start of round. Name: {_activeRound.RoundName} Desc: {_activeRound.Description}");
        var announcementPanel = GameSystem.Instance.UIManager.AnnouncementPanel;
        announcementPanel.Title.text = _activeRound.RoundName;
        announcementPanel.Description.text = _activeRound.Description;
        announcementPanel.Show();

        // SpawnAreas
        {
            SpawnArea[] spawnAreas = FindObjectsOfType<SpawnArea>();
            foreach (SpawnArea spawnArea in spawnAreas)
            {
                switch (spawnArea.gameObject.name)
                {
                    case "WeaponSpawner Platform":
                        spawnArea.SpawnableObjects = _activeRound.PlatformSpawnList;
                        break;
                    case "WeaponSpawner Stairs":
                        spawnArea.SpawnableObjects = _activeRound.StairsSpawnList;
                        break;
                    case "WeaponSpawner Field":
                        spawnArea.SpawnableObjects = _activeRound.FieldSpawnList;
                        break;
                    case null:
                        Debug.LogWarning("Found a spawnarea with an unexpected name. Gamemode will not alter it's objects to spawn.");
                        break;
                }
                spawnArea.SpawnInitialWeapons();
            }
        }

        // Settings
        {
            SettingsManager.Instance.HazardSettings = _activeRound.HazardSettings;
            SettingsManager.Instance.PlayerSettings = _activeRound.PlayerSettings;
            SettingsManager.Instance.SmashHealthSettings = _activeRound.SmashHealthSettings;
            SettingsManager.Instance.SpawnAreaSettings = _activeRound.SpawnAreaSettings;
            SettingsManager.Instance.DragonSettings = _activeRound.DragonSettings;

            foreach (PlayerController p in GameSystem.Instance.PlayerManager.Players)
            {
                p.Lives.ResetLives();
                p?.PlayerPawn?.SmashHealth?.ResetHealth();
            }
        }

        OnLoadDontPlay.IsLoading = false;
    }


    private void Start()
    {
        StartCoroutine(SubscribeManager_Coroutine());
    }

    private IEnumerator SubscribeManager_Coroutine()
    {
        var gameSys = GameSystem.Instance;
        while (gameSys.PlayerManager == null)
        {
            yield return null;
        }
        gameSys.PlayerManager.AddPlayerEvent += PlayerAdded;
    }

    private void PlayerAdded(PlayerController player)
    {
        player.DeathEvent += PlayerDied;
    }

    private void PlayerDied(PlayerController player)
    {
        int counter = 0;
        PlayerController lastPlayer = null;
        GameSystem.Instance.PlayerManager.Players.ForEach(p => 
        { 
            if (p.Lives.Amount > 0)
            {
                lastPlayer = p;
                ++counter; 
            }
        });
        if (counter <= 1)
        {
            _roundHasEnded = true;
            Debug.Log("Game ended. Only one player with more than 0 lives");
            
            var announcementPanel = GameSystem.Instance.UIManager.AnnouncementPanel;
            announcementPanel.Title.text = "Round over!";
            announcementPanel.Description.text = $"Player {lastPlayer.PlayerID + 1} won the round.";
            announcementPanel.Show();

            GameSystem.Instance.ScoreManager.AddScoreSoleSurvivor(lastPlayer.PlayerID);

            short? winningPlayerID = GameSystem.Instance.ScoreManager.GetWinningPlayerId();
            if (winningPlayerID != null) RoundEnded?.Invoke(winningPlayerID.Value);

            StartCoroutine(RoundEnd_Coroutine());
        }
    }

    private IEnumerator RoundEnd_Coroutine()
    {
        yield return new WaitForSeconds(_restAfterRound);
        OnLoadDontPlay.IsLoading = true;
        foreach (PlayerController p in GameSystem.Instance.PlayerManager.Players)
        {
            p.Lives.ResetLives();
            p.PlayerPawn?.SmashHealth?.ResetHealth();
            p.PlayerPawn?.DropWeapon();
        }
        --_amountOfRoundsLeft;
        if (_amountOfRoundsLeft == 0)
        {
            SceneLoader.Instance.LoadScene("MainMenuScene");
            _amountOfRoundsLeft = _amountOfRounds;
        }
        else
        {
            SceneLoader.Instance.LoadScene("GameScene");
        }
    }

    // called when the game is terminated
    void OnDisable()
    {
        var sceneLoader = SceneLoader.Instance;
        if (sceneLoader) sceneLoader.SceneLoadedEvent -= OnSceneLoaded;
    }

    private GameRound GetRandGameRound()
    {
        float totalWeight = GameRounds.Sum(obj => obj.Weight);

        float randomWeight = UnityEngine.Random.Range(0f, totalWeight);

        foreach (WeightedGameRound obj in GameRounds)
        {
            randomWeight -= obj.Weight;
            if (randomWeight <= 0)
            {
                return obj.Round;
            }
        }

        throw new UnityException("Something went terribly wrong. TotalWeight might've been 0");
    }
}