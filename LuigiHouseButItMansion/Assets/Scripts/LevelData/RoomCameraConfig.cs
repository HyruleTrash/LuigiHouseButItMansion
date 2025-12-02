
using System;
using UnityEngine;

public class RoomCameraConfig : MonoBehaviour
{
    [SerializeField]
    private Vector3 centerPointBounds;
    [SerializeField]
    private Vector3 sizeBounds;

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
}