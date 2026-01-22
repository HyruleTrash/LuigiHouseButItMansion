using System.Collections.Generic;
using System.Linq;
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
    
    public override bool CanGenerate() => entrancePoints.Count != 0 && entrancePrefab  != null;

    public override void Generate(RoomObjectData roomObjectData)
    {
        var possibleInteractionPoints = entrancePoints.Cast<EntrancePointDataHolder>().ToList();
        var result = new List<EntrancePointDataHolder>();
        var amount = Random.Range(minMaxChosenFromList.x, minMaxChosenFromList.y);
        for (int i = 0; i < amount; i++)
        {
            var index = Random.Range(0, possibleInteractionPoints.Count);
            var point = possibleInteractionPoints[index];
            result.Add(point);
            possibleInteractionPoints.RemoveAt(index);
        }

        foreach (var point in result)
        {
            point.CreateInstance(roomObjectData, parent, entrancePrefab);
        }
    }
}