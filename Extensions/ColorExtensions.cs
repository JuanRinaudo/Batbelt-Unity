using UnityEngine;

public static class ColorExtensions
{
    public static Color WithR(this Color color, float red)
    {
        return new Color(red, color.g, color.b, color.a);
    }

    public static Color WithG(this Color color, float green)
    {
        return new Color(color.r, green, color.b, color.a);
    }

    public static Color WithB(this Color color, float blue)
    {
        return new Color(color.r, color.g, blue, color.a);
    }

    public static Color WithA(this Color color, float alpha)
    {
        return new Color(color.r, color.g, color.b, alpha);
    }
}