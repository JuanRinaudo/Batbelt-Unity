using UnityEngine;

public static class CameraExtensions
{
    public static Vector2 GetCameraWorldSize(this Camera camera)
    {
        var topRight = camera.ViewportToWorldPoint(new Vector3(1, 1, 0));
        var bottomLeft = camera.ViewportToWorldPoint(new Vector3(0, 0, 0));
        var width = topRight.x - bottomLeft.x;
        var height = topRight.y - bottomLeft.y;
        return new Vector2(width, height);
    }

    public static float GetCameraWorldWidth(this Camera camera)
    {
        var bottomRight = camera.ViewportToWorldPoint(new Vector3(1, 0, 0));
        var bottomLeft = camera.ViewportToWorldPoint(new Vector3(0, 0, 0));
        var width = bottomRight.x - bottomLeft.x;
        return width;
    }

    public static float GetCameraWorldHeight(this Camera camera)
    {
        var topRight = camera.ViewportToWorldPoint(new Vector3(1, 1, 0));
        var bottomRight = camera.ViewportToWorldPoint(new Vector3(1, 0, 0));
        var height = topRight.y - bottomRight.y;
        return height;
    }
}