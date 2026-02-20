using System;
using SimpleTweens;

[Serializable]
public struct TweenAnimationData<T>
{
    public float Duration;
    public float Delay;
    public T Value;
    public Ease Ease;
}