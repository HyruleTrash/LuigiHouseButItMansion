
using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Serialization;

[RequireComponent(typeof(GoopManager))]
public class RoomObjectData : MonoBehaviour
{
    public static Action<RoomObjectData> OnCurrentRoomChange;

    public bool firstRoom = false;
    private List<RoomEntrance> entrances;
    public Vector3 cameraViewPoint;
    public RoomCameraConfig cameraConfig;
    public Action onReadyRoom;
    public InteractableObjectsManager interactableObjectsManager;
    public GoopManager goopManager;

    private void Start()
    {
        OnCurrentRoomChange = null;
        RoomManager.instance.LiveRooms.Add(this);
        goopManager = GetComponent<GoopManager>();
        goopManager.parent = this;
        goopManager.roomTexture = null;

        if (!firstRoom) return;
        SetCurrentRoom(this);
        ReadyRoom();
    }

    private void OnDestroy()
    {
        RoomManager.instance.LiveRooms.Remove(this);
        if (SceneData.instance.GetRegisteredObject<RoomObjectData>() == this)
            SceneData.instance.DeRegistereObject<RoomObjectData>();
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        var offset = Vector3.up * 5;
        var startPosition = transform.position;
        var endPosition = cameraViewPoint * 2;
        Gizmos.DrawLine(startPosition + offset, startPosition + endPosition + offset);
    }

    private void OnValidate()
    {
        goopManager = GetComponent<GoopManager>();
        goopManager.parent = this;
    }

    public static void SetCurrentRoom(RoomObjectData newRoom)
    {
        RoomObjectData oldRoom = SceneData.instance.GetRegisteredObject<RoomObjectData>();
        if (newRoom == null || newRoom == oldRoom) return;

        if (oldRoom != null) oldRoom.DisableRoom();

        Debug.Log("registering current room" + newRoom.gameObject.name);
        SceneData.instance.RegistereObject<RoomObjectData>(newRoom, true);
        Debug.Log(newRoom.gameObject.name + " has been set");
        
        GoopManager.UpdateTexture(newRoom.goopManager);
        
        newRoom.ReadyRoom();
        OnCurrentRoomChange?.Invoke(newRoom);
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
        onReadyRoom?.Invoke();

        goopManager.SetGlobalShaderData();
    }

    public void DisableRoom()
    {
        goopManager.DestroyTexture();
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