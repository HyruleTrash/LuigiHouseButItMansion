using System.Collections.Generic;
using UnityEngine;

public class EntrancePointsGenerator : BaseRoomGeneratorComponent
{
    public List<PointDataHolder> entrancePoints = new();
    public GameObject entrancePrefab;
    
    public override List<PointDataHolder> GetList() => entrancePoints;

    public override void UpdateList()
    {
        entrancePoints.Clear();
        foreach (var data in GetComponentsInChildren<EntrancePointDataHolder>())
        {
            entrancePoints.Add(data);
        }
    }

    public override void Generate(RoomObjectData roomObjectData)
    {
        // throw new System.NotImplementedException(); TODO
    }
}