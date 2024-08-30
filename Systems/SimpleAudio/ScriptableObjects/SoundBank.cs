using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "SoundBank", menuName = "Batbelt/Data/SoundBank")]
public class SoundBank : ScriptableObject
{

    public AudioClip[] clips;
    public float volume = 1;

    public AudioClip GetRandomClip()
    {
        return clips[Random.Range(0, clips.Length)];
    }

}
