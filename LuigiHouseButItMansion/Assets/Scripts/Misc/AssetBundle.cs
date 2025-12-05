using System;
using System.Collections.Generic;
using UnityEngine;

public class AssetBundle : MonoBehaviour
{
    [SerializeField]
    private List<ScriptableObject> assets = new();
    private bool initialized = false;
    private void OnGUI()
    {
        if(initialized)
            return;
        // foreach (var obj in assets)
        // {
        //     obj
        // }

        initialized = true;
    }
}
