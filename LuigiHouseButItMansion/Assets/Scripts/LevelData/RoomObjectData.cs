
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class RoomObjectData : MonoBehaviour
{
    [HideInInspector]
    public List<RoomEntrance> entrances;
    public Vector3 cameraViewPoint;
    public RoomCameraConfig cameraConfig;
    public Action OnReadyRoom;
    
    private void Start()
    {
        RoomManager.instance.LiveRooms.Add(this);
    }

    private void OnDestroy()
    {
        RoomManager.instance.LiveRooms.Remove(this);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        var offset = Vector3.up * 5;
        var startPosition = transform.position;
        var endPosition = cameraViewPoint * 2;
        Gizmos.DrawLine(startPosition + offset, startPosition + endPosition + offset);
    }

    public void ReadyRoom()
    {
        foreach (var entrance in entrances)
        {
            entrance.enabled = true;
        }
        OnReadyRoom?.Invoke();
    }

    public void DisableRoom()
    {
        foreach (var entrance in entrances)
        {
            entrance.enabled = false;
        }
    }
}