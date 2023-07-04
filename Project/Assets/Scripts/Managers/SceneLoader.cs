using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Object = UnityEngine.Object;

public class SceneLoader : Singleton<SceneLoader>
{
    // Variables
    public event Action<string> SceneLoadedEvent;
    private bool _isLoading = false;

    private string _titleScreenName = "TitleScreenScene";
    private string _gameSystemTag = "GameSystem";

    private float _fadeSpeed = 1.0f;

    private string _previousSceneName;
    public string PreviousSceneName 
    { 
        get
        {
            return _previousSceneName;
        }
        private set
        {
            string p = _previousSceneName;
            _previousSceneName = value;
            Debug.Log($"PreviousScene changing from {p} to {_previousSceneName}");
        }
    }

    // Loading
    // -------
    public void LoadScene(string sceneName)
    {
        if (_isLoading) return;
        _isLoading = true;
        StartCoroutine(LoadScene_Coroutine(sceneName));
    }

    private IEnumerator LoadScene_Coroutine(string sceneName)
    {
        // Get fadingScreen
        GameObject fadingScreen = Instantiate(Resources.Load<GameObject>("FadingScreen_Variant"));
        Image fadeImage = fadingScreen.GetComponentInChildren<Image>();

        PreviousSceneName = SceneManager.GetActiveScene().name;
        yield return FadeOut(fadeImage);

        // Get loading screen
        GameObject loadingScreen = Instantiate(Resources.Load<GameObject>("LoadingScreen_Variant"));
        LoadingScreen loadingScreenComp = loadingScreen.GetComponent<LoadingScreen>();
        Debug.Assert(loadingScreenComp != null, "No loadingscreen script found on the loadingscreen obj");

        yield return FadeIn(fadeImage);

        // Start operation
        AsyncOperation loadOperation = SceneManager.LoadSceneAsync(sceneName);

        // If going to titleScreen
        if (sceneName == _titleScreenName)
        {
            // Go through all gameObject
            foreach (GameObject o in Object.FindObjectsOfType<GameObject>())
            {
                // If contains gameSystem tag, destroy
                if (o.tag == _gameSystemTag) Destroy(o);
            }
        }

        // While still loading
        float progressValue;
        while (loadOperation.isDone == false)
        {
            progressValue = Mathf.Clamp01(loadOperation.progress / 0.9f);
            loadingScreenComp.LoadingScreenBar.fillAmount = progressValue;

            yield return null;
        }


        yield return FadeOut(fadeImage);

        Destroy(loadingScreen);
        yield return FadeIn(fadeImage);

        Destroy(fadingScreen);

        _isLoading = false;
        SceneLoadedEvent?.Invoke(sceneName);
    }


    private IEnumerator FadeOut(Image fadeImage)
    {
        Debug.Log("Is fading out fadeScreen");

        var color = fadeImage.color;
        while (fadeImage.color.a < 1.0f)
        {
            color.a += Time.unscaledDeltaTime * _fadeSpeed;
            fadeImage.color = color;
            yield return null;
        }
    }
    private IEnumerator FadeIn(Image fadeImage)
    {
        Debug.Log("Is fading in fadeScreen");

        var color = fadeImage.color;
        while (fadeImage.color.a > 0.0f)
        {
            color.a -= Time.unscaledDeltaTime * _fadeSpeed;
            fadeImage.color = color;
            yield return null;
        }
    }
}
