using System;
using UnityEngine;
using UnityEngine.Serialization;

public class PlayerData : MonoBehaviour
{
    [SerializeField]
    private RoomObjectData currentRoom;
    public Action<RoomObjectData> OnCurrentRoomChange;
    [Header("Cam data")]
    [SerializeField]
    private PlayerCameraLookAt playerCameraLookAt;
    [SerializeField]
    private PlayerCameraMovement playerCameraMovement;
    [SerializeField]
    private Transform camInterestPoint;
    [Header("Player")]
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

    public void SetCurrentRoom(RoomObjectData newRoom)
    {
        if (newRoom == currentRoom)
            return;
        currentRoom.DisableRoom();
        currentRoom = newRoom;
        currentRoom.ReadyRoom();
        OnCurrentRoomChange?.Invoke(currentRoom);
    }

    public RoomObjectData GetCurrentRoom()
    {
        return currentRoom;
    }

    public void SetPlayerPosition(Vector3 transformPosition)
    {
        playerRigidbody.position = transformPosition;
    }
}
