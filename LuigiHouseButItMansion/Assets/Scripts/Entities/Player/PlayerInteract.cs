using System;
using LucasCustomClasses;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInteract : MonoBehaviour
{
    [SerializeField]
    private PlayerData playerDataRef;
    [SerializeField]
    private InputActionAsset inputActionAsset;
    private InputAction interactAction;
    private bool canInteract = true;
    private Timer interactionCooldownTimer;
    [SerializeField]
    private Bounds bounds;
    [SerializeField]
    private Vector3 offset;
    private Vector3 usedOffset;

    private void Start()
    {
        interactionCooldownTimer = new Timer(0.5f, () => canInteract = true);
        interactionCooldownTimer.running = false;
        
        interactAction = InputSystem.actions.FindAction("Interact");
        interactAction.started += TryInteract;
    }

    private void TryInteract(InputAction.CallbackContext obj)
    {
        if (!canInteract || !playerDataRef)
            return;
        
        usedOffset = transform.rotation * offset;
        
        foreach (var interactable in RoomObjectData.CurrentRoom.interactableObjectsManager.GetInteractables())
        {
            if (!interactable)
                return;
            if (!interactable.CheckIntersection(bounds, transform.position + usedOffset)) continue;
            TriggerInteraction(interactable);
            break;
        }
        
        canInteract = false;
        interactionCooldownTimer.Reset();
    }

    private void TriggerInteraction(InteractableObject interactable) => interactable.TriggerInteraction();
    private void Update() => interactionCooldownTimer.Update(Time.deltaTime);

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        usedOffset = transform.rotation * offset;
        Gizmos.DrawWireCube(bounds.center + transform.position + usedOffset, bounds.size);
    }
}
