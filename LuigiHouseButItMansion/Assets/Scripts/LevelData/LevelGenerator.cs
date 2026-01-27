using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.AI.Navigation;
using UnityEngine;
using Random = UnityEngine.Random;

public class LevelGenerator : MonoBehaviour
{
    [SerializeField]
    private GameObject navMeshSurface;
    public DirectionBasedEntranceList unUsedEntrances = new();
    public List<GameObject> possibleRooms = new();
    [HideInInspector]
    public GameObject lastPickedRoomPrefab;

    private void OnValidate()
    {
        if (possibleRooms.Count != 0) return;
        enabled = false;
    }

    private void Start()
    {
        RoomObjectData.OnCurrentRoomChange = null;
        CreateRoom(PickRoomFromPossibles(possibleRooms), true);
    }

    private GameObject PickRoomFromPossibles(List<GameObject> possibilityList)
    {
        if (!ValidatePossibleRooms(possibilityList))
        {
            Debug.LogError("No possible rooms found");
            return null;
        }

        const int maxAmountOfTries = 10;
        int tries = 0;
        while (true)
        {
            tries++;
            if (possibilityList.Count == 0)
            {
                Debug.LogError("No possible rooms found");
                return null;
            }
            var found = possibilityList[Random.Range(0, possibilityList.Count)];
            if (!found || lastPickedRoomPrefab == found)
            {
                if (tries > maxAmountOfTries)
                    return null;
                continue;
            }
            lastPickedRoomPrefab = found;
            return found;
        }
    }

    private bool ValidatePossibleRooms(List<GameObject> possibilityList)
    {
        if (possibilityList.Count == 0) return false;
        foreach (var obj in possibilityList) if (!obj) return false;
        return true;
    }

    private RoomObjectData CreateRoom(GameObject prefab, bool isFirst = false, Vector3? direction = null)
    {
        if (prefab == null) return null;
        var instance = Instantiate(prefab, transform.position, transform.rotation);
        var roomObj = instance.GetComponent<RoomObjectData>();
        roomObj.firstRoom = isFirst;
        roomObj.TurnToInstance(this, direction);
        
        foreach (var surface in navMeshSurface.GetComponents<NavMeshSurface>())
            StartCoroutine(BuildNextFrame(surface));
        
        return roomObj;
    }

    IEnumerator BuildNextFrame(NavMeshSurface surface)
    {
        yield return null;
        surface.BuildNavMesh();
    }

    public RoomEntrance GetConnectedRoom(RoomEntrance roomEntrance)
    {
        unUsedEntrances.Remove(roomEntrance);
        var dir = roomEntrance.transform.forward;
        
        var possibleConnection = GetPossibleRoomsInDirection(-dir);
        var room = CreateRoom(PickRoomFromPossibles(possibleConnection), direction: -dir);
        
        var entrance = room.GetEntranceToUse(this, roomEntrance, -dir);
        unUsedEntrances.Remove(entrance);
        
        return entrance;
    }

    private List<GameObject> GetPossibleRoomsInDirection(Vector3 dir)
    {
        List<GameObject> result = new();
        foreach (var room in possibleRooms)
        {
            var entrances = room.GetComponent<RoomObjectData>().GetEntrances();
            if (entrances.ContainsKey(dir))
                result.Add(room);
        }
        return result;
    }

    /// <summary>
    /// Checks if a backlog exists into a certain direction, if yes returns entrances that already have a backlog
    /// </summary>
    /// <param name="entrances"></param>
    /// <returns></returns>
    public List<RoomEntrance> CheckIfBacklogExists(DirectionBasedEntranceList entrances)
    {
        List<RoomEntrance> result = new();
        foreach (var entrance in entrances)
        {
            var dir = entrance.transform.forward;
            if (unUsedEntrances.ContainsKey(dir) && unUsedEntrances.GetFromDir(dir).Count > 0)
            {
                result.Add(entrance);
            }
        }
        return result;
    }
}