
using System;
using UnityEngine;

public class PlayerCameraLookAt : MonoBehaviour
{
    public Camera playerCamera;
    [HideInInspector]
    public Transform camInterestPoint;

    private void Update()
    {
        transform.LookAt(camInterestPoint);
    }
}