using UnityEngine;

[RequireComponent(typeof(Camera))]
public class FitTargetsFOV : MonoBehaviour
{
    public Transform TargetA;
    public Transform TargetB;

    public Vector2 Padding = new Vector2(1.15f, 1.15f);

    public float MinFov = 10f;
    public float MaxFov = 160f;

    Camera cam;
    int lastScreenWidth;
    int lastScreenHeight;

    void Awake()
    {
        cam = GetComponent<Camera>();
    }

    void OnEnable()
    {
        CacheScreenDimensions();
        AdjustFOV();
    }

    void Update()
    {
        if (Screen.width != lastScreenWidth || Screen.height != lastScreenHeight)
        {
            CacheScreenDimensions();
            AdjustFOV();
        }
    }

#if UNITY_EDITOR
    void OnValidate()
    {
        if (cam == null)
            cam = GetComponent<Camera>();

        AdjustFOV();
    }
#endif

    void CacheScreenDimensions()
    {
        lastScreenWidth = Screen.width;
        lastScreenHeight = Screen.height;
    }

    [ContextMenu("Adjust FOV")]
    public void AdjustFOV()
    {
        if (cam == null || TargetA == null || TargetB == null)
            return;

        Vector3 localA = cam.transform.InverseTransformPoint(TargetA.position);
        Vector3 localB = cam.transform.InverseTransformPoint(TargetB.position);

        float nearPlane = cam.nearClipPlane > 0f ? cam.nearClipPlane : 0.01f;
        if (localA.z <= nearPlane || localB.z <= nearPlane)
        {
            Debug.LogWarning("Cannot fit FOV: One or both targets are behind or at the camera plane.", this);
            return;
        }

        float aspect = (Screen.height > 0) ? (float)Screen.width / Screen.height : cam.aspect;

        float padX = Mathf.Max(1.0f, Padding.x);
        float padY = Mathf.Max(1.0f, Padding.y);

        float reqXA = ((Mathf.Abs(localA.x) / localA.z) / aspect) * padX;
        float reqYA = (Mathf.Abs(localA.y) / localA.z) * padY;

        float reqXB = ((Mathf.Abs(localB.x) / localB.z) / aspect) * padX;
        float reqYB = (Mathf.Abs(localB.y) / localB.z) * padY;

        float maxTan = Mathf.Max(reqXA, reqYA, reqXB, reqYB);

        float halfFovRad = Mathf.Atan(maxTan);
        float calculatedFov = halfFovRad * 2f * Mathf.Rad2Deg;

        cam.fieldOfView = Mathf.Clamp(calculatedFov, MinFov, MaxFov);
    }
}