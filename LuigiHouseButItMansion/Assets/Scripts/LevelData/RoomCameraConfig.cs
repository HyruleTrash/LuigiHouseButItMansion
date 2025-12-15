
using System;
using UnityEngine;

public class RoomCameraConfig : MonoBehaviour
{
    [SerializeField]
    private Vector3 centerPointBounds;
    [SerializeField]
    private Vector3 sizeBounds;
    private RoomObjectData parent;
    private LayerMask playerMask;

    private void Start()
    {
        parent = transform.parent.GetComponent<RoomObjectData>();
        if (parent == null)
            enabled = false;
        playerMask = LayerMask.NameToLayer("Player");
        
        if (!TryGetComponent<BoxCollider>(out _))
            parent.cameraConfig = this;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawWireCube(centerPointBounds + transform.position, sizeBounds);
    }

    public Vector3 GetNearestInBounds(Vector3 newPos)
    {
        var bounds = new Bounds(centerPointBounds + transform.position, sizeBounds);
        return bounds.Contains(newPos) ? newPos : bounds.ClosestPoint(newPos);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == playerMask)
            parent.cameraConfig = this;
    }
}