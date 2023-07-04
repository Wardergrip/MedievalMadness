using UnityEngine;

public class HideCursor : MonoBehaviour
{
    // Start
    // -----
    void Start()
    {
        Cursor.visible = false;
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void OnSceneLoaded()
    {        
        // Instantiate cursor object
        Instantiate(Resources.Load<GameObject>("HideCursor_Variant"));
    }
}
