using System;
using System.Collections.Generic;
using UnityEngine;

public class SceneData : SingletonBehaviour<SceneData>
{
    private AssetBundle assetBundle;
    private Dictionary<Type, object> registeredObjects = new Dictionary<Type, object>();

    private void Start()
    {
        assetBundle = GetComponent<AssetBundle>();
        if (assetBundle == null)
            assetBundle = gameObject.AddComponent<AssetBundle>();
    }

    public void RegisteredObject<T>(object newInstance)
    {
        if (newInstance == null)
            return;
        registeredObjects.TryAdd(typeof(T), newInstance);
    }
    
    public T GetRegisteredObject<T>()
    {
        registeredObjects.TryGetValue(typeof(T), out object value);
        return (T)value;
    }
    
    public void DeRegisteredObject<T>()
    {
        registeredObjects.Remove(typeof(T));
    }
}
