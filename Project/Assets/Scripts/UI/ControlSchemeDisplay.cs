using System.Collections;
using UnityEngine;

public class ControlSchemeDisplay : MonoBehaviour
{
    [SerializeField] private GameObject _canvas;
    private RectTransform _panelRectTransform;
    private Vector3 _originalPanelScale;
    [SerializeField] private float _scaleDuration = 1.0f;
    private GameObject Canvas 
    { 
        get 
        {
            if (_canvas == null)
            {
                _canvas = transform.GetChild(0).gameObject;
            }
            return _canvas; 
        } 
    }
    private bool _hasBeenShowed = false;

    private const string MAIN_MENU_SCENE_NAME = "MainMenuScene";
    private const string GAME_SCENE_SCENE_NAME = "GameScene";
    private const string TITLE_SCREEN_SCENE_NAME = "TitleScreenScene";

    void Start()
    {
        Debug.Assert(Canvas, "Canvas isn't assigned or not found");
        _panelRectTransform = Canvas.transform.GetChild(0).GetComponent<RectTransform>();
        Debug.Assert(_panelRectTransform, "No recttransform found on the child of the canvas");
        _originalPanelScale = _panelRectTransform.localScale;
        Canvas.SetActive(false);
        SceneLoader.Instance.SceneLoadedEvent += SceneLoadedEvent;
    }

    private void SceneLoadedEvent(string sceneName)
    {
        /*if (sceneName == GAME_SCENE_SCENE_NAME) 
        {
            _hasBeenShowed = false;
        }
        else*/
        if (sceneName == MAIN_MENU_SCENE_NAME && SceneLoader.Instance.PreviousSceneName == TITLE_SCREEN_SCENE_NAME)
        {            
            if (_hasBeenShowed) return;
            _hasBeenShowed = true;
            Show();
        }
    }
    private void OnDestroy()
    {
        var sceneLoader = SceneLoader.Instance;
        if (sceneLoader) sceneLoader.SceneLoadedEvent -= SceneLoadedEvent;
    }

    private void Show()
    {
        Time.timeScale = 0.0f;
        StartCoroutine(Show_Coroutine());
    }

    public void Hide()
    {
        Time.timeScale = 1.0f;
        StartCoroutine(Hide_Coroutine());
    }

    private IEnumerator Show_Coroutine()
    {
        Canvas.SetActive(true);
        _panelRectTransform.localScale = Vector3.zero;

        float elapsedTime = 0f;

        while (elapsedTime < _scaleDuration)
        {
            float t = EaseInOut(elapsedTime / _scaleDuration);

            _panelRectTransform.localScale = Vector3.Lerp(Vector3.zero, _originalPanelScale, t);

            elapsedTime += Time.unscaledDeltaTime;
            yield return null;
        }

        _panelRectTransform.localScale = _originalPanelScale;
    }

    private IEnumerator Hide_Coroutine()
    {
        float elapsedTime = 0f;
        while (elapsedTime < _scaleDuration)
        {
            float t = EaseInOut(elapsedTime / _scaleDuration);

            _panelRectTransform.localScale = Vector3.Lerp(_originalPanelScale, Vector3.zero, t);

            elapsedTime += Time.unscaledDeltaTime;
            yield return null;
        }

        Canvas.SetActive(false);
        _panelRectTransform.localScale = _originalPanelScale;
    }

    private float EaseInOut(float t)
    {
        return t < 0.5f ? 4f * t * t * t : (t - 1f) * (2f * t - 2f) * (2f * t - 2f) + 1f;
    }
}
