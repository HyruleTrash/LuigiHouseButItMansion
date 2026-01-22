
using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

[RequireComponent(typeof(GoopManager))]
public class RoomObjectData : MonoBehaviour
{
    public static Action<RoomObjectData> OnCurrentRoomChange;

    public bool firstRoom = false;
    [SerializeField]
    private List<RoomEntrance> entrances = new();
    public Vector3 cameraViewPoint;
    public RoomCameraConfig cameraConfig;
    public Action onReadyRoom;
    public InteractableObjectsManager interactableObjectsManager;
    public GoopManager goopManager;
    
    public void Init(RoomCameraConfig roomCameraConfigRef, Vector3 camSetViewDir)
    {
        cameraConfig = Instantiate(roomCameraConfigRef, transform);
        cameraViewPoint = camSetViewDir;
    }
    
    private void Start()
    {
        OnCurrentRoomChange = null;
        RoomManager.instance.LiveRooms.Add(this);
        goopManager = GetComponent<GoopManager>();
        goopManager.parent = this;

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

        SceneData.instance.RegistereObject<RoomObjectData>(newRoom, true);
        newRoom.goopManager.UpdateTexture();
        
        newRoom.ReadyRoom();
        OnCurrentRoomChange?.Invoke(newRoom);
    }

    public void AddEntrance(RoomEntrance entrance) => entrances.Add(entrance);
    public void RemoveEntrance(RoomEntrance roomEntrance) => entrances?.Remove(roomEntrance);

    public List<RoomEntrance> GetEntrances() => entrances?.ToList();

    public void ReadyRoom()
    {
        gameObject.SetActive(true);
        goopManager.SetToCurrent();
        
        foreach (var entrance in entrances)
        {
            entrance.enabled = true;
        }

        UnLockDoors();
        onReadyRoom?.Invoke();
    }

    public void DisableRoom()
    {
        goopManager.SaveTextureData(goopManager.usedRoomTexture);
        foreach (var entrance in entrances)
        {
            entrance.enabled = false;
        }
        
        gameObject.SetActive(false);
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

    public void TurnToInstance(LevelGenerator levelGenerator)
    {
        interactableObjectsManager.PickInteractables();

        if (firstRoom)
        {
            var spawner = GetComponentInChildren<EnemySpawnManager>();
            if (spawner)
                Destroy(spawner.gameObject);
        }
        
        // check how many not used entrances still exist
        var canBeRemoved = levelGenerator.unUsedEntrances.CheckIfBacklogExists(entrances);
        if (canBeRemoved.Count != 0)
        {
            var amount = Random.Range(0, canBeRemoved.Count);
            for (var i = 0; i < amount; i++)
            {
                var index = Random.Range(0, canBeRemoved.Count);
                var instance = entrances.Find(x => x == canBeRemoved[index]);
                Destroy(instance.gameObject);
                entrances.Remove(instance);
            }
        }
        levelGenerator.unUsedEntrances.AddRange(entrances);

        foreach (var entrance in entrances)
        {
            entrance.levelGeneratorRef = levelGenerator;
        }
        
        ReadyRoom();
    }

    public RoomEntrance GetEntranceToUse(LevelGenerator levelGenerator, RoomEntrance otherEntranceToConnect, Vector3 dir)
    {
        foreach (var entrance in entrances)
        {
            var foundDir = entrance.transform.forward;
            if (foundDir != dir) continue;
            levelGenerator.unUsedEntrances.Remove(entrance);
            entrance.otherRoomEntrance = otherEntranceToConnect;
            return entrance;
        }
        return null;
    }
}