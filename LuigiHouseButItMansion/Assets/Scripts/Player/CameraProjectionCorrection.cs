using System;
using UnityEngine;

public class CameraProjectionCorrection : MonoBehaviour
{
    [SerializeField]
    private Camera cam;

    private void Update()
    {
        transform.rotation = cam.transform.rotation;
    }
}