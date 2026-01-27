using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerLookAt : MonoBehaviour
{
    [SerializeField]
    private PlayerData playerData;
    [SerializeField]
    private LayerMask layerMask;
    private Rigidbody rb;
    private Vector3? lastLookDirection;
    private bool wasMouseActive;

    private void Start()
    {
        if (playerData == null)
            enabled = false;
        rb = playerData.playerRigidbody;
    }

    private Vector3 hitPoint;
    private Vector3 lookAtPoint;
    private void Update()
    {
        void SetLookAt(Vector3 point)
        {
            hitPoint = point;
            lookAtPoint = new Vector3(hitPoint.x, rb.transform.position.y, hitPoint.z);
            rb.transform.LookAt(lookAtPoint);
        }
        
        Vector2 mouseDelta = Mouse.current.delta.ReadValue();
        float movementAmount = mouseDelta.magnitude;
        bool mouseActive = movementAmount >= MouseRayGetter.instance.minMouseActivity;
        if (mouseActive && !wasMouseActive)
            lastLookDirection = null;
        wasMouseActive = mouseActive;
        
        var mouseRay = MouseRayGetter.instance.GetMouseRay();
        var temp = MouseRayGetter.instance.GetRelevantHitBasedOnLastDirection(ref lastLookDirection, rb.position, layerMask, out var wasSizeZero);
        if (temp != null)
            SetLookAt(temp.Value.point);
        else if (!wasSizeZero)
            SetLookAt(hitPoint);
        else
        {
            if (!Physics.Raycast(rb.position, -Vector3.up, out var groundHit, Mathf.Infinity, layerMask)) return;
            if (!IntersectY(mouseRay.origin, mouseRay.direction, groundHit.point.y, out var foundHitPoint, out _)) return;
            SetLookAt(foundHitPoint);
        }
    }
    
    /// <summary>
    /// Gets the intersection position on a certain Y position
    /// </summary>
    public static bool IntersectY(Vector3 origin, Vector3 direction, float targetY, out Vector3 hitPoint, out float travelDistance)
    {
        if (direction == Vector3.zero)
        {
            hitPoint = Vector3.zero;
            travelDistance = 0f;
            return false;
        }

        var dir = direction.normalized;
        travelDistance = (targetY - origin.y) / dir.y;

        // Ray is parallel to the plane
        if (float.IsInfinity(travelDistance))
        {
            hitPoint = Vector3.zero;
            return false;
        }

        // Intersection is behind the ray origin
        if (travelDistance < 0f)
        {
            hitPoint = Vector3.zero;
            return false;
        }

        hitPoint = origin + dir * travelDistance;
        return true;
    }

    private void OnDrawGizmos()
    {
        if (!Application.isPlaying)
            return;
        Gizmos.color = Color.red;
        Gizmos.DrawLine(transform.position, hitPoint);
        Gizmos.DrawSphere(hitPoint, 0.1f);
        Gizmos.DrawSphere(lookAtPoint, 0.1f);
    }
}
