#if UNITY_EDITOR
using System;
using System.Linq;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

[CustomPropertyDrawer(typeof(ClassReference<>))]
public class ClassReferencePropertyDrawer : PropertyDrawer
{
    private class ClassReferenceStyles
    {
        private GUIStyle customStyle;

        public static GUIStyle ClassReferenceStyle
        {
            get
            {
                var temp = new ClassReferenceStyles();
                return temp.customStyle;
            }
        }
        
        public static GUIStyle ClassReferenceStyleNone
        {
            get
            {
                var temp = new ClassReferenceStyles();
                temp.customStyle.font.material.color = Color.gray;
                return temp.customStyle;
            }
        }

        private ClassReferenceStyles()
        {
            customStyle = new GUIStyle(EditorStyles.objectField)
            {
                font = EditorStyles.standardFont,
                richText = true
            };
        }
    }
    
    private string[] displays = new string[0];
    private Type[] options = new Type[0];
    private bool generated = false;
    
    private int selectedIndex = 0;

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);

        // Draw label
        position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);
        
        // Calculate rect
        var nameRect = new Rect(position.x, position.y, position.width, position.height);

        // Draw fields
        GenerateDisplays();

        // Get original value
        SerializedProperty className = property.FindPropertyRelative("className");
        selectedIndex = GetSavedType(className);
        
        // Draw pop up field, and return the set index
        selectedIndex = EditorGUI.Popup(nameRect, selectedIndex, displays, ClassReferenceStyles.ClassReferenceStyle);
        
        // Using retrieved index set new value
        var chosenOption = options[selectedIndex];
        if (chosenOption != null)
            className.stringValue = chosenOption.Name;

        EditorGUI.EndProperty();
    }

    private int GetSavedType(SerializedProperty property)
    {
        string className = property.stringValue;
        if (className == null)
            return 0;
        
        for (var i = 0; i < options.Length; i++)
        {
            var option = options[i];
            if (option == null)
                continue;
            if (option.Name == className)
                return i;
        }

        return 0;
    }

    private void GenerateDisplays()
    {
        if (generated)
            return;
        Type foundType = fieldInfo.FieldType;
        var genericArguments = foundType.GetGenericArguments();
        
        var type = genericArguments[0];
        var types = AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(s => s.GetTypes())
            .Where(p => type.IsAssignableFrom(p));
        
        displays = new string[types.Count() + 1];
        options = new Type[types.Count() + 1];
        displays[0] = $" None ({type.Name})";
        
        int counter = 1;
        foreach (var typeThatImplements in types)
        {
            if (typeThatImplements.FullName == type.FullName)
                continue;
            displays[counter] = $" {typeThatImplements.Name}";
            options[counter] = typeThatImplements;
            counter++;
        }

        generated = true;
    }
}
#endif