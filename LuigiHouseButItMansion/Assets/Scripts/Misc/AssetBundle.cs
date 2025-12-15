using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using UnityEngine;

public class AssetBundle : SingletonBehaviour<AssetBundle>
{
    [SerializeField]
    private List<ScriptableObject> assets = new();
    [ReadOnly]
    public List<string> keys = new();
    
#if UNITY_EDITOR
    private void OnValidate()
    {
        List<ScriptableObject> assets = new();
        List<string> keys = new();

        for (var i = 0; i < this.assets.Count; i++)
        {
            try
            {
                var asset = this.assets[i];
                var type = asset == null ? null : asset.GetType();
                var fullName = type == null ? "" : type.FullName;

                if (keys.Contains(fullName))
                {
                    keys.Add("");
                    assets.Add(null);
                    continue;
                }

                keys.Add(fullName);
                assets.Add(asset);
            }
            catch (Exception _)
            {
                keys.Add("");
                assets.Add(null);
            }
        }

        this.assets = assets;
        this.keys = keys;
    }
#endif

    public static T GetAsset<T>() where T : ScriptableObject
    {
        foreach (var pair in instance.assets)
        {
            Debug.Log($"test: {pair.name}");
        }
        
        var type = typeof(T);
        if (type.FullName == null)
            return null;
        return (T)instance.GetAsset(type.FullName);
    }

    private ScriptableObject GetAsset(string givenKey)
    {
        for (var i = 0; i < keys.Count; i++)
        {
            var key = keys[i];
            if (key == givenKey)
                return assets[i];
        }
        return null;
    }
}
