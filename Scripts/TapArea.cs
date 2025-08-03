using UnityEngine;
using UnityEngine.UI;

public class HitArea : MonoBehaviour
{
    public Image Target;

    void Awake()
    {
        Target.color = Target.color.WithA(0);
    }
}
