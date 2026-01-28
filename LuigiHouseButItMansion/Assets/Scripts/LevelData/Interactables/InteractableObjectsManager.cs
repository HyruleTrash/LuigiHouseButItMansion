
using System;
using System.Collections.Generic;
using UnityEngine;

public class InteractableObjectsManager : MonoBehaviour
{
    [SerializeField]
    private RoomObjectData parentRoom;
    private List<InteractableObject> interactables = new();
    [SerializeField] private List<InteractionPointDataHolder> possibleInteractables;

    public void Init(RoomObjectData roomObjectData, List<InteractionPointDataHolder> result)
    {
        possibleInteractables = result;
        parentRoom = roomObjectData;
        roomObjectData.interactableObjectsManager = this;
    }

    private void Start()
    {
        if (parentRoom == null)
        {
            enabled = false;
            return;
        }

        parentRoom.interactableObjectsManager = this;
    }

    public void Add(InteractableObject instance)
    {
        interactables.Add(instance);
    }
    
    public void Remove(InteractableObject instance)
    {
        interactables.Remove(instance);
    }
    
    public InteractableObject[] GetInteractables() => interactables.ToArray();

    public InteractableObject GetRandom()
    {
        var index = -1;
        var count = interactables.Count * 2;
        while (true)
        {
            index = UnityEngine.Random.Range(0, interactables.Count - 1);
            count--;
            if (index >= 0 && index < interactables.Count && interactables[index] != null)
                break;
            if (count <= 0)
                return null;
        }
        
        return interactables[index];
    }

    public void PickInteractables()
    {
        foreach (var possibleInteractable in possibleInteractables)
        {
            var prefab = possibleInteractable.GetInteractable();
            interactables.Add(possibleInteractable.InstantiatePrefab(prefab, transform).GetComponent<InteractableObject>());
            Destroy(possibleInteractable.gameObject);
        }
        
        // cleanup
        foreach (var possibleInteractable in possibleInteractables) Destroy(possibleInteractable);
        possibleInteractables.Clear();
    }
}