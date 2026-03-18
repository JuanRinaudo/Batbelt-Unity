using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ImageSyncColors : MonoBehaviour
{
    public Image Target;
    public Image SyncTarget;

    void Update()
    {
        if(Target.color != SyncTarget.color)
            Target.color = SyncTarget.color;
    }
}
