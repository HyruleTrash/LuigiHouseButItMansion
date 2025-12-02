
using System;
using System.Collections.Generic;
using UnityEngine;

public class RoomEntrance : MonoBehaviour
{
    public RoomEntrance otherRoomEntrance;
    private RoomObjectData parentRoom;
    public Collider entranceTrigger;
    
    private void Awake()
    {
        parentRoom = transform.parent.GetComponent<RoomObjectData>();
        if (parentRoom == null)
        {
            enabled = false;
            return;
        }

        if (parentRoom.entrances == null)
            parentRoom.entrances = new List<RoomEntrance>();
        parentRoom.entrances.Add(this);

        if (otherRoomEntrance == null)
            enabled = false;

        if (entranceTrigger == null)
        {
            entranceTrigger = GetComponent<Collider>();
            entranceTrigger.isTrigger = true;
        }
    }
}