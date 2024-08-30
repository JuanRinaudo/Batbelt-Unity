using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

[DefaultExecutionOrder(-75)]
public class SimpleAudio : MonoBehaviour
{
    public static bool soundEnabled = false;

    public static SimpleAudio instance;

    [Header("Mixer")]
    public AudioMixer mixer;

    [Header(AttributeConstants.HeaderReferences)]
    public AudioSource sfxSource;
    public AudioSource musicSource;

    const string SIMPLE_INSTANCE_RESOURCE_PATH = "Batbelt/SimpleAudioInstance";
    const string BASE_MIXER_RESOURCE_PATH = "Batbelt/SimpleAudioInstance";
    const string MASTER_VOLUME_NAME = "MasterVolume";
    const string MUSIC_VOLUME_NAME = "MusicVolume";
    const string SFX_VOLUME_NAME = "SFXVolume";
    const string MUSIC_GROUP_NAME = "Music";
    const string SFX_GROUP_NAME = "SFX";

    public static SimpleAudio CreateAudioSingleton()
    {
        Instantiate(Resources.Load<GameObject>(SIMPLE_INSTANCE_RESOURCE_PATH));
        return instance;
    }

    private void Awake()
    {
        if (instance != null)
        {
            Destroy(instance.gameObject);
            return;
        }

        if (mixer == null)
        {
            mixer = Resources.Load<AudioMixer>(BASE_MIXER_RESOURCE_PATH);

            AudioMixerGroup[] musicGroups = mixer.FindMatchingGroups(MUSIC_GROUP_NAME);
            AudioMixerGroup[] sfxGroups = mixer.FindMatchingGroups(SFX_GROUP_NAME);

            if (musicSource != null && musicGroups.Length > 0) { musicSource.outputAudioMixerGroup = musicGroups[0]; }
            if (sfxSource != null && sfxGroups.Length > 0) { sfxSource.outputAudioMixerGroup = sfxGroups[0]; }
        }

        if (soundEnabled)
        {
            SoundOn();
        }
        else
        {
            SoundOff();
        }

        instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void PlaySoundBank(SoundBank sound, float volumeModifier = 1.0f)
    {
        sfxSource.PlayOneShot(sound.GetRandomClip(), sound.volume * volumeModifier);
    }

    public void PlayAudioClip(AudioClip clip, float volume = 1.0f)
    {
        sfxSource.PlayOneShot(clip, volume);
    }

    public void PlayMusic(AudioClip clip, float volume = 1.0f)
    {
        musicSource.loop = true;
        musicSource.clip = clip;
        musicSource.volume = volume;
        musicSource.Play();
    }

    public void SetMasterVolume(float volume)
    {
        mixer.SetFloat(MASTER_VOLUME_NAME, volume * 100f - 80f);
    }

    public void SetMusicVolume(float volume)
    {
        mixer.SetFloat(MUSIC_VOLUME_NAME, volume * 100f - 80f);
    }

    public void SetSFXVolume(float volume)
    {
        mixer.SetFloat(SFX_VOLUME_NAME, volume * 100f - 80f);
    }

    public float GetMasterVolume()
    {
        float volume;
        return mixer.GetFloat(MASTER_VOLUME_NAME, out volume) ? ((volume + 80f) / 100f) : 0;
    }

    public float GetMusicVolume()
    {
        float volume;
        return mixer.GetFloat(MUSIC_VOLUME_NAME, out volume) ? ((volume + 80f) / 100f) : 0;
    }

    public float GetSFXVolume()
    {
        float volume;
        return mixer.GetFloat(SFX_VOLUME_NAME, out volume) ? ((volume + 80f) / 100f) : 0;
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
        SetMasterVolume(1);
    }

    public void SoundOff()
    {
        soundEnabled = false;
        SetMasterVolume(0);
    }
}