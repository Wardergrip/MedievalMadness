using UnityEngine;

public class ScalePulser : MonoBehaviour
{
    [SerializeField] private RectTransform _targetRectTransform;
    [SerializeField] private float _pulseDuration = 1f;
    [SerializeField] private float _minScale = 0.8f;
    [SerializeField] private float _maxScale = 1.2f;
    [Tooltip("If false, the transition is determined by 'Mathf.PingPong'")]
    [SerializeField] private bool _useSine = true;

    private Vector3 _initialScale;
    private float _timer = 0f;

    void Start()
    {
        _initialScale = _targetRectTransform.localScale;
    }

    void Update()
    {
        // Increase the timer by the time passed since the last frame
        _timer += Time.unscaledDeltaTime;

        // Calculate the current scale based on the timer and desired scale range
        float t;
        float scale;
        if (_useSine)
        {
            t = Mathf.Sin(_timer / _pulseDuration * (2 * Mathf.PI));
            scale = Mathf.Lerp(_minScale, _maxScale, (t + 1) / 2);
        }
        else
        {
            t = Mathf.PingPong(_timer, _pulseDuration) / _pulseDuration;
            scale = Mathf.Lerp(_minScale, _maxScale, t);
        }
        // Apply the scale to the UI element
        _targetRectTransform.localScale = _initialScale * scale;
    }
}
