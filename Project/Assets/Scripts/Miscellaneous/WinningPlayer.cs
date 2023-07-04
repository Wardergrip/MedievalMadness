using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public class WinningPlayer : MonoBehaviour
{
    public UnityEvent OnStart;
    public PlayerPawn WinningPlayerPawn { get; set; }
    void Start()
    {
        SceneManager.activeSceneChanged += SceneChanged;
        OnStart?.Invoke();
    }

    private void OnDestroy()
    {
        SceneManager.activeSceneChanged -= SceneChanged;
    }

    private void SceneChanged(Scene prevScene, Scene newScene)
    {
        if (newScene.name == "GameScene")
        {
            Destroy(gameObject);
        }
    }
}
