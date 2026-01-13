
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
    [SerializeField]
    float maxDownAngle = 34f;
    [SerializeField]
    float maxUpAngle = -34f;

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
        var lookRot = Quaternion.LookRotation(lastInterestPointPosition.position - transform.position, Vector3.up);
        var lookRotEuler = lookRot.eulerAngles;
        
        // Convert from 0–360 to -180–180
        if (lookRotEuler.x > 180f) lookRotEuler.x -= 360f;
        lookRotEuler.x = Mathf.Clamp(lookRotEuler.x,maxUpAngle, maxDownAngle);
        
        transform.rotation = Quaternion.Euler(lookRotEuler);
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