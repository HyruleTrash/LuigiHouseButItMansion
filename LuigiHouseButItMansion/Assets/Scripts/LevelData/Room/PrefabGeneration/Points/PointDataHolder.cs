using UnityEngine;
using UnityEditor;

public abstract class PointDataHolder : MonoBehaviour
{
    public Quaternion interactableObjRotation = Quaternion.identity; 
    protected BaseRoomGeneratorComponent parentGenerator;
    
    protected virtual void OnValidate()
    {
        if (parentGenerator != null) return;
        parentGenerator = GetParentComponent();
        AddSelfToParent();
    }

    protected abstract BaseRoomGeneratorComponent GetParentComponent();
    protected abstract void AddSelfToParent();
    public abstract Color GetColor();
    
    protected virtual void OnDrawGizmosSelected()
    {
        if (Selection.activeGameObject != transform.gameObject)
            return;
        Gizmos.color = GetColor();
        
        var dir = interactableObjRotation * Vector3.forward;
        var origin = transform.position + Vector3.up * 2;
        Gizmos.DrawLine(origin, origin + dir);
    }
}