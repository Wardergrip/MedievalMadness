using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(BoxCollider))]
public class OpenHatchTimer : MonoBehaviour
{
    [Header("Settings")]
    [Tooltip("How long it will take before the shutter opens after he stands inside the collider")]
    [SerializeField] private float _timeOpen = 5.0f;
    [Tooltip("How long it will take before the shutter closes again after opening")]
    [SerializeField] private float _timeClose = 5.0f;
    [SerializeField] private GameObject _hatch;

    [Header("View variables")]
    [SerializeField] private float _currentTimeBeforeOpening;

    // Timer
    private bool _startCountdown = false;

    // Player
    private int _playerIdx = 0;

    // Start
    // -----
    void Start()
    {
        // Assert
        Debug.Assert(_hatch != null, "OpenHatchTimer needs hatch");

        // Set timer to max
        _currentTimeBeforeOpening = _timeOpen;
    }

    // Update
    // ------
    void Update()
    {
        // Countdown
        if (_startCountdown)
        {
            // Start game if reaches 0
            _currentTimeBeforeOpening -= Time.deltaTime;
            if (_currentTimeBeforeOpening < 0) OpenHatch();
        }
    }

    // On Game Start
    // -------------
    private void OpenHatch()
    {
        // Open hatch
        _hatch.SetActive(false);
        StartCoroutine(CloseHatch());
    }
    private IEnumerator CloseHatch()
    {
        yield return new WaitForSeconds(_timeClose);

        // Close hatch
        _hatch.SetActive(true);
    }

    // On Collision
    // ------------
    private void OnTriggerEnter(Collider other)
    {
        // Try to get smashHealth, is player when it has the script
        SmashHealth otherHealth = other.gameObject.GetComponentInParent<SmashHealth>();
        if (otherHealth == null)
        {
            otherHealth = other.gameObject.GetComponent<SmashHealth>();
            if (otherHealth == null)
            {
                Debug.Log("Other did not have smashHealth");
                return;
            }
        }

        // Add player
        ++_playerIdx;

        // Start countdown
        _startCountdown = true;
    }

    private void OnTriggerExit(Collider other)
    {
        // Try to get smashHealth, is player when it has the script
        SmashHealth otherHealth = other.gameObject.GetComponentInParent<SmashHealth>();
        if (otherHealth == null)
        {
            otherHealth = other.gameObject.GetComponent<SmashHealth>();
            if (otherHealth == null)
            {
                Debug.Log("Other did not have smashHealth");
                return;
            }
        }

        // Remove player
        --_playerIdx;

        // If no-one left on hatch
        if (_playerIdx <= 0)
        {
            // End countdown and set to max
            _startCountdown = false;
            _currentTimeBeforeOpening = _timeOpen;
        }
    }
}
