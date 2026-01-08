
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class RoomObjectData : MonoBehaviour
{
    private List<RoomEntrance> entrances;
    public Vector3 cameraViewPoint;
    public RoomCameraConfig cameraConfig;
    public Action OnReadyRoom;
    public InteractableObjectsManager interactableObjectsManager;
    
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
    
    public void AddEntrance(RoomEntrance entrance)
    {
        entrances ??= new List<RoomEntrance>();
        entrances.Add(entrance);
        entrance.UnLock();
    }

    public void ReadyRoom()
    {
        foreach (var entrance in entrances)
        {
            entrance.enabled = true;
        }

        UnLockDoors();
        OnReadyRoom?.Invoke();
    }

    public void DisableRoom()
    {
        foreach (var entrance in entrances)
        {
            entrance.enabled = false;
        }
    }

    public void LockDoors()
    {
        foreach (var entrance in entrances)
        {
            entrance.Lock();
        }
    }

    public void UnLockDoors()
    {
        foreach (var entrance in entrances)
        {
            entrance.UnLock();
        }
    }
}