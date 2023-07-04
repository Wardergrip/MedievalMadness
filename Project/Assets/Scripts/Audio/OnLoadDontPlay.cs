using UnityEngine;

public class OnLoadDontPlay : MonoBehaviour
{
    public static bool IsLoading {  get; set; }

    [SerializeField] private AudioSource _audioSource;
    
    public void Play(AudioPatch audioPatch)
    {
        if (IsLoading) return;
        audioPatch.Play(_audioSource);
    }

    public void PlayOneShot(AudioPatch audioPatch)
    {
        if (IsLoading) return;
        audioPatch.PlayOneShot(_audioSource);
    }
}
