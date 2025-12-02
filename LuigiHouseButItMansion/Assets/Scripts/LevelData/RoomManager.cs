using UnityEngine;
using System.Collections.Generic;


public class RoomManager : SingletonBehaviour<RoomManager>
{
    public List<RoomObjectData> LiveRooms
    {
        get
        {
            if (liveRooms == null)
                liveRooms = new List<RoomObjectData>();
            return liveRooms;
        }
        set
        {
            if (liveRooms == null)
                liveRooms = new List<RoomObjectData>();
            liveRooms = value;
        }
    }

    private List<RoomObjectData> liveRooms;
}