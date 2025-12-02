using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerLookAt : MonoBehaviour
{
    [SerializeField]
    private PlayerData playerData;
    [SerializeField]
    private LayerMask layerMask;
    private Rigidbody rb;
    private Camera cam;

    private void Start()
    {
        if (playerData == null)
            enabled = false;
        rb = playerData.playerRigidbody;
        cam = GetComponent<Camera>();
    }

    private Vector3 hitPoint;
    private Vector3 lookAtPoint;
    private void Update()
    {
        var mouseRay = cam.ScreenPointToRay(Mouse.current.position.ReadValue());
        if (Physics.Raycast(mouseRay, out RaycastHit hit, Mathf.Infinity, layerMask))
        {
            hitPoint = hit.point;
            lookAtPoint = new Vector3(hit.point.x, rb.transform.position.y, hit.point.z);
            rb.transform.LookAt(lookAtPoint);
        }
    }

    private void OnDrawGizmos()
    {
        if (!Application.isPlaying)
            return;
        Gizmos.color = Color.red;
        Gizmos.DrawLine(transform.position, hitPoint);
        Gizmos.DrawSphere(hitPoint, 0.1f);
        Gizmos.DrawSphere(lookAtPoint, 0.1f);
    }
}
