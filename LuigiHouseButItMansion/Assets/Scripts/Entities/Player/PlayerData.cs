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
    private PlayerCameraInterestOffset playerCameraInterestOffset;
    public Transform camInterestPoint;
    [Header("Player")]
    public Rigidbody playerRigidbody;
    public PlayerMovement playerMovement;
    [SerializeField]
    private GameObject bodyModel;
    public Health healthComp;

    private void Awake()
    {
        if (camInterestPoint == null || playerCameraLookAt == null || playerCameraMovement == null)
        {
            enabled = false;
            return;
        }
        playerCameraInterestOffset.SetPlayerData(this);
        playerCameraLookAt.camInterestPoint = camInterestPoint;
        playerCameraMovement.camInterestPoint = camInterestPoint;
    }

    private void Start()
    {
        SceneData.instance.RegisteredObject<PlayerData>(this);
        currentRoom?.ReadyRoom();
    }

    private void OnDestroy()
    {
        SceneData.instance.DeRegisteredObject<PlayerData>();
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
        newRoom.ReadyRoom();
        OnCurrentRoomChange?.Invoke(currentRoom);
    }

    public RoomObjectData GetCurrentRoom()
    {
        return currentRoom;
    }

    /// <summary>
    /// Meant only to be used by teleportation devices ect, not for physics
    /// </summary>
    /// <param name="transformPosition"></param>
    public void SetPlayerPosition(Vector3 transformPosition)
    {
        playerCameraLookAt.Reset(transformPosition - playerRigidbody.position);
        playerRigidbody.position = transformPosition;
    }

    public void TriggerHitFlash()
    {
        var rendererComponent = bodyModel.GetComponentsInChildren<MeshRenderer>();
        if (healthComp.HitFlashKey == -1 || EntityHitFlash.instance.GetRegisteredEntity(healthComp.HitFlashKey) == null)
            healthComp.HitFlashKey = EntityHitFlash.instance.RegisterEntity(rendererComponent);
    }
}
