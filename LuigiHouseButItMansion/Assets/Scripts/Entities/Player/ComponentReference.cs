using System.Linq;
using UnityEngine;

public class ComponentReference : MonoBehaviour
{
    public Component reference;

    public T GetReference<T>()  where T : Component
    {
        if (reference is T asType)
            return asType;
        return null;
    }

    public static T GetReference<T>(ComponentReference[] references) where T : Component
    {
        return references.Select(comp => comp.GetReference<T>()).FirstOrDefault(temp => temp != null);
    }
}