using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class LevelGenerator : MonoBehaviour
{
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
        CreateRoom(PickRoomFromPossibles(), true);
    }

    private GameObject PickRoomFromPossibles()
    {
        while (true)
        {
            var found = possibleRooms[Random.Range(0, possibleRooms.Count)];
            if (!found || lastPickedRoomPrefab == found) continue;
            lastPickedRoomPrefab = found;
            return found;
        }
    }

    private RoomObjectData CreateRoom(GameObject prefab, bool isFirst)
    {
        var instance = Instantiate(prefab, transform.position, transform.rotation);
        var roomObj = instance.GetComponent<RoomObjectData>();
        roomObj.firstRoom = isFirst;
        roomObj.TurnToInstance();
        return roomObj;
    }
}