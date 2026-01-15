using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField]
    private PlayerData playerData;
    [SerializeField]
    private InputActionAsset inputActionAsset;
    
    [Serializable]
    public class SpeedData
    {
        public string name;
        public float speed = 5;
        public float maxSpeed = 3;
        public bool usesDrag = false;
        public float dragAmount = 0.1f;
        public float dragDecrementSpeed = 0.1f;
    }
    [Header("Configuration")]
    public List<SpeedData> speedData;
    public string currentSpeedDataName;
    private SpeedData currentSpeedData;
    private float currentDrag = 0;
    
    private InputAction moveAction;
    private Vector2 moveVector;
    private Rigidbody rb;

    private void OnEnable()
    {
        inputActionAsset.FindActionMap("Player").Enable();
    }
    
    private void OnDisable()
    {
        inputActionAsset.FindActionMap("Player").Disable();
    }

    private void OnValidate()
    {
        if (SetSpeedData(currentSpeedDataName)) return;
        enabled = false;
    }

    public bool SetSpeedData(string dataName)
    {
        var found = speedData.FirstOrDefault(x => x.name == dataName);
        if (found == null) return false;
        currentSpeedData = found;
        currentSpeedDataName = dataName;
        return true;
    }

    private void Start()
    {
        if (playerData == null){
            enabled = false;
        }

        rb = playerData.playerRigidbody;
        var parentsParent = rb.transform.parent.parent;
        rb.transform.SetParent(parentsParent);

        moveAction = inputActionAsset.FindActionMap("Player").FindAction("Move");
        
        SetSpeedData(currentSpeedDataName);
    }

    private void Update()
    {
        moveVector = moveAction.ReadValue<Vector2>();
        playerData.transform.position = rb.transform.position;

        MovePlayerBasedOnMoveAction(moveVector);
        LimitVelocity();
    }

    private void LimitVelocity()
    {
        var horizontalVelocity = VectorHelper.GetXZ(rb.linearVelocity);
        // Debug.Log($"HorizontalVelocity: {horizontalVelocity}, magnitude: {horizontalVelocity.magnitude}");
        if (horizontalVelocity.magnitude <= currentSpeedData.maxSpeed) 
            return;
        horizontalVelocity = horizontalVelocity.normalized * currentSpeedData.maxSpeed;
        rb.linearVelocity = VectorHelper.XZToVector3(horizontalVelocity, rb.linearVelocity.y);
    }

    private void MovePlayerBasedOnMoveAction(Vector2 givenMoveVec)
    {
        if (givenMoveVec == Vector2.zero)
        {
            if (currentSpeedData.usesDrag)
            {
                currentDrag = Mathf.Lerp(currentDrag, 0, Time.deltaTime * currentSpeedData.dragDecrementSpeed);
                rb.linearVelocity += rb.linearVelocity.normalized * currentDrag;
            }
            return;
        }

        currentDrag = currentSpeedData.dragAmount;

        var cameraRotation = Quaternion.LookRotation(SceneData.instance.GetRegisteredObject<RoomObjectData>().cameraViewPoint, Vector3.up);
        var moveVector3D = new Vector3(givenMoveVec.x, 0, givenMoveVec.y);
        var moveVectorFinal = cameraRotation * moveVector3D;
        
        var force = moveVectorFinal * currentSpeedData.speed;
        rb.AddForce(force * Time.deltaTime, ForceMode.VelocityChange);
    }
}
