using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerCameraInterestOffset : MonoBehaviour
{
    [SerializeField]
    private float speed = 1;
    [SerializeField]
    private float offsetLerpSpeed = 8f;
    private Vector3 offset;
    private Vector3 lastAppliedOffset;
    private Vector3 desiredOffset;
    [SerializeField]
    private Vector2 minMaxHorizontal = new (10, 10);
    [SerializeField]
    private Vector2 minMaxVertical = new (10, 10);
    private Transform camInterestPoint;
    private RoomObjectData currentRoom;
    
    [Serializable]
    public struct Box
    {
        [SerializeField]
        private Vector4 size; // meaning top,bottom,left,and right from the center
        [SerializeField]
        private Vector2 center;

        public Box(Vector4 size, Vector2 center)
        {
            this.size = size;
            this.center = center;
        }
        
        public bool Contains(Vector2 point)
        {
            return
                point.x >= center.x - size.z &&
                point.x <= center.x + size.w &&
                point.y >= center.y - size.y &&
                point.y <= center.y + size.x;
        }
    }

    [Header("MoveBoxSizes")]
    [SerializeField]
    private Vector2 defaultSizes = new (50, 50); // meaning the base width and height
    [SerializeField]
    private Box leftBox;
    [SerializeField]
    private Box rightBox;
    [SerializeField]
    private Box upBox;
    [SerializeField]
    private Box downBox;

    public void SetPlayerData(PlayerData playerData)
    {
        camInterestPoint = playerData.camInterestPoint;
        currentRoom = playerData.GetCurrentRoom();
        playerData.OnCurrentRoomChange += room =>
        {
            currentRoom = room;
            Reset();
        };
    }

    private void Update()
    {
        UpdateBoxes();

        var mouse = Mouse.current.position.ReadValue();
        
        if (leftBox.Contains(mouse))
            offset.x -= Time.deltaTime * speed;
        if (rightBox.Contains(mouse))
            offset.x += Time.deltaTime * speed;
        offset.x = Mathf.Clamp(offset.x, -minMaxHorizontal.x, minMaxHorizontal.y);
        if (upBox.Contains(mouse))
            offset.y += Time.deltaTime * speed;
        if (downBox.Contains(mouse))
            offset.y -= Time.deltaTime * speed;
        offset.y = Mathf.Clamp(offset.y, -minMaxVertical.x, minMaxVertical.y);

        var cameraRotation = Quaternion.LookRotation(currentRoom.cameraViewPoint, Vector3.up);
        desiredOffset = cameraRotation * offset;
        
        Vector3 newOffset = Vector3.Lerp(
            lastAppliedOffset,
            desiredOffset,
            Time.deltaTime * offsetLerpSpeed
        );
        
        camInterestPoint.position += (newOffset - lastAppliedOffset);
        lastAppliedOffset = newOffset;
    }
    
    private void UpdateBoxes()
    {
        float w = Screen.width;
        float h = Screen.height;

        leftBox = CreateEdgeBox(
            new Vector2(defaultSizes.x, h),
            new Vector2(defaultSizes.x * 0.5f, h * 0.5f)
        );

        rightBox = CreateEdgeBox(
            new Vector2(defaultSizes.x, h),
            new Vector2(w - defaultSizes.x * 0.5f, h * 0.5f)
        );

        upBox = CreateEdgeBox(
            new Vector2(w, defaultSizes.y),
            new Vector2(w * 0.5f, h - defaultSizes.y * 0.5f)
        );

        downBox = CreateEdgeBox(
            new Vector2(w, defaultSizes.y),
            new Vector2(w * 0.5f, defaultSizes.y * 0.5f)
        );
    }

    private Box CreateEdgeBox(Vector2 size, Vector2 center)
    {
        Vector4 halfExtents = new(
            size.y * 0.5f,
            size.y * 0.5f,
            size.x * 0.5f,
            size.x * 0.5f
        );

        return new Box(halfExtents, center);
    }

    public void Reset()
    {
        ResetInterestPoint();
        offset = Vector3.zero;
        desiredOffset = Vector3.zero;
        lastAppliedOffset = Vector3.zero;
    }

    private void ResetInterestPoint()
    {
        if (lastAppliedOffset == Vector3.zero) return;
        camInterestPoint.position -= lastAppliedOffset;
        lastAppliedOffset = Vector3.zero;
    }
}
