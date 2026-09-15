using System.ComponentModel;
using UnityEngine;
using UnityEngine.Serialization;

[DefaultExecutionOrder(-200)]
public partial class Configs : MonoBehaviour
{
    static Configs _instance;
    public static Configs Instance {
        get {
            if(_instance == null)
                _instance = FindAnyObjectByType<Configs>();

            return _instance;
        }
        set {
            _instance = value;
        }
    }

    [Header("Platform")]
    public WebPlatformConfigData WebPlatformConfig;
    public AndroidPlatformConfigData AndroidPlatformConfig;
    public DesktopPlatformConfigData DesktopPlatformConfig;

    public static PlatformConfigData Platform
    {
        get
        {
#if UNITY_WEBGL
            return Instance != null ? Instance.WebPlatformConfig : null;
#elif UNITY_ANDROID
            return Instance != null ? Instance.AndroidPlatformConfig : null;
#else
            return Instance != null ? Instance.DesktopPlatformConfig : null;
#endif
        }
    }

    [Header("Configs")]
    public CoreConfigData CoreConfig;
    public static CoreConfigData Core => Instance != null ? Instance.CoreConfig : null;
    
    public GameConfigData GameConfig;
    public static GameConfigData Game => Instance != null ? Instance.GameConfig : null;

    public AnimationsConfigData AnimationsConfig;
    public static AnimationsConfigData Animations => Instance != null ? Instance.AnimationsConfig : null;

    public AudioConfigData AudioConfig;
    public static AudioConfigData Audio => Instance != null ? Instance.AudioConfig : null;

    public UIConfigData UIConfig;
    public static UIConfigData UI => Instance != null ? Instance.UIConfig : null;
}
