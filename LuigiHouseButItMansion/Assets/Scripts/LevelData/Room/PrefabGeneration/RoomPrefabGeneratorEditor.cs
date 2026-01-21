
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(RoomPrefabGenerator))]
public class RoomPrefabGeneratorEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        
        RoomPrefabGenerator generator = (RoomPrefabGenerator)serializedObject.targetObject;
        if (!generator.enabled)
            return;
        
        GUILayout.Space(16);
        if (GUILayout.Button("Update all lists")) 
            generator.UpdateAllLists();
        if (GUILayout.Button("Save and generate prefab")) 
            generator.SaveAndGenerateAsPrefab();
    }
}