using System.Collections;
using UnityEngine;

public class PlayCooldown : MonoBehaviour
{
    [SerializeField] private AudioSource _audioSource;
    [SerializeField] private float _cooldown = 5.0f;
    private bool _onCooldown = false;
    private Coroutine _resetCoroutine = null;

    public void Play(AudioPatch audioPatch)
    {
        if (_onCooldown) return;
        _onCooldown = true;
        audioPatch.Play(_audioSource);
        _resetCoroutine = StartCoroutine(ResetCooldown_Coroutine());
    }

    public void PlayOneShot(AudioPatch audioPatch)
    {
        if (_onCooldown) return;
        _onCooldown = true;
        audioPatch.PlayOneShot(_audioSource);
        _resetCoroutine = StartCoroutine(ResetCooldown_Coroutine());
    }

    private IEnumerator ResetCooldown_Coroutine()
    {
        yield return new WaitForSeconds(_cooldown);
        _onCooldown = false;
        _resetCoroutine = null;
    }

    private void OnDestroy()
    {
        if (_resetCoroutine != null) StopCoroutine(_resetCoroutine);
    }
}
