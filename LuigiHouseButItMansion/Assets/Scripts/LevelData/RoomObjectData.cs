
using System;
using System.Collections.Generic;
using UnityEngine;

public class RoomObjectData : MonoBehaviour
{
    [HideInInspector]
    public List<RoomEntrance> entrances;
    
    private void Start()
    {
        RoomManager.instance.liveRooms.Add(this);
    }

    private void OnDestroy()
    {
        RoomManager.instance.liveRooms.Remove(this);
    }
}