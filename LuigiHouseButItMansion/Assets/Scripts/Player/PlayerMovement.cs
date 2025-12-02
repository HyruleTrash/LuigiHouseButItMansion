using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField]
    private PlayerData playerData;
    [SerializeField]
    private InputActionAsset inputActionAsset;
    
    [Header("Configuration")]
    [SerializeField]
    private float speed = 1;
    
    // [SerializeField]
    private InputAction moveAction;
    private Vector2 moveVector;
    private Rigidbody rb;

    private void OnEnable()
    {
        inputActionAsset.FindActionMap("Player").Enable();
    }
    
    private void OnDisable()
    {
        inputActionAsset.FindActionMap("Player").Enable();
    }

    private void Start()
    {
        if (playerData == null){
            enabled = false;
        }

        rb = playerData.playerRigidbody;
        var parentsParent = rb.transform.parent.parent;
        rb.transform.SetParent(parentsParent);

        moveAction = InputSystem.actions.FindAction("Move");
    }

    private void Update()
    {
        moveVector = moveAction.ReadValue<Vector2>();
        playerData.transform.position = rb.transform.position;
        
        if (moveVector == Vector2.zero)
            return;

        var cameraRotation = Quaternion.LookRotation(playerData.currentRoom.cameraViewPoint, Vector3.up);
        Vector3 moveVector3D = new Vector3(moveVector.x, 0, moveVector.y);
        Vector3 moveVectorFinal = cameraRotation * moveVector3D;
        
        var force = moveVectorFinal * speed;
        rb.AddForce(force);
    }
}
