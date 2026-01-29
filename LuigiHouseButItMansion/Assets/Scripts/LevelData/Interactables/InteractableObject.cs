
using System;
using UnityEngine;
using UnityEngine.Events;

public class InteractableObject : MonoBehaviour
{
    public bool isWallMounted = false;
    public Bounds bounds;
    [SerializeField]
    private Vector3 spawnPoint;
    private InteractableObjectsManager managerRef;
    public UnityEvent OnInteract;
#if UNITY_EDITOR
    public MeshFilter objectRepresentation;
#endif

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(GetBoundsCenter(), bounds.size);
        Gizmos.color = Color.blue;
        Gizmos.DrawSphere(GetSpawnPoint(), 0.1f);
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
        var selfBounds = new Bounds(GetBoundsCenter(), bounds.size);
        var result = BoxesOverlap(selfBounds, otherBounds);
        return result;
    }
    
    private Vector3 GetBoundsCenter() => transform.rotation * bounds.center + transform.position;
    
    private bool BoxesOverlap(Bounds a, Bounds b)
    {
        return
            Mathf.Abs(a.center.x - b.center.x) <= (a.extents.x + b.extents.x) &&
            Mathf.Abs(a.center.y - b.center.y) <= (a.extents.y + b.extents.y) &&
            Mathf.Abs(a.center.z - b.center.z) <= (a.extents.z + b.extents.z);
    }

    public virtual void TriggerInteraction()
    {
        OnInteract.Invoke();
        OnInteract = new UnityEvent();
    }

    public Vector3 GetSpawnPoint()
    {
        return transform.TransformPoint(spawnPoint);
    }
}