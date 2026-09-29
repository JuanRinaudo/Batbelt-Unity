using System;
using UnityEngine;

[Serializable]
public struct TweenAnimationCurveData
{
    public float Duration;
    public float Delay;
    public AnimationCurve Curve;
}

[Serializable]
public struct TweenAnimationCurveData<T>
{
    public float Duration;
    public float Delay;
    public T Value;
    public AnimationCurve Curve;
}