using System.Collections;
using UnityEngine;

public class HelmetShake : MonoBehaviour
{
    [SerializeField] private RectTransform _rectTransform;
    [SerializeField] private float _shakeDuration = 0.1f;
    [SerializeField] private float _shakeIntensity = 10.0f;
    private Vector3 _originalPosition;
    private float _timer;

    // Start is called before the first frame update
    void Start()
    {
        if (_rectTransform == null) _rectTransform = GetComponent<RectTransform>();
        Debug.Assert(_rectTransform, "No Recttransform found");
        _originalPosition = _rectTransform.localPosition;
    }

    public void Shake()
    {
        if (gameObject.activeInHierarchy)
        {
            StartCoroutine(Shake_Coroutine());
        }
    }

    private IEnumerator Shake_Coroutine()
    {
        _timer = 0;
        while (_timer < _shakeDuration) 
        {
            _timer += Time.deltaTime;
            _rectTransform.localPosition = _originalPosition + (Random.insideUnitSphere * _shakeIntensity);
            yield return null;
        }
        _rectTransform.localPosition = _originalPosition;
    }
}
