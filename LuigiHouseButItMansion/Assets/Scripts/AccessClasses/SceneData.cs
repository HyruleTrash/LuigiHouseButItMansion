using System;
using System.Collections.Generic;
using UnityEngine;

public class SceneData : SingletonBehaviour<SceneData>
{
    private AssetBundle assetBundle;
    private Dictionary<Type, object> registeredObjects = new();

    private void Start()
    {
        assetBundle = GetComponent<AssetBundle>();
        if (assetBundle == null)
            assetBundle = gameObject.AddComponent<AssetBundle>();
    }

    public void RegistereObject<T>(object newInstance, bool replace = false)
    {
        if (newInstance == null)
            return;
        if (!replace)
            registeredObjects.TryAdd(typeof(T), newInstance);
        else
        {
            registeredObjects[typeof(T)] = newInstance;
        }
    }
    
    public T GetRegisteredObject<T>()
    {
        registeredObjects.TryGetValue(typeof(T), out object value);
        return (T)value;
    }
    
    public void DeRegistereObject<T>()
    {
        registeredObjects.Remove(typeof(T));
    }
}
