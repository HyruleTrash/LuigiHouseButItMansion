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
    public UnUsedEntranceList unUsedEntrances = new();
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
        CreateRoom(PickRoomFromPossibles(possibleRooms), true);
    }

    private GameObject PickRoomFromPossibles(List<GameObject> possibilityList)
    {
        while (true)
        {
            if (possibilityList.Count == 0)
            {
                Debug.LogError("No possible rooms found");
                return null;
            }
            var found = possibilityList[Random.Range(0, possibilityList.Count)];
            if (!found || lastPickedRoomPrefab == found) continue;
            lastPickedRoomPrefab = found;
            return found;
        }
    }

    private RoomObjectData CreateRoom(GameObject prefab, bool isFirst = false)
    {
        if (prefab == null) return null;
        var instance = Instantiate(prefab, transform.position, transform.rotation);
        var roomObj = instance.GetComponent<RoomObjectData>();
        roomObj.firstRoom = isFirst;
        roomObj.TurnToInstance(this);
        
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
        
        List<GameObject> possibleConnection = GetPossibleRoomsInDirection(-dir);
        var room = CreateRoom(PickRoomFromPossibles(possibleConnection));
        
        return room.GetEntranceToUse(this, roomEntrance, -dir);
    }

    private List<GameObject> GetPossibleRoomsInDirection(Vector3 direction)
    {
        var possibleConnection = new List<GameObject>();
        foreach (var room in possibleRooms)
        {
            foreach (var entrance in room.GetComponent<RoomObjectData>().GetEntrances())
            {
                var foundDir = entrance.transform.forward;
                if (foundDir != direction) continue;
                possibleConnection.Add(room);
                break;
            }
        }
        return possibleConnection;
    }
}

public class UnUsedEntranceList
{
    private Dictionary<Vector3, List<RoomEntrance>> unUsedEntrances = new();

    public void Remove(RoomEntrance roomEntrance)
    {
        foreach (var unUsedEntrance in unUsedEntrances.Values)
        {
            unUsedEntrance.Remove(roomEntrance);
            break;
        }
    }

    public void AddRange(List<RoomEntrance> entrances)
    {
        foreach (var entrance in entrances)
        {
            var dir = entrance.transform.forward;
            if (!unUsedEntrances.ContainsKey(dir))
                unUsedEntrances.Add(dir, new List<RoomEntrance>());
            unUsedEntrances[dir].Add(entrance);
        }
    }

    /// <summary>
    /// Checks if a backlog exists into a certain direction, if yes returns entrances that already have a backlog
    /// </summary>
    /// <param name="entrances"></param>
    /// <returns></returns>
    public List<RoomEntrance> CheckIfBacklogExists(List<RoomEntrance> entrances)
    {
        List<RoomEntrance> result = new();
        foreach (var entrance in entrances)
        {
            var dir = entrance.transform.forward;
            if (unUsedEntrances.ContainsKey(dir) && unUsedEntrances[dir].Count > 0)
            {
                result.Add(entrance);
            }
        }
        return result;
    }
}