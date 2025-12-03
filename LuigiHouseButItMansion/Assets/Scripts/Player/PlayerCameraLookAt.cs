
using System;
using UnityEngine;

public class PlayerCameraLookAt : MonoBehaviour
{
    [SerializeField]
    private float speed = 1;
    [SerializeField]
    float minDistance = 1f;
    public Camera playerCamera;
    [HideInInspector]
    public Transform camInterestPoint;
    private Transform lastInterestPointPosition;

    private void Start()
    {
        if (camInterestPoint == null){
            enabled = false;
            return;
        }
        lastInterestPointPosition = new GameObject("camInterestPoint").transform;
        lastInterestPointPosition.position = camInterestPoint.position;
    }

    private void Update()
    {
        if (!(Vector3.Distance(lastInterestPointPosition.position, camInterestPoint.position) < minDistance))
        {
            var newPos = Vector3.Lerp(lastInterestPointPosition.position, camInterestPoint.position,
                Time.deltaTime * speed);
            lastInterestPointPosition.position = newPos;
        }
        transform.LookAt(lastInterestPointPosition.position);
    }

    /// <summary>
    /// Reset the camera lerping
    /// </summary>
    /// <param name="playerOffset">needs a player offset based on new position</param>
    public void Reset(Vector3 playerOffset)
    {
        lastInterestPointPosition.position = camInterestPoint.position + playerOffset;
    }
}