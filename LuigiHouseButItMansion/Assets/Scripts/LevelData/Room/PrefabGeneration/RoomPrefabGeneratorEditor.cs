
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
        if (GUILayout.Button("Update entrance list")) 
            generator.UpdateEntranceList();
        if (GUILayout.Button("Update interactable list")) 
            generator.UpdateInteractableList();
    }
}