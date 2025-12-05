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
    [SerializeField]
    private Camera cam;

    private void Start()
    {
        if (playerData == null)
            enabled = false;
        rb = playerData.playerRigidbody;
        cam = GetComponent<Camera>();
    }

    private Vector3 hitPoint;
    private Vector3 lookAtPoint;
    private void Update()
    {
        var mousePos = Mouse.current.position.ReadValue() / 2;
        var mouseRay = cam.ScreenPointToRay(new Vector2(mousePos.x, mousePos.y));
        if (Physics.Raycast(mouseRay, out var hit, Mathf.Infinity, layerMask))
        {
            hitPoint = hit.point;
            lookAtPoint = new Vector3(hitPoint.x, rb.transform.position.y, hitPoint.z);
            rb.transform.LookAt(lookAtPoint);
        }
        else
        {
            if (!Physics.Raycast(rb.position, -Vector3.up, out var groundHit, Mathf.Infinity, layerMask)) return;
            if (!IntersectY(mouseRay.origin, mouseRay.direction, groundHit.point.y, out hitPoint, out _)) return;
            lookAtPoint = new Vector3(hitPoint.x, rb.transform.position.y, hitPoint.z);
            rb.transform.LookAt(lookAtPoint);
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
