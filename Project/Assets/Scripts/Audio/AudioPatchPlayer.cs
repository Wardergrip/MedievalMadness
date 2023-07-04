using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioPatchPlayer : MonoBehaviour
{
    [Header("Play")]
    [SerializeField] private bool _play = false;
    [Header("Audio")]
    [SerializeField] private AudioPatch _patch;

    [Header("AudioSource")]
    [SerializeField] private AudioSource _source;

    private void Update()
    {
        if (_play)
        {
            _play = false;
            if (_source == null)
            {
                _source = gameObject.AddComponent<AudioSource>();
            }
            if (_patch == null)
            {
                Debug.LogWarning("No audio patch assigned");
                return;
            }
            _patch.Play(_source);
        }
    }
}
