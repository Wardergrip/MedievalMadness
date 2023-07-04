using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScreenShake : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private AnimationCurve _animationCurve;

    // Shake settings
    private Vector3 _startTranslation;
    private float _shakeMultiplier = 1.0f;
    private bool _isShaking;

    private float _shakeDuration;
    private float _elapsedTime;

    // Start
    // -----
    private void Start()
    {
        StartCoroutine(LateStart(0.5f));
    }
    private IEnumerator LateStart(float seconds)
    {
        yield return new WaitForSeconds(seconds);
        _startTranslation = transform.position - transform.parent.position;
    }

    // Shake
    // -----
    public void StartShake(float shakeDuration, float shakeMultiplier = 1.0f)
    {
        _shakeDuration = shakeDuration;
        _shakeMultiplier = shakeMultiplier;
        _isShaking = true;

        _elapsedTime = 0;
    }

    public void Update()
    {
        // Return if not shaking
        if (_isShaking == false) return;

        // Shake
        Vector3 startPosition = transform.parent.position + _startTranslation;
        float animationStrength = _animationCurve.Evaluate(_elapsedTime / _shakeDuration);
        transform.position = startPosition + Random.insideUnitSphere * animationStrength * _shakeMultiplier;

        // If elapsedTime bigger then duration
        _elapsedTime += Time.deltaTime;
        if (_shakeDuration < _elapsedTime)
        {
            // Reset camera
            _isShaking = false;
            transform.position = startPosition;
        }
    }
}
