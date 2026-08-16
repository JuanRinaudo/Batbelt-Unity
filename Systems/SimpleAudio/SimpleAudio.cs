using System;
using System.Collections;
using System.Collections.Generic;
using SimpleTweens;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Pool;

[DefaultExecutionOrder(-75)]
public class SimpleAudio : MonoBehaviour
{
    public static bool SoundEnabled { get; private set; } = true;
    public static bool SoundMuted { get; private set; } = false;

    public static SimpleAudio instance;

    [Header("Mixer")]
    public AudioMixer mixer;

    [Header(AttributeConstants.HeaderReferences)]
    public AudioSource baseSfxSource;
    public AudioSource musicSource;
    public AudioSource altMusicSource;

    public Tween _musicTween;
    public Tween _altMusicTween;
    public bool _altMusicPlaying = false;

    List<AudioSource> _activeSFXSources;
    ObjectPool<AudioSource> _sfxPool;

    static float _realMixerMasterVolume;

    const string SIMPLE_INSTANCE_RESOURCE_PATH = "Batbelt/SimpleAudioInstance";
    const string BASE_MIXER_RESOURCE_PATH = "Batbelt/SimpleAudioInstance";
    const string MASTER_VOLUME_NAME = "MasterVolume";
    const string MUSIC_VOLUME_NAME = "MusicVolume";
    const string SFX_VOLUME_NAME = "SFXVolume";
    const string MUSIC_GROUP_NAME = "Music";
    const string SFX_GROUP_NAME = "SFX";

    const float MIN_DB = -80f;
    const float MAX_DB = 0f;

#if UNITY_EDITOR
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void Restart()
    {
        instance = null;
    }
#endif

    public static SimpleAudio CreateAudioSingleton(Transform parent = null)
    {
        Instantiate(Resources.Load<GameObject>(SIMPLE_INSTANCE_RESOURCE_PATH));
        if (parent != null)
            instance.transform.SetParent(parent);
        return instance;
    }

    void Awake()
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
            if (altMusicSource != null && musicGroups.Length > 0) { altMusicSource.outputAudioMixerGroup = musicGroups[0]; }
            if (baseSfxSource != null && sfxGroups.Length > 0) { baseSfxSource.outputAudioMixerGroup = sfxGroups[0]; }
        }

        _activeSFXSources = new List<AudioSource>();
        _sfxPool = new ObjectPool<AudioSource>(() =>
        {
            var instance = Instantiate(baseSfxSource, baseSfxSource.transform.parent);
            return instance;
        }, audioSource =>
        {
            audioSource.gameObject.SetActive(true);
        }, audioSource =>
        {
            audioSource.gameObject.SetActive(false);
            audioSource.clip = null;
        }, audioSource =>
        {
            if (audioSource != null)
                Destroy(audioSource.gameObject);
        }, false, 10, 100);

        instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void PlaySoundBank(SoundBank sound, float volumeModifier = 1.0f)
    {
        var clip = sound.GetRandomClip();
        if (clip == null)
            return;

        var pitch = sound.randomPitch ? sound.GetPitch() : sound.minPitch;

        var sfxSource = _sfxPool.Get();
        sfxSource.pitch = pitch;
        sfxSource.volume = sound.volume * volumeModifier;
        sfxSource.clip = clip;
        sfxSource.Play();

        _activeSFXSources.Add(sfxSource);
    }

    public void PlayAudioClip(AudioClip clip, float volume = 1.0f, float pitch = 1.0f)
    {
        var sfxSource = _sfxPool.Get();
        sfxSource.pitch = pitch;
        sfxSource.volume = volume;
        sfxSource.clip = clip;
        sfxSource.Play();

        _activeSFXSources.Add(sfxSource);
    }

    public void PlayMusic(AudioClip clip, float volume = 1.0f, float transitionDuration = -1f)
    {
        _musicTween.TryCancel();
        _altMusicTween.TryCancel();

        var currentSource = _altMusicPlaying ? altMusicSource : musicSource;
        var targetSource = _altMusicPlaying ? musicSource : altMusicSource;

        if (currentSource.clip == clip)
        {
            if (volume != currentSource.volume)
            {
                if (transitionDuration > 0f)
                    currentSource.TwVolume(volume, transitionDuration, Easer.Linear);
                else
                    currentSource.volume = volume;
            }
            return;
        }

        _altMusicPlaying = !_altMusicPlaying;

        targetSource.loop = true;
        targetSource.clip = clip;
        targetSource.Play();

        if (transitionDuration > 0f)
        {
            currentSource.TwVolume(0f, transitionDuration, Easer.Linear).AddOnComplete(() =>
            {
                currentSource.Stop();
            });

            targetSource.volume = 0f;
            targetSource.TwVolume(volume, transitionDuration, Easer.Linear);
        }
        else
        {
            currentSource.volume = 0;
            targetSource.volume = volume;
        }
    }

    public void StopMusic(float transitionDuration = -1f)
    {
        var audioSource = _altMusicPlaying ? altMusicSource : musicSource;
        _musicTween.TryCancel();
        _altMusicTween.TryCancel();

        if (transitionDuration > 0f)
        {
            _musicTween = audioSource.TwVolume(0f, transitionDuration, Easer.Linear).AddOnComplete(() =>
            {
                audioSource.Stop();
            });
        }
        else
        {
            audioSource.Stop();
        }
    }

    public void SetMusicVolume(float volume = 1.0f, float duration = -1f)
    {
        var audioSource = _altMusicPlaying ? altMusicSource : musicSource;
        _musicTween.TryCancel();
        if (duration > 0f)
            _musicTween = audioSource.TwVolume(volume, duration, Easer.Linear);
        else
            audioSource.volume = volume;
    }

    public AudioClip GetCurrentMusicClip()
    {
        var audioSource = _altMusicPlaying ? altMusicSource : musicSource;
        return audioSource.clip;
    }

    static float LinearToDb(float linearVolume)
    {
        linearVolume = Mathf.Clamp01(linearVolume);

        if (linearVolume <= 0.0001f)
            return MIN_DB;

        return Mathf.Log10(linearVolume) * 20f;
    }

    static float DbToLinear(float dbVolume)
    {
        if (dbVolume <= MIN_DB)
            return 0f;

        return Mathf.Pow(10f, dbVolume / 20f);
    }

    void InternalSetMasterVolume(float volume)
    {
        if (!SoundEnabled || SoundMuted)
            volume = 0;

        mixer.SetFloat(MASTER_VOLUME_NAME, LinearToDb(volume));
    }

    public void SetMixerMasterVolume(float volume)
    {
        _realMixerMasterVolume = volume;

        InternalSetMasterVolume(volume);
    }

    public void SetMixerMusicVolume(float volume)
    {
        mixer.SetFloat(MUSIC_VOLUME_NAME, LinearToDb(volume));
    }

    public void SetMixerSFXVolume(float volume)
    {
        mixer.SetFloat(SFX_VOLUME_NAME, LinearToDb(volume));
    }

    public float GetMixerMasterVolume()
    {
        float volume;
        return mixer.GetFloat(MASTER_VOLUME_NAME, out volume) ? DbToLinear(volume) : 0f;
    }

    public float GetMixerMusicVolume()
    {
        float volume;
        return mixer.GetFloat(MUSIC_VOLUME_NAME, out volume) ? DbToLinear(volume) : 0f;
    }

    public float GetMixerSFXVolume()
    {
        float volume;
        return mixer.GetFloat(SFX_VOLUME_NAME, out volume) ? DbToLinear(volume) : 0f;
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
        if (!SoundMuted)
        {
            SoundMuted = true;
            InternalSetMasterVolume(0);
        }
    }

    public void UnmuteSound()
    {
        if (SoundMuted)
        {
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
        if (!SoundEnabled)
        {
            SoundEnabled = true;
            InternalSetMasterVolume(_realMixerMasterVolume);
        }
    }

    public void DisableSound()
    {
        if (SoundEnabled)
        {
            SoundEnabled = false;
            InternalSetMasterVolume(0);
        }
    }

    void Update()
    {
        for (int index = _activeSFXSources.Count - 1; index >= 0; index--)
        {
            var activeSFX = _activeSFXSources[index];
            if (!activeSFX.isPlaying)
            {
                _activeSFXSources.RemoveAt(index);
                _sfxPool.Release(activeSFX);
            }
        }
    }
}