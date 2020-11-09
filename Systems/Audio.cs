using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[DefaultExecutionOrder(-75)]
public class Audio : MonoBehaviour
{
    public static bool soundEnabled = true;

    public static Audio Instance;
    public static float globalVolume = 1;

    [Header("SFX")]
    public AudioSource sfxSource;

    [Header("Music")]
    public AudioSource musicSource;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(Instance.gameObject);
        }

        if (soundEnabled)
        {
            SoundOn();
        }
        else
        {
            SoundOff();
        }

        Instance = this;
    }

    public void PlaySound(Sound sound, float volumeModifier = 1.0f)
    {
        if (soundEnabled)
        {
            sfxSource.PlayOneShot(sound.GetClip(), sound.volume * volumeModifier * globalVolume);
        }
    }

    public void PlaySound(AudioClip clip, float volume = 1.0f)
    {
        if (soundEnabled)
        {
            sfxSource.PlayOneShot(clip, volume * globalVolume);
        }
    }

    public void ToggleAudio()
    {
        if (soundEnabled)
        {
            SoundOff();
        }
        else
        {
            SoundOn();
        }
    }

    public void SoundOn()
    {
        soundEnabled = true;
        musicSource.volume = .35f;
    }

    public void SoundOff()
    {
        soundEnabled = false;
        musicSource.volume = 0;
    }
}