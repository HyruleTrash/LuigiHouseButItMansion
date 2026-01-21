using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[RequireComponent(typeof(RoomPrefabGenerator))]
public class InteractionPointsGenerator : BaseRoomGeneratorComponent
{
    public List<PointDataHolder> interactionPoints = new();
    
    public override void UpdateList()
    {
        interactionPoints.Clear();
        foreach (var data in GetComponentsInChildren<InteractionPointDataHolder>())
        {
            interactionPoints.Add(data);
        }
    }
    
    public override void Generate(RoomObjectData roomObjectData)
    {
        var interactableObjectsManager = new GameObject("InteractablesManager").AddComponent<InteractableObjectsManager>();
        interactableObjectsManager.transform.SetParent(roomObjectData.transform);
        
        var possibleInteractionPoints = interactionPoints.Cast<InteractionPointDataHolder>().ToList();
        var result = new List<InteractionPointDataHolder>();
        int amount = UnityEngine.Random.Range(minMaxChosenFromList.x, minMaxChosenFromList.y);
        for (int i = 0; i < amount; i++)
        {
            int index = UnityEngine.Random.Range(0, possibleInteractionPoints.Count);
            InteractionPointDataHolder point = possibleInteractionPoints[index];
            result.Add(Instantiate(point, roomObjectData.transform));
            possibleInteractionPoints.RemoveAt(index);
        }

        interactableObjectsManager.Init(roomObjectData, result);
    }

    public override List<PointDataHolder> GetList() => interactionPoints;
}