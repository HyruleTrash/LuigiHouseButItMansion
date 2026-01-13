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
    private EdgeBoxes edgeBoxes = new ();
    
    [Header("MoveBoxSizes (percentage of screen size)")]
    [SerializeField, Range(0f, 100f)]
    private float defaultSizeW = 20;
    [SerializeField, Range(0f, 100f)]
    private float defaultSizeH = 20;
    
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

    [Serializable]
    public class EdgeBoxes
    {
        public Box leftBox;
        public Box rightBox;
        public Box upBox;
        public Box downBox;
        
        public void UpdateBoxes(float defaultSizeW, float defaultSizeH, Vector2 minMaxHorizontal, Vector2 minMaxVertical, Vector2 offset)
        {
            float w = Screen.width;
            float h = Screen.height;

            Vector2 usedSize = new(w * defaultSizeW / 100, h * defaultSizeH / 100);
        
            var usedParam = usedSize.x * (1 + 1 / minMaxHorizontal.x * offset.x);
            leftBox = CreateEdgeBox(
                new Vector2(usedParam, h),
                new Vector2(usedParam * 0.5f, h * 0.5f)
            );

            usedParam = usedSize.x * (1 - 1 / minMaxHorizontal.y * offset.x);
            rightBox = CreateEdgeBox(
                new Vector2(usedParam, h),
                new Vector2(w - usedParam * 0.5f, h * 0.5f)
            );

            usedParam = usedSize.y * (1 - 1 / minMaxVertical.x * offset.y);
            upBox = CreateEdgeBox(
                new Vector2(w, usedParam),
                new Vector2(w * 0.5f, h - usedParam * 0.5f)
            );

            usedParam = usedSize.y * (1 + 1 / minMaxVertical.y * offset.y);
            downBox = CreateEdgeBox(
                new Vector2(w, usedParam),
                new Vector2(w * 0.5f, usedParam * 0.5f)
            );
        }

        private static Box CreateEdgeBox(Vector2 size, Vector2 center)
        {
            Vector4 halfExtents = new(
                size.y * 0.5f,
                size.y * 0.5f,
                size.x * 0.5f,
                size.x * 0.5f
            );

            return new Box(halfExtents, center);
        }
    }

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
        edgeBoxes.UpdateBoxes(defaultSizeW, defaultSizeH, minMaxHorizontal, minMaxVertical, offset);

        var mouse = Mouse.current.position.ReadValue();
        
        if (edgeBoxes.leftBox.Contains(mouse))
            offset.x -= Time.deltaTime * speed;
        if (edgeBoxes.rightBox.Contains(mouse))
            offset.x += Time.deltaTime * speed;
        offset.x = Mathf.Clamp(offset.x, -minMaxHorizontal.x, minMaxHorizontal.y);
        if (edgeBoxes.upBox.Contains(mouse))
            offset.y += Time.deltaTime * speed;
        if (edgeBoxes.downBox.Contains(mouse))
            offset.y -= Time.deltaTime * speed;
        offset.y = Mathf.Clamp(offset.y, -minMaxVertical.x, minMaxVertical.y);

        var cameraRotation = Quaternion.LookRotation(currentRoom.cameraViewPoint, Vector3.up);
        desiredOffset = cameraRotation * offset;
        
        var newOffset = Vector3.Lerp(
            lastAppliedOffset,
            desiredOffset,
            Time.deltaTime * offsetLerpSpeed
        );
        
        camInterestPoint.position += (newOffset - lastAppliedOffset);
        lastAppliedOffset = newOffset;
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
