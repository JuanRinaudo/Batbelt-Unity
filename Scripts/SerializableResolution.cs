using System;
using UnityEngine;

[Serializable]
public class SerializableResolution
{
    public int width;
    public int height;
    public uint refreshRateDenominator;
    public uint refreshRateNumerator;

    public RefreshRate refreshRateRatio {
        get {
            var refreshRate = new RefreshRate();
            refreshRate.denominator = refreshRateDenominator;
            refreshRate.numerator = refreshRateNumerator;
            return refreshRate;
        }
    }

    public SerializableResolution(Resolution r)
    {
        width = r.width;
        height = r.height;
        refreshRateDenominator = r.refreshRateRatio.denominator;
        refreshRateNumerator = r.refreshRateRatio.numerator;
    }

    public static explicit operator Resolution(SerializableResolution r)
    {
        var refreshRate = new RefreshRate();
        refreshRate.denominator = r.refreshRateDenominator;
        refreshRate.numerator = r.refreshRateNumerator;
        var resolution = new Resolution();
        resolution.width = r.width;
        resolution.height = r.height;
        return resolution;
    }
}