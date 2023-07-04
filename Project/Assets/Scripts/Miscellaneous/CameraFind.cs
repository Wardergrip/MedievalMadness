using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFind : MonoBehaviour
{
    private string _cameraTag = "MainCamera";
    private string _resourceCameraName = "BaseCamera";

    // Start
    // -----
    void Awake()
    {
        // Find camera
        GameObject foundCamera = GameObject.FindGameObjectWithTag(_cameraTag);
        
        // If not found one
        if (foundCamera == null)
        {
            // Get the one from resources
            foundCamera = Instantiate(Resources.Load(_resourceCameraName)) as GameObject;
            foundCamera.transform.position = transform.position;
        }
        else
        {
            // If there are multiple audioListeners
            AudioListener[] auioListeners = FindObjectsOfType<AudioListener>();
            if (auioListeners.Length > 1)
            {
                // Remove audioListener
                AudioListener listener = foundCamera.GetComponent<AudioListener>();
                if (listener) Destroy(listener);
            }

            // Check if has screenShake
            ScreenShake shakeScript = foundCamera.GetComponent<ScreenShake>();
            Debug.Assert(shakeScript != null, "Error: SceneCamera doesn't have ScreenShake script");
        }

        // Attach as child and add component
        foundCamera.transform.parent = transform;
    }
}
