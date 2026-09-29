using System;

public static class DoubleExtensions
{
    public static bool Approximately(this double a, double b, double epsilon = 1e-15)
    {
        if (a == b) return true;
        
        return Math.Abs(a - b) < epsilon * Math.Max(Math.Abs(a), Math.Abs(b));
    }
}
