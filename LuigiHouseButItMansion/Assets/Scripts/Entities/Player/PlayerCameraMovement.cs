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
    }

    private void Update()
    {
        var roomObjectData = RoomObjectData.CurrentRoom;
        if (!camInterestPoint || !roomObjectData)
            return;
        var usedInterestPoint = camInterestPoint.position + offset;
        if (Vector3.Distance(transform.position, usedInterestPoint) < minDistance)
            return;
        var newPos = Vector3.Lerp(transform.position, usedInterestPoint, Time.deltaTime * speed);
        transform.position = roomObjectData.cameraConfig.GetNearestInBounds(newPos);
    }
}
