using System;
using UnityEngine;
using UnityEngine.Serialization;

public class PlayerCameraMovement : MonoBehaviour
{
    [SerializeField]
    private float speed = 1;
    [SerializeField]
    float minDistance = 1f;
    [SerializeField]
    private Camera playerCamera;
    [HideInInspector]
    public Transform camInterestPoint;
    [HideInInspector]
    public Vector3 offset;
    private PlayerData playerData;
    private RoomObjectData currentRoom;

    private void Start()
    {
        playerData = transform.parent.GetComponent<PlayerData>();
        if (playerData == null)
        {
            enabled = false;
            return;
        }
        offset = transform.position - playerData.transform.position;
        
        var parentsParent = transform.parent.parent;
        transform.SetParent(parentsParent);

        currentRoom = playerData.GetCurrentRoom();
        playerData.OnCurrentRoomChange += room => { currentRoom = room;};
    }

    private void Update()
    {
        if (!camInterestPoint)
            return;
        if (Vector3.Distance(transform.position, camInterestPoint.position + offset) < minDistance)
            return;
        var newPos = Vector3.Lerp(transform.position, camInterestPoint.position + offset, Time.deltaTime * speed);
        transform.position = currentRoom.cameraConfig.GetNearestInBounds(newPos);
    }
}
