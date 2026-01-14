
using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Serialization;

public class RoomObjectData : MonoBehaviour
{
    private static readonly int GoopMask = Shader.PropertyToID("_GoopMask");
    private static readonly int GoopColor = Shader.PropertyToID("_GoopColor");
    private static readonly int RoomMin = Shader.PropertyToID("_RoomMin");
    private static readonly int RoomSize = Shader.PropertyToID("_RoomSize");
    private static readonly int CutOffThreshold = Shader.PropertyToID("_CutOffThreshold");
    private static Vector3 setRoomMin;
    private static Vector3 setRoomSize;
    private static Color setRoomColor;
    private static Texture2D setRoomGoopTex;
    private static float setRoomCutOffThreshold;

    private List<RoomEntrance> entrances;
    public Vector3 cameraViewPoint;
    public RoomCameraConfig cameraConfig;
    public Action OnReadyRoom;
    public InteractableObjectsManager interactableObjectsManager;

    [SerializeField] private bool shouldResetTex = false;
    [SerializeField]
    private Texture2D roomGoopTex = null;
    
    [SerializeField] 
    private GoopData goopData;
    [SerializeField]
    private Bounds roomBounds;


    private void Start()
    {
        RoomManager.instance.LiveRooms.Add(this);
        setRoomGoopTex = null;
    }

    private void OnDestroy()
    {
        RoomManager.instance.LiveRooms.Remove(this);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        var offset = Vector3.up * 5;
        var startPosition = transform.position;
        var endPosition = cameraViewPoint * 2;
        Gizmos.DrawLine(startPosition + offset, startPosition + endPosition + offset);

        SetGlobalShaderGoopData();
        Gizmos.DrawWireCube(roomBounds.center + transform.position, roomBounds.size);
    }

    private void OnValidate()
    {
        if (!Application.isPlaying)
        {
            if (shouldResetTex)
            {
                roomGoopTex = null;
                shouldResetTex = false;
            }
            SetGlobalShaderGoopData();
        }
    }

    public void AddEntrance(RoomEntrance entrance)
    {
        entrances ??= new List<RoomEntrance>();
        entrances.Add(entrance);
        entrance.UnLock();
    }

    public void ReadyRoom()
    {
        foreach (var entrance in entrances)
        {
            entrance.enabled = true;
        }

        UnLockDoors();
        OnReadyRoom?.Invoke();

        SetGlobalShaderGoopData();
    }

    public void SetGlobalShaderGoopData()
    {
        if (goopData == null)
        {
            setRoomGoopTex = null;
        }
        else
        {
            if (roomGoopTex == null)
            {
                var format = GraphicsFormat.R8_UNorm;
                roomGoopTex = new Texture2D(512, 512, format, TextureCreationFlags.None);
                
                byte[] rawData = new byte[512 * 512];
                for (int i = 0; i < rawData.Length; i++)
                {
                    rawData[i] = (byte)(i * 255 / (512 * 512 - 1));  // Continuous progression
                }
                
                roomGoopTex.SetPixelData(rawData, 0);
                roomGoopTex.Apply();
            }

            setRoomGoopTex = roomGoopTex;
            setRoomMin = roomBounds.min + transform.position;
            setRoomSize = roomBounds.size;
            setRoomColor = goopData.goopColor;
            setRoomCutOffThreshold = goopData.cutoffThreshold;
        }

        Shader.SetGlobalTexture(GoopMask, setRoomGoopTex);
        Shader.SetGlobalVector(RoomMin, setRoomMin);
        Shader.SetGlobalVector(RoomSize, setRoomSize);
        Shader.SetGlobalColor(GoopColor, setRoomColor);
        Shader.SetGlobalFloat(CutOffThreshold, setRoomCutOffThreshold);
    }

    public void DisableRoom()
    {
        foreach (var entrance in entrances)
        {
            entrance.enabled = false;
        }
    }

    public void LockDoors()
    {
        foreach (var entrance in entrances)
        {
            entrance.Lock();
        }
    }

    public void UnLockDoors()
    {
        foreach (var entrance in entrances)
        {
            entrance.UnLock();
        }
    }
}