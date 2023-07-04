using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class PlayerController : MonoBehaviour
{
    // Player
    [SerializeField] private GameObject _playerPrefab;
    private PlayerPawn _playerPawn;
    public PlayerPawn PlayerPawn { get { return _playerPawn; } }
    
    public short PlayerID { get; set; }

    bool _isInitialized = false;

    // Input
    public int GamepadID { get; set; }

    PlayerInput _playerInput;

    // Position
    private Vector3 _startPosition = Vector3.zero;
    public Vector3 StartPosition
    {
        set { _startPosition = value; }
    }

    // Lives
    private PlayerLives _lives;
    public PlayerLives Lives { get { return _lives; } }
    public event Action<PlayerController> DeathEvent;

    [Header("Input")]
    [Tooltip("The amount of time that has to be between a pickup input and a drop input or vice versa")]
    [SerializeField] private float _minimumDelayPickUpNDrop = .2f;
    [SerializeField] private bool _debugLogPickUpInput = false;

    public bool CanPickUp { get; set; } = true;

    // Color
    private ColorManager.PlayerColors _pawnColor;
    public ColorManager.PlayerColors PawnColor
    {
        get { return _pawnColor; }
        set { _pawnColor = value; }
    }

    // Scene
    private string gameSceneName = "GameScene";
    private bool _hasToSetPosition = false;

    // Other
    private bool _isBeingDestroyed = false;

    // Input events
    // ------------
    public void OnCharachterMove(InputAction.CallbackContext ctx)
    {
        if (_playerPawn)
        {
            _playerPawn.OnCharachterMove(ctx);
        }
    }
    public void OnArmMove(InputAction.CallbackContext ctx)
    {
        if (_playerPawn)
        {
            _playerPawn.OnArmMove(ctx);
        }
    }
    public void OnPickUp(InputAction.CallbackContext ctx)
    {
        // Only call the player pawn function if the button got pressed and not
        // if it is fully pressed or released
        if (!ctx.started) return;

        if (_debugLogPickUpInput) Debug.Log("PickUp input registered...");
        if (_playerPawn)
        {
            if (CanPickUp)
            {
                if (_debugLogPickUpInput) Debug.Log("We can pick up, delegating to pawn...");
                _playerPawn.OnPickUp(ctx);

                if (_playerPawn.EquippedWeaponScript)
                {
                    if (_debugLogPickUpInput) Debug.Log("We have picked up something, starting coroutine.");
                    StartCoroutine(ToggleCanPickUp(false));
                }
            }
        }
    }
    public void OnDrop(InputAction.CallbackContext ctx)
    {
        // Only call the player pawn function if the button got pressed and not
        // if it is fully pressed or released
        if (!ctx.started) return;

        if (_debugLogPickUpInput) Debug.Log("Drop input registered...");
        if (_playerPawn)
        {
            if (CanPickUp == false)
            {
                if (_debugLogPickUpInput) Debug.Log("We can drop, dropping and starting coroutine");
                _playerPawn.OnDrop(ctx);
                StartCoroutine(ToggleCanPickUp(true));
            }
        }
    }
    public void OnWeaponAbility(InputAction.CallbackContext ctx)
    {
        // Only call the player pawn function if the button got pressed and not
        // if it is fully pressed or released
        if (!ctx.started) return;

        if (_playerPawn)
        {
            _playerPawn.OnWeaponAbility();
        }
    }
    //public void OnGrabHold(InputAction.CallbackContext ctx)
    //{
    //    if (!ctx.started) return;

    //    if (_playerPawn)
    //    {
    //        _playerPawn.OnGrabHold();
    //    }
    //}
    //public void OnGrabRelease(InputAction.CallbackContext ctx)
    //{
    //    if (!ctx.canceled) return;

    //    if (_playerPawn)
    //    {
    //        _playerPawn.OnGrabRelease();
    //    }
    //}
    public void OnLeftPunch(InputAction.CallbackContext ctx)
    {
        if (!ctx.started) return;

        if (_playerPawn)
        {
            _playerPawn.OnLeftPunch();
        }
    }
    public void OnRightPunch(InputAction.CallbackContext ctx)
    {
        if (!ctx.started) return;

        if (_playerPawn)
        {
            _playerPawn.OnRightPunch();
        }
    }

    public void OnPause(InputAction.CallbackContext ctx)
    {
        if (!ctx.started) return;

        GameSystem.Instance.PauseMenu.OnPause();
        Debug.Log("paused");
    }

    // Input helpers
    // --------------
    private IEnumerator ToggleCanPickUp(bool state)
    {
        yield return new WaitForSeconds(_minimumDelayPickUpNDrop);
        CanPickUp = state;
    }

    // Start
    // -----
    void Start()
    {
        // Return if is being destroyed
        if (GameSystem.Instance.IsBeingDestroyed) return;

        GameSystem.Instance.ControlSchemeDisplay.Hide();

        // Default gamePadID
        GamepadID = -1;

        _playerInput = GetComponent<PlayerInput>();

#if UNITY_STANDALONE && !UNITY_EDITOR
        // If is in gameScene, don't Init
        if (SceneManager.GetActiveScene().name == gameSceneName)
        {
            RemoveControllerFromGame();
            return;
        }
#endif
        _isInitialized = true;

        // Player managing
        Debug.Assert(_playerPrefab, "No player prefab assigned");
        GameSystem.Instance.PlayerManager.AddPlayer(this);
        
        // Input
        if (_playerInput.devices[0] is Gamepad)
        {
            GameSystem.Instance.PlayerManager.AddGamepad(this);
        }

        // Lives
        _lives = GetComponent<PlayerLives>();
        Debug.Assert(_lives, "No lives script found on playerController");
        _lives.PlayerController = this;

        // Scene change
        SceneManager.activeSceneChanged += SceneChanged;

        // Spawning
        DontDestroyOnLoad(gameObject);
        Spawn();

        Debug.Log("PlayerID: " + PlayerID);
        Debug.Log("GamePadID: " + GamepadID);
    }

    private void SceneChanged(Scene currentScene, Scene nextScene)
    {
        // If loading into gameScene
        if (Lives.Amount > 0)
        {
            GameSystem.Instance.PlayerManager.SetRandomSpawnPosition(ref _startPosition, true);
            Spawn();
        }
        if (nextScene.name == gameSceneName)
        {
            if (_playerPawn == null) _hasToSetPosition = true;
            else
            {
                GameSystem.Instance.PlayerManager.SetRandomSpawnPosition(ref _startPosition, true);
                SetPositionOfPawn(_startPosition);
            }
        }
    }

    private void OnDestroy()
    {
        // We use ?. because on closing the program it doesn't matter if we remove ourself
        // from the manager.
        if (_isInitialized)
        {
            GameSystem gameSystem = GameSystem.Instance;
            if (gameSystem)
            {
                PlayerManager playerManager = gameSystem.PlayerManager;
                if (playerManager) playerManager.RemovePlayer(this);
            }
        }

        SceneManager.activeSceneChanged -= SceneChanged;
        _isBeingDestroyed = true;
    }

    public void Spawn()
    {
        if (Lives.Amount < Lives.StartAmount) GameSystem.Instance.PlayerManager.SetRandomSpawnPosition(ref _startPosition);
        Spawn(_startPosition);
    }
    public void Spawn(Vector3 spawnLocation)
    {
        // If is being destroyed, return
        if (_isBeingDestroyed) return;

        // If can't respawn, return
        if (!Lives.CanRespawn) return;

        // Return if alreadyPawn
        if (_playerPawn != null) return;

        // Create pawn
        _playerPawn = Instantiate(_playerPrefab, transform).GetComponent<PlayerPawn>();
        _playerPawn.transform.position = spawnLocation;
        _playerPawn.PlayerController = this;

        _playerPawn.PlayerID = PlayerID;
        _playerPawn.GamepadID = GamepadID;
        _playerPawn.PawnColor = _pawnColor;

        // Add pawn to camera field
        GameSystem.Instance.PlayerManager.AddToCamera(_playerPawn.GetPlayerTransform());

        // Set position
        if (_hasToSetPosition)
        {
            SetPositionOfPawn(_startPosition);
            _hasToSetPosition = false;
        }
    }

    public void SetPositionOfPawn(Vector3 position)
    {
        _playerPawn.SetPlayerPos(position);
    }

    public void OnDeath()
    {
        // Return if there is no playerPawn
        //if (_playerPawn == null) return;

        Lives.ReduceLife();
        DeathEvent?.Invoke(this);

        // Remove pawn from camera field, start spawn timer
        GameSystem.Instance.PlayerManager.RemoveFromCamera(_playerPawn.GetPlayerTransform());
        StartCoroutine(Spawn_Coroutine());

        // Set playerPawn to null
        _playerPawn = null;
    }
    private IEnumerator Spawn_Coroutine()
    {
        // After time, spawn
        yield return new WaitForSeconds(SettingsManager.Instance.PlayerSettings.spawnTimer);
        Spawn();
    }

    public void RemoveControllerFromGame()
    {
        Destroy(gameObject);
    }
    public void RemovePlayerPawn()
    {
        Destroy(_playerPawn);
        _playerPawn = null;
    }
}
