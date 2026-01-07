
using System;
using UnityEngine;

public class InteractableObject : MonoBehaviour
{
    public Bounds bounds;
    private InteractableObjectsManager managerRef;

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(bounds.center + transform.position, bounds.size);
    }

    private void Start()
    {
        managerRef = GetComponentInParent<InteractableObjectsManager>();
        if (!managerRef)
        {
            enabled = false;
            return;
        }

        managerRef.Add(this);
    }

    private void OnDestroy()
    {
        if (!managerRef)
            return;
        managerRef.Remove(this);
    }

    public bool CheckIntersection(Bounds other, Vector3 offset)
    {
        var otherBounds = new Bounds(other.center + offset, other.size);
        var selfBounds = new Bounds(bounds.center + transform.position, bounds.size);
        var result = BoxesOverlap(selfBounds, otherBounds);
        return result;
    }
    
    private bool BoxesOverlap(Bounds a, Bounds b)
    {
        return
            Mathf.Abs(a.center.x - b.center.x) <= (a.extents.x + b.extents.x) &&
            Mathf.Abs(a.center.y - b.center.y) <= (a.extents.y + b.extents.y) &&
            Mathf.Abs(a.center.z - b.center.z) <= (a.extents.z + b.extents.z);
    }

    public virtual void TriggerInteraction()
    {
        Debug.Log("Interact!");
    }
}