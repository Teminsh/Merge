using System.Collections;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public AudioClip[] audioClips;
    private AudioSource audioSource;

    [Range(0f, 1f)]  // This attribute adds a slider in the inspector
    public float volume = 1f;  // Volume control with a default value of 1

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        audioSource.volume = volume;  // Set the initial volume
        PlayRandomClip();
    }

    void Update()
    {
        if (!audioSource.isPlaying)
        {
            PlayRandomClip();
        }
    }

    void PlayRandomClip()
    {
        if (audioClips.Length == 0) return;

        int randomIndex = Random.Range(0, audioClips.Length);
        audioSource.clip = audioClips[randomIndex];
        audioSource.Play();
    }

    public void SetVolume(float volume)
    {
        this.volume = volume;  // Update the volume property
        audioSource.volume = volume;  // Set the volume of the audio source
    }
}