using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AdaptableFlags : MonoBehaviour
{
    [Header("Flag materials")]
    [Tooltip("The material the flag will start on")]
    [SerializeField] private int _startMaterialID = 0;

    [Header("Settings")]
    [SerializeField] private bool _showWinningPlayerOnLoad;
    [Tooltip("Disable if you don't want the flag to automatically change colors")]
    [SerializeField] private bool _loopFlag = true;
    [Tooltip("How long it takes before the flag starts changing")]
    [SerializeField] private float _startTime = 10.0f;
    [Tooltip("How long it takes before it checks whether to swap flag colors")]
    [SerializeField] private float _timeBetweenFlagSwap = 3.0f;
    [Tooltip("Color swap speed between flags")]
    [SerializeField] private float _colorSwapSpeed = 1.0f;

    // Flag
    private MeshRenderer[] _flagRenderers;
    private string _flagChangeMethodName = "ChangeMethod";

    // Shader variables
    private string _currentTextureName = "_current_tex_index";
    private string _desiredTextureName = "_blend_tex_index";
    private string _blendName = "_blend";

    // Scenes
    private string _mainMenuName = "MainMenuScene";
    private string _gameSceneName = "GameScene";

    // Start
    // -----
    void Start()
    {
        // Get materials
        _flagRenderers = GetComponentsInChildren<MeshRenderer>();

        // Set startMaterial
        SetFlagMaterial(_startMaterialID);

        // Subscribe to events if announcing player on load
        if (_showWinningPlayerOnLoad)
        {
            SceneLoader.Instance.SceneLoadedEvent += SceneLoadedEvent;
        }

        // Start loop
        if (_loopFlag) InvokeRepeating(_flagChangeMethodName, _startTime, _timeBetweenFlagSwap);
    }
    private void OnDestroy()
    {
        if (_showWinningPlayerOnLoad)
        {
            SceneLoader loader = SceneLoader.Instance;
            if(loader) loader.SceneLoadedEvent-= SceneLoadedEvent;
        }
    }

    private void SceneLoadedEvent(string sceneName)
    {
        // If going from game to main menu
        if (sceneName == _mainMenuName && SceneLoader.Instance.PreviousSceneName == _gameSceneName)
        {
            ChangeMethod();
        }
    }

    private void ChangeMethod()
    {
        // Check which player is currently winning
        short? winningPlayerID = GameSystem.Instance.ScoreManager.GetWinningPlayerId();
        if (winningPlayerID == null) return;

        // Get player color and set flag material
        GameSystem gameSystem = GameSystem.Instance;
        if(gameSystem == null) return;

        PlayerManager playerManager = gameSystem.PlayerManager;
        if (playerManager == null) return;

        PlayerController winningPlayer = playerManager.GetPlayer(winningPlayerID.Value);
        if (winningPlayer == null || winningPlayer.PlayerPawn == null) return;

        SetFlagMaterial((int)winningPlayer.PlayerPawn.PawnColor);
    }
    private void SetFlagMaterial(int materialID)
    {
        // Hard-coded values, since the shader values don't add up with the actual color values
        switch (materialID)
        {
            case 0:
                materialID = 2;
            break;

            case 1:
                materialID = 0;
            break;

            case 2:
                materialID = 1;
            break;

            case 3:
                materialID = 3;
            break;
        }

        // Prepare each renderer
        float currentTextureID = 0;
        foreach (var renderer in _flagRenderers)
        {
            // Set desired to current texture
            currentTextureID = renderer.material.GetFloat(_desiredTextureName);
            renderer.material.SetFloat(_currentTextureName, currentTextureID); 

            // Set desired texture
            renderer.material.SetFloat(_desiredTextureName, materialID);

            // Set blending
            renderer.material.SetFloat(_blendName, 0.0f);
        }

        // Call method
        StartCoroutine(BlendFlag());
    }

    private IEnumerator BlendFlag()
    {
        // While blend is not fully done
        float currentBlend = 0.0f;
        while (currentBlend <= 1.0f)
        {
            // Add blending
            currentBlend += _colorSwapSpeed * Time.deltaTime;
            foreach (var renderer in _flagRenderers)
            {
                // Set blending
                renderer.material.SetFloat(_blendName, currentBlend);
            }

            // Yield return
            yield return null;
        }
    }
}
