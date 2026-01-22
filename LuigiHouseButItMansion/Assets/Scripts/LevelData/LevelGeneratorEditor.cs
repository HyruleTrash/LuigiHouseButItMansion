
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(LevelGenerator)), CanEditMultipleObjects]
public class LevelGeneratorEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        
        GUILayout.Space(10);
        LevelGenerator levelGenerator = (LevelGenerator)target;
        
        if (!GUILayout.Button("Update possible rooms list")) return;
        levelGenerator.possibleRooms.Clear();
        
        const string basePath = "Assets/Resources/Rooms";
        var subFolders = AssetDatabase.GetSubFolders(basePath);
        var assetsGUIDs = AssetDatabase.FindAssets("t:prefab", subFolders);
        foreach (var assetGuid in assetsGUIDs)
        {
            var path = AssetDatabase.GUIDToAssetPath(assetGuid);
            var go = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            levelGenerator.possibleRooms.Add(go);
        }
    }
}