
using System;
using System.Collections.Generic;
using UnityEngine;

public class InteractableObjectsManager : MonoBehaviour
{
    [SerializeField]
    private RoomObjectData parentRoom;
    private List<InteractableObject> interactables = new();

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
}