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

    void Awake()
    {
#if UNITY_WEBGL
        _targetPlatformConfig = WebPlatformConfig;
#elif UNITY_ANDROID
        _targetPlatformConfig = AndroidPlatformConfig;
#else
        _targetPlatformConfig = DesktopPlatformConfig;
#endif
    }

    [Header("Platform")]
    public WebPlatformConfigData WebPlatformConfig;
    public AndroidPlatformConfigData AndroidPlatformConfig;
    public DesktopPlatformConfigData DesktopPlatformConfig;

    PlatformConfigData _targetPlatformConfig;

    public static PlatformConfigData Platform => Instance != null ? Instance._targetPlatformConfig : null;

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
