using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using UnityEngine.SceneManagement;
using System;
using Random = UnityEngine.Random;

public class PlayerManager : MonoBehaviour
{
    [Header("Cinemachine")]
    [Tooltip("This variable can not be editted in runtime. This value determines, at a minimum, the distance between the camera and the player.")]
    [SerializeField] private float _minimumDistPlayerNCamera = 1.0f;
    private CinemachineTargetGroup _cmTargetGroup;

    // PlayerControllers
    private List<PlayerController> _players = new List<PlayerController>();
    public List<PlayerController> Players { get { return _players; } }
    private int _previousGamepadCount = 0;

    // PlayerSpawns
    private GameObject[] _playerSpawns;
    private string _playerSpawnTag = "PlayerSpawn";
    private int _previousPlayerSpawnID = 0;

    // Camera gameObject
    private GameObject _tempCameraGameObject;

    // Scenes
    private string _mainMenuName = "MainMenuScene";
    private string _titleScreenName = "TitleScreenScene";

    // Players / Camera
    // ----------------
    public event Action<PlayerController> AddPlayerEvent;
    public event Action<PlayerController> RemovePlayerEvent;

    public void AddPlayer(PlayerController controller)
    {
        // Remove temp cameraObject
        if (_tempCameraGameObject)
        {
            _cmTargetGroup.RemoveMember(_tempCameraGameObject.transform);
            Destroy(_tempCameraGameObject);
        }

        // Player ID
        controller.PlayerID = GetNextAvailablePlayerId();
        _players.Add(controller);

        // Set spawnPoint
        if (_playerSpawns.Length <= 0) FindPlayerSpawns("");
        controller.StartPosition = _playerSpawns[controller.PlayerID].transform.position;

        // Event
        AddPlayerEvent?.Invoke(controller);
        controller.DeathEvent += DeathEffect.Spawn;
    }
    public void AddToCamera(Transform playerTransform)
    {
        if (_cmTargetGroup)
        {
            Debug.Log("Added playerTransform " + playerTransform.name + " to the camera");
            _cmTargetGroup.AddMember(playerTransform, 1, _minimumDistPlayerNCamera);
        }
    }
    public void AddGamepad(PlayerController controller)
    {
        controller.GamepadID = _previousGamepadCount;
        ++_previousGamepadCount;
    }

    public void RemovePlayer(PlayerController controller) 
    {
        // Remove controller
        bool isRemovedSuccesfully = _players.Remove(controller);
        Debug.Assert(isRemovedSuccesfully, $"Tried to remove player that is not in the internal list: {controller.gameObject.name}");

        // Lower IDs
        if (controller.GamepadID != -1) --_previousGamepadCount;

        // Remove Events
        RemovePlayerEvent?.Invoke(controller);

        controller.DeathEvent -= DeathEffect.Spawn;

        // If all players are removed in mainMenu scene
        if (_players.Count <= 0 && SceneManager.GetActiveScene().name == _mainMenuName)
        {
            SceneLoader.Instance.LoadScene(_titleScreenName);
        }
    }
    public void RemoveFromCamera(Transform playerTransform)
    {
        if (_cmTargetGroup)
        {
            Debug.Log("Removes playerTransform " + playerTransform.name + " from the camera");
            _cmTargetGroup.RemoveMember(playerTransform);
        }
    }
    public PlayerController GetPlayer(short playerId)
    {
        return _players.Find(x => x.PlayerID == playerId);
    }
    public int GetNrPlayers()
    {
        return _players.Count;
    }
    public List<PlayerController> GetPlayers()
    {
        return _players;
    }

    public void SetRandomSpawnPosition(ref Vector3 currentPosition, bool findNewSpawns = false)
    {
        // If no playerSpawn or asked to, get them
        if (_playerSpawns == null || _playerSpawns[0] == null || findNewSpawns)
        {
            FindPlayerSpawns("");
        }

        // Get randomID, untill is not the same as previous one
        int randomInt = 0;
        do
        {
            randomInt = Random.Range(0, _playerSpawns.Length);
        } while (randomInt == _previousPlayerSpawnID);
        _previousPlayerSpawnID = randomInt;

        currentPosition = _playerSpawns[randomInt].transform.position;
    }

    // Unity game loop
    private void Start()
    {
        // SpawnPoints
        SceneLoader.Instance.SceneLoadedEvent += FindPlayerSpawns;
        FindPlayerSpawns("");

        // Cinemachine
        _cmTargetGroup = GetComponent<CinemachineTargetGroup>();
        Debug.Assert(_cmTargetGroup, "No cinemachine target group found. This will give issues with keeping players on screen.");

        // Setup temp cameraObject
        _tempCameraGameObject = new GameObject("tempCameraGameObject");
        _cmTargetGroup.AddMember(_tempCameraGameObject.transform, 1.0f, _minimumDistPlayerNCamera);
    }

    private void FindPlayerSpawns(string name)
    {
        // Find playerSpawns
        _playerSpawns = GameObject.FindGameObjectsWithTag(_playerSpawnTag);

        // Set playerSpawns
        int idx = 0;
        foreach (PlayerController controller in _players)
        {
            controller.StartPosition = _playerSpawns[idx].transform.position;
            ++idx;
        }
    }

    private short GetNextAvailablePlayerId()
    {
        for (short i = 0; i < 4; ++i)
        {
            var pl = Players.Find(x => x.PlayerID == i);
            if (pl == null)
            {
                return i;
            }
        }
        Debug.LogWarning("GetNextAvailableId did not find an available playerId between 0-3");
        return -1;
    }

    private void OnDestroy()
    {
        // Destroy all players
        foreach (var player in _players)
        {
            Destroy(player);
        }

        // Remove events
        SceneLoader sceneLoader = SceneLoader.Instance;
        if(sceneLoader) sceneLoader.SceneLoadedEvent -= FindPlayerSpawns;
    }
}
