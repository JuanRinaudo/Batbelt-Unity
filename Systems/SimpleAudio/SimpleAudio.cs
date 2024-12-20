﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

[DefaultExecutionOrder(-75)]
public class SimpleAudio : MonoBehaviour
{
    public static bool SoundEnabled { get; private set; } = false;
    public static bool SoundMuted { get; private set; } = false;

    public static SimpleAudio instance;

    [Header("Mixer")]
    public AudioMixer mixer;

    [Header(AttributeConstants.HeaderReferences)]
    public AudioSource sfxSource;
    public AudioSource musicSource;

    static float _realMixerMasterVolume;

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
    
    public void SetMusicVolume(float volume = 1.0f)
    {
        musicSource.volume = volume;
    }

    void InternalSetMasterVolume(float volume)
    {
        if(!SoundEnabled || SoundMuted)
            volume = 0;

        mixer.SetFloat(MASTER_VOLUME_NAME, volume * 80f - 80f);
    }

    public void SetMixerMasterVolume(float volume)
    {
        _realMixerMasterVolume = volume;

        InternalSetMasterVolume(volume);
    }

    public void SetMixerMusicVolume(float volume)
    {
        mixer.SetFloat(MUSIC_VOLUME_NAME, volume * 80f - 80f);
    }

    public void SetMixerSFXVolume(float volume)
    {
        mixer.SetFloat(SFX_VOLUME_NAME, volume * 80f - 80f);
    }

    public float GetMixerMasterVolume()
    {
        float volume;
        return mixer.GetFloat(MASTER_VOLUME_NAME, out volume) ? ((volume + 80f) / 80f) : 0;
    }

    public float GetMixerMusicVolume()
    {
        float volume;
        return mixer.GetFloat(MUSIC_VOLUME_NAME, out volume) ? ((volume + 80f) / 80f) : 0;
    }

    public float GetMixerSFXVolume()
    {
        float volume;
        return mixer.GetFloat(SFX_VOLUME_NAME, out volume) ? ((volume + 80f) / 80f) : 0;
    }

    public bool ToggleMute()
    {
        if (SoundMuted)
            UnmuteSound();
        else
            MuteSound();

        return SoundMuted;
    }

    public void MuteSound()
    {
        if(!SoundMuted) {
            SoundMuted = true;
            InternalSetMasterVolume(0);
        }
    }

    public void UnmuteSound()
    {
        if(SoundMuted) {
            SoundMuted = false;
            InternalSetMasterVolume(_realMixerMasterVolume);
        }
    }

    public bool ToggleEnabled()
    {
        if (SoundEnabled)
            EnableSound();
        else
            DisableSound();

        return SoundEnabled;
    }

    public void EnableSound()
    {
        if(!SoundEnabled) {
            SoundEnabled = true;
            InternalSetMasterVolume(_realMixerMasterVolume);
        }
    }
 
    public void DisableSound()
    {
        if(SoundEnabled) { 
            SoundEnabled = false;
            InternalSetMasterVolume(0);
        }
    }
}