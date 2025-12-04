using System;
using UnityEngine;

[System.Serializable]
public class ClassReference<T>
{
    [SerializeField] private string className;
    [SerializeField] private T fallbackObject;

    public System.Type ResolvedType => string.IsNullOrEmpty(className)
        ? fallbackObject?.GetType()
        : System.Type.GetType(className);

    public void CallMethod(string methodName, object[] parameters = null)
    {
        var type = ResolvedType;
        if (type == null)
            return;
        var instance = Activator.CreateInstance(type);
        type.GetMethod(methodName)?.Invoke(instance, parameters);
    }
    
    public void CallMethodStatic(string methodName, object[] parameters = null)
    {
        ResolvedType?.GetMethod(methodName)?.Invoke(null, parameters);
    }
}