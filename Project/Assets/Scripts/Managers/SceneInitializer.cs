using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneInitializer : MonoBehaviour
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void OnSceneLoaded()
    {
        // If not in the titleScreen
        if (SceneManager.GetActiveScene().name != "TitleScreenScene")
        {
            // Instantiate gameSystem
            Instantiate(Resources.Load<GameObject>("GameSystem_Variant"));
        }
    }
}
