using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "SoundBank", menuName = "Batbelt/Data/SoundBank")]
public class SoundBank : ScriptableObject
{

    public AudioClip[] clips;
    public float volume = 1;

    [Space]
    public bool randomPitch = false; 
    public float minPitch = 1;
    public float maxPitch = 1;
    
    public AudioClip GetRandomClip()
    {
        return clips[Random.Range(0, clips.Length)];
    }

    public float GetPitch()
    {
        return Random.Range(minPitch, maxPitch);
    }

}
