using UnityEngine;

public class PlayRandomClipPoint : MonoBehaviour
{
    [SerializeField] private AudioClip _clip;

    public void Play(AudioSource source)
    {
        source.clip = _clip;

        source.time = Random.Range(0f, _clip.length);
        source.Play();
    }
}
