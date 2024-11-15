using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public AudioSource defaultAudioSource;

    [Header("UI")]
    public AudioClip buttonHover;
    public AudioClip buttonClick;

    [Header("Player")]
    public AudioClip[] playerHit;
    public AudioClip[] playerDeath;

    private float minRandomPitch = 0.9f;
    private float maxRandomPitch = 1.2f;

    // Instance
    public static AudioManager inst;

    void Awake ()
    {
        #region Singleton

        // If the instance already exists, destroy this one.
        if(inst != this && inst != null)
        {
            Destroy(gameObject);
            return;
        }

        // Set the instance to this script.
        inst = this;

        #endregion
    }

    // Play a sound effect through the default audio source.
    public void Play (AudioClip sfx)
    {
        defaultAudioSource.PlayOneShot(sfx);
    }

    // Play a sound effect with a random pitch change.
    public void Play (AudioSource audioSource, AudioClip sfx, bool randomPitch = false)
    {
        audioSource.pitch = randomPitch ? Random.Range(minRandomPitch, maxRandomPitch) : 1.0f;
        audioSource.PlayOneShot(sfx);
    }

    // Play a random sound effect with a random pitch change.
    public void Play (AudioSource audioSource, AudioClip[] sfxArray, bool randomPitch = false)
    {
        audioSource.pitch = randomPitch ? Random.Range(minRandomPitch, maxRandomPitch) : 1.0f;
        audioSource.PlayOneShot(sfxArray[Random.Range(0, sfxArray.Length)]);
    }
}