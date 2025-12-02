using System;
using UnityEngine;

public class PlayerCameraMovement : MonoBehaviour
{
    [SerializeField]
    private float speed = 1;
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
        transform.position = Vector3.Lerp(transform.position, playerData.transform.position + offset, Time.deltaTime * speed);
    }
}
