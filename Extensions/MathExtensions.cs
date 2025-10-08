using UnityEngine;

public static class MathExtensions
{

    public static Vector2 ToVector2XY(this Vector3 value)
    {
        return new Vector2(value.x, value.y);
    }

    public static Vector3 ToVector3XY(this Vector2 value, float z = 0)
    {
        return new Vector3(value.x, value.y, z);
    }

}
