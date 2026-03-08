#if UNITY_WEBGL && WEB_EXTRAS_ENABLED
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