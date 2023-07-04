using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MusicFader : MonoBehaviour
{
    [SerializeField] private AudioSource _audioSource;
    [SerializeField] private float _fadeSpeed = 0.0005f;
    [SerializeField] private float _maxVolume = 0.5f;
    private Coroutine _coroutine = null;

    void Start()
    {
        _coroutine = StartCoroutine(FadeIn_Coroutine());
    }

    public void FadeOut()
    {
        if (_coroutine != null) StopCoroutine(_coroutine);
        _coroutine = StartCoroutine(FadeOut_Coroutine());
    }

    private IEnumerator FadeOut_Coroutine()
    {
        while (_audioSource.volume > 0)
        {
            _audioSource.volume -= _fadeSpeed * Time.deltaTime;
            yield return null;
        }
        _coroutine = null;
    }

    private IEnumerator FadeIn_Coroutine()
    {
        _audioSource.volume = 0;
        while (_audioSource.volume < _maxVolume)
        {
            _audioSource.volume += _fadeSpeed * Time.deltaTime;
            yield return null;
        }
        _coroutine = null;
    }

    private void OnDestroy()
    {
        if (_coroutine != null) StopCoroutine(_coroutine);
    }
}
