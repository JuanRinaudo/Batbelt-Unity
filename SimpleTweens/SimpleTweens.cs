using UnityEngine;

namespace SimpleTweens
{
    public static class SimpleTweensUtils
    {
        const string SIMPLE_INSTANCE_RESOURCE_PATH = "Batbelt/SimpleTweenInstance";
        
        public static void CreateTweenSingleton(Transform parent = null)
        {
            var instance = GameObject.Instantiate(Resources.Load<GameObject>(SIMPLE_INSTANCE_RESOURCE_PATH));
            if(parent != null)
                instance.transform.SetParent(parent);
        }
    }
}