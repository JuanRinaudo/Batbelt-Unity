using System.Collections;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public static class AddressablesHelper
{
    /// <summary>
    /// Use this instead of WaitForCompletion everywhere.
    /// On WebGL: awaits async. On other platforms: uses sync.
    /// </summary>
    public static async Task<T> LoadAsync<T>(AssetReferenceT<T> assetRef) where T : UnityEngine.Object
    {
        var handle = Addressables.LoadAssetAsync<T>(assetRef);

#if UNITY_WEBGL && !UNITY_EDITOR
        // WebGL: must await — no blocking allowed
        await handle.Task;
#else
        // All other platforms: synchronous is fine
        handle.WaitForCompletion();
#endif

        if (handle.Status == AsyncOperationStatus.Succeeded)
            return handle.Result;

        Debug.LogError($"Failed to load addressable: {assetRef}");
        return default;
    }
    
    /// <summary>
    /// Use this instead of WaitForCompletion everywhere.
    /// On WebGL: awaits async. On other platforms: uses sync.
    /// </summary>
    public static async Task<T> LoadAsync<T>(string key)
    {
        var handle = Addressables.LoadAssetAsync<T>(key);

#if UNITY_WEBGL && !UNITY_EDITOR
        // WebGL: must await — no blocking allowed
        await handle.Task;
#else
        // All other platforms: synchronous is fine
        handle.WaitForCompletion();
#endif

        if (handle.Status == AsyncOperationStatus.Succeeded)
            return handle.Result;

        Debug.LogError($"Failed to load addressable: {key}");
        return default;
    }

    /// <summary>
    /// Coroutine version if you can't use async/await.
    /// </summary>
    public static IEnumerator LoadCoroutine<T>(string key, System.Action<T> onComplete)
    {
        var handle = Addressables.LoadAssetAsync<T>(key);

#if UNITY_WEBGL && !UNITY_EDITOR
        yield return handle;
#else
        handle.WaitForCompletion();
        yield return null;
#endif

        if (handle.Status == AsyncOperationStatus.Succeeded)
            onComplete?.Invoke(handle.Result);
        else
            Debug.LogError($"Failed to load addressable: {key}");
    }
}