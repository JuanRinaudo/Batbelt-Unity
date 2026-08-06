using System.Collections;
using System.Collections.Generic;
using SimpleTweens;
using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu(fileName = "SoundBank", menuName = "Batbelt/Data/SoundBank")]
public class SoundBank : ScriptableObject
{
    public enum AudioSelectType
    {
        Random,
        Sequential,
    }

    public AudioClip[] clips;
    public float volume = 1;
    
    public AudioSelectType SelectType = AudioSelectType.Random;

    [Space]
    public bool randomPitch = false; 
    public float minPitch = 1;
    public float maxPitch = 1;

    public float Delay;

    int _index = 0;
    
    public AudioClip GetRandomClip()
    {
        if(clips.Length == 0)
            return null;
        
        if (SelectType == AudioSelectType.Sequential)
        {
            var clip = clips[_index];
            _index = (_index + 1) % clips.Length;
            return clip;
        }
        else
        {
            return clips[Random.Range(0, clips.Length)];
        }
    }

    public float GetPitch()
    {
        return Random.Range(minPitch, maxPitch);
    }

    public void Play()
    {
        if(Delay <= 0)
            SimpleAudio.instance.PlaySoundBank(this);
        else
            TweenManager.Instance.DelayedCall(Delay, () => SimpleAudio.instance.PlaySoundBank(this), this);
    }
}
