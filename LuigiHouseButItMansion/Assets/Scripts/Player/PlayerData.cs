using System;
using UnityEngine;
using UnityEngine.Serialization;

public class PlayerData : MonoBehaviour
{
    public RoomObjectData currentRoom;
    [SerializeField]
    private PlayerCameraLookAt playerCameraLookAt;
    [SerializeField]
    private PlayerCameraMovement playerCameraMovement;
    [SerializeField]
    private Transform camInterestPoint;
    public Rigidbody playerRigidbody;

    private void Awake()
    {
        if (camInterestPoint == null || playerCameraLookAt == null || playerCameraMovement == null)
        {
            enabled = false;
            return;
        }
        playerCameraLookAt.camInterestPoint = camInterestPoint;
        playerCameraMovement.camInterestPoint = camInterestPoint;
    }

    public Vector3 GetCameraDirection()
    {
        return (transform.position - playerCameraLookAt.playerCamera.transform.position).normalized;
    }
}
