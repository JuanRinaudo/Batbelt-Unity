#if UNITY_WEBGL
using UnityEngine;
using System.Runtime.InteropServices;

public static class WebExtras
{
    [DllImport("__Internal")]
    public static extern void ChangeDevicePixelRatio(float value);

    [DllImport("__Internal")]
    public static extern void FilesystemSync();
}
#endif