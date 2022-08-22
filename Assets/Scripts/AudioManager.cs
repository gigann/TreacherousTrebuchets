using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

// Client-side singleton AudioManager for managing and playing sound effects and music.

public class AudioManager : MonoBehaviour
{
    public static AudioManager instance;
    [SerializeField] public AudioMixer masterMixer;
    [SerializeField] public AudioSource sfxSource;
    [SerializeField] public AudioSource bgmSource;

    // Lists of sound effects and background music.
    [SerializeField] public List<AudioClip> SFX = new List<AudioClip>();
    [SerializeField] public List<AudioClip> BGM = new List<AudioClip>();

    private void Awake() 
    { 
        if (instance != null && instance != this)
        {
            Destroy(this.gameObject);
        }
        else
        {
            instance = this;
            DontDestroyOnLoad(this.gameObject);
        }
    }

    public void PlaySFX(int index)
    {
        sfxSource.PlayOneShot(SFX[index], sfxSource.volume);
    }

    // Picks and plays a random sound effect between the two indices.
    public void PlaySFX(int startingIndex, int endingIndex)
    {
        sfxSource.PlayOneShot(SFX[Random.Range(startingIndex, endingIndex)], sfxSource.volume);
    }

    public void PlayBGM(int index, bool loop = true)
    {
        bgmSource.clip = BGM[index];
        bgmSource.loop = loop;
        bgmSource.Play();
    }

    public void AdjustSFXVolume(float newVolume)
    {
        masterMixer.SetFloat("volumeSFX", newVolume);
    }

    public void AdjustBGMVolume(float newVolume)
    {
        masterMixer.SetFloat("volumeBGM", newVolume);
    }

    public void AdjustMasterVolume(float newVolume)
    {
        masterMixer.SetFloat("volumeMaster", newVolume);
    }
}