using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TitleScreen : MonoBehaviour
{
    [SerializeField] private GameObject _titleScreenObject;

    // Start
    // -----
    private void Start()
    {
       Debug.Assert(_titleScreenObject != null, "No eventManager given");
    }

    // On button press
    // ---------------
    public void OnPlayButtonPress()
    {
        // Delete eventSystem
        Destroy(_titleScreenObject);

        // Start coroutine, to make sure the gameSystem doesn't take over variables of the titleScreen scene
        StartCoroutine(SceneLoad(0.5f));
    }
    private IEnumerator SceneLoad(float seconds)
    {
        yield return new WaitForSeconds(seconds);
        
        // Instantiate gameSystem
        Instantiate(Resources.Load<GameObject>("GameSystem_Variant"));

        // Load scene
        SceneLoader.Instance.LoadScene("MainMenuScene");
    }

    public void OnQuitButtonPress()
    {
        Debug.Log("Game quit");
        Application.Quit();
    }
}
