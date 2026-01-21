
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(BaseRoomGeneratorComponent), true), CanEditMultipleObjects]
public class BaseRoomGeneratorComponentEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        
        BaseRoomGeneratorComponent generator = (BaseRoomGeneratorComponent)serializedObject.targetObject;
        if (!generator.enabled)
            return;
        
        if (GUILayout.Button("Update list")) 
            generator.UpdateList();
    }
}