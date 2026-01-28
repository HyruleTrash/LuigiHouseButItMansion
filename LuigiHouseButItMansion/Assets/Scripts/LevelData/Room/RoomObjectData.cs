
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
    public static RoomObjectData CurrentRoom { get; private set; }

    public bool firstRoom = false;
    [SerializeField]
    private DirectionBasedEntranceList entrances = new();
    public Vector3 cameraViewPoint;
    public RoomCameraConfig cameraConfig;
    public Action onReadyRoom;
    public InteractableObjectsManager interactableObjectsManager;
    public GoopManager goopManager;
    
    public void Init(List<RoomCameraConfig> roomCameraConfigRef, Vector3 camSetViewDir)
    {
        for (var i = 0; i < roomCameraConfigRef.Count; i++)
        {
            var instance = Instantiate(roomCameraConfigRef[i], transform);
            if (i == 0)
                cameraConfig = instance;
        }

        cameraViewPoint = camSetViewDir;
    }
    
    public void AddEntrance(RoomEntrance entrance) => entrances.Add(entrance);
    public DirectionBasedEntranceList GetEntrances() => entrances;
    
    private void Start()
    {
        goopManager = GetComponent<GoopManager>();
        goopManager.parent = this;

        if (!firstRoom) return;
        SetCurrentRoom(this);
        ReadyRoom();
    }

    private void OnDestroy()
    {
        if (CurrentRoom == this)
            CurrentRoom = null;
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
        var oldRoom = CurrentRoom;
        if (newRoom == null || newRoom == oldRoom) return;

        if (oldRoom != null) oldRoom.DisableRoom();

        CurrentRoom = newRoom;
        newRoom.goopManager.UpdateTexture();
        
        newRoom.ReadyRoom();
        OnCurrentRoomChange?.Invoke(newRoom);
    }

    public void ReadyRoom()
    {
        if (CurrentRoom != this)
            return;
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
        Debug.Log("LockingDoors");
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

    public void TurnToInstance(LevelGenerator levelGenerator, Vector3? direction)
    {
        interactableObjectsManager.PickInteractables();

        if (firstRoom)
        {
            var spawner = GetComponentInChildren<EnemySpawnManager>();
            if (spawner)
                Destroy(spawner.gameObject);
        }
        
        // check how many not used entrances still exist
        var canBeRemoved = levelGenerator.CheckIfBacklogExists(entrances);
        if (canBeRemoved.Count != 0)
        {
            var amount = Random.Range(0, canBeRemoved.Count);
            for (var i = 0; i < amount; i++)
            {
                var index = Random.Range(0, canBeRemoved.Count);
                var instance = entrances.Find(x => ReferenceEquals(x, canBeRemoved[index]));
                if (!instance)
                    continue;
                if (direction != null && instance.transform.forward == direction.Value)
                    continue;
                entrances.Remove(instance);
                Destroy(instance.gameObject);
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
        var foundEntrance = entrances.GetFromDir(dir).FirstOrDefault();
        if (foundEntrance == null)
            return null;
        levelGenerator.unUsedEntrances.Remove(foundEntrance);
        foundEntrance.otherRoomEntrance = otherEntranceToConnect;
        return foundEntrance;
    }
}