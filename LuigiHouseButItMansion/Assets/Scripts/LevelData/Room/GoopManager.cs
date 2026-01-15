
using System;
using JetBrains.Annotations;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(RoomObjectData))]
public class GoopManager : MonoBehaviour
{
    private static readonly int GoopMask = Shader.PropertyToID("_GoopMask");
    private static readonly int GoopColor = Shader.PropertyToID("_GoopColor");
    private static readonly int RoomMin = Shader.PropertyToID("_RoomMin");
    private static readonly int RoomSize = Shader.PropertyToID("_RoomSize");
    private static readonly int CutOffThreshold = Shader.PropertyToID("_CutOffThreshold");

    [HideInInspector] public RoomObjectData parent;
    
    [SerializeField] 
    private GoopData goopData;
    #if UNITY_EDITOR
    private GoopData oldGoopData;
    #endif
    [SerializeField]
    private Bounds roomBounds;
    [HideInInspector]
    [CanBeNull] public Texture3D roomTexture = null;
    private NativeArray<byte> roomTextureData;
    
    private Vector2 CalculateGoopTexUV(Vector3 worldPos)
    {
        Vector3 goopUV = DivideV3(worldPos - GetRoomMin(), roomBounds.size);
        
        return new Vector3(
            Math.Clamp(goopUV.x, 0, 1),
            Math.Clamp(goopUV.y, 0, 1),
            Math.Clamp(goopUV.z, 0, 1)
        );
    }

    private static Vector3 DivideV3(Vector3 a, Vector3 b) => new Vector3(a.x / b.x, a.y / b.y, a.z / b.z);

    public Vector3 GetRoomMin() => roomBounds.min + parent.transform.position;

    public void SetGlobalShaderData()
    {
        Shader.SetGlobalVector(RoomMin, GetRoomMin());
        Shader.SetGlobalVector(RoomSize, roomBounds.size);
        Shader.SetGlobalColor(GoopColor, goopData.goopColor);
        Shader.SetGlobalFloat(CutOffThreshold, goopData.cutoffThreshold);
        Shader.SetGlobalTexture(GoopMask, roomTexture);
    }
    
    public static void UpdateTexture(GoopManager instance)
    {
        Debug.Log($"updating texture! {instance.GetInstanceID()}");
        if (instance.roomTexture)
            return;
        
        var format = GraphicsFormat.R8_UNorm;
        const int size = 128;
        const float temp = size - 1;
        instance.roomTexture = new Texture3D(size, size, size, format, TextureCreationFlags.DontUploadUponCreate);
                
        byte[] rawData = new byte[(int)MathF.Pow(size, 3)];
        
        if (instance.roomTextureData.IsCreated)
        {
            for (var i = 0; i < rawData.Length; i++)
            {
                rawData[i] = instance.roomTextureData[i];
            }
        }
        else
        {
            // Calculate gradient in 3D space
            for (int z = 0; z < size; z++)
            {
                for (int y = 0; y < size; y++)
                {
                    for (int x = 0; x < size; x++)
                    {
                        // Calculate position in 0-1 range for each axis
                        float px = x / temp;
                        float py = y / temp;
                        float pz = z / temp;
                            
                        // Create a smooth 3D gradient
                        float value = (px + py + pz) / 3.0f;
                            
                        // Convert to byte value (0-255)
                        rawData[z * size * size + y * size + x] = (byte)(value * 255);
                    }
                }
            }
            instance.roomTextureData = new NativeArray<byte>(rawData.Length, Allocator.Persistent);
        }
                
        instance.roomTexture.SetPixelData(rawData, 0);
        instance.roomTexture.Apply(false, false);
        Debug.Log($"{instance.GetInstanceID()} texture updated!: {instance.roomTexture}");
    }

    public void RemoveGoopAt(Vector3 contactPoint)
    {
        if (!parent || !parent.gameObject || !parent.transform)
        {
            Debug.LogError($"Missing required component in {gameObject?.name}: Parent={parent}, ParentGO={parent?.gameObject}, ParentTransform={parent?.transform}");
            return;
        }
        if (!roomTexture)
        {
            Debug.Log(GetInstanceID() + " has no room texture");
            UpdateTexture(this);
            if (!roomTexture)
            {
                Debug.LogError($"Failed to initialize room texture in {gameObject.name}");
                return;
            }
        }
        
        // Convert world space contact point to UV coordinates matching shader logic
        Vector3 goopUV = CalculateGoopTexUV(contactPoint);
        
        // Create a small spherical brush for removal
        int brushSize = 5; // Adjust based on desired removal size
        
        // Ensure we stay within texture bounds
        int startX = Mathf.Max(0, Mathf.RoundToInt(goopUV.x * roomTexture.width) - brushSize); // Object reference is not set to an instance of an object
        int startY = Mathf.Max(0, Mathf.RoundToInt(goopUV.y * roomTexture.height) - brushSize);
        int startZ = Mathf.Max(0, Mathf.RoundToInt(goopUV.z * roomTexture.depth) - brushSize);
        
        int endX = Mathf.Min(roomTexture.width - 1, Mathf.RoundToInt(goopUV.x * roomTexture.width) + brushSize);
        int endY = Mathf.Min(roomTexture.height - 1, Mathf.RoundToInt(goopUV.y * roomTexture.height) + brushSize);
        int endZ = Mathf.Min(roomTexture.depth - 1, Mathf.RoundToInt(goopUV.z * roomTexture.depth) + brushSize);
        
        // Modify the texture data
        using (NativeArray<byte> collection = roomTexture.GetPixelData<byte>(0))
        {
            var pixels = collection;
            
            // Calculate center coordinates
            int centerX = Mathf.RoundToInt(goopUV.x * roomTexture.width);
            int centerY = Mathf.RoundToInt(goopUV.y * roomTexture.height);
            int centerZ = Mathf.RoundToInt(goopUV.z * roomTexture.depth);
        
            // Remove goop in a spherical region
            for (int z = startZ; z <= endZ; z++)
            {
                for (int y = startY; y <= endY; y++)
                {
                    for (int x = startX; x <= endX; x++)
                    {
                        // Calculate distance from center in 3D space
                        float distSq = (x - centerX) * (x - centerX) +
                                       (y - centerY) * (y - centerY) +
                                       (z - centerZ) * (z - centerZ);
                    
                        // Only affect pixels within brush radius
                        if (distSq <= brushSize * brushSize)
                        {
                            int index = z * roomTexture.width * roomTexture.height + 
                                        y * roomTexture.width + x;
                            pixels[index] = 0; // Set to minimum value (no goop)
                        }
                    }
                }
            }
            collection.CopyTo(roomTextureData);
        }
        roomTexture.Apply();
    }

    private void OnDestroy()
    {
        if (roomTextureData.IsCreated)
        {
            roomTextureData.Dispose();
        }
        if (roomTexture != null)
        {
            DestroyImmediate(roomTexture, true);
        }
    }

#if UNITY_EDITOR
    public void OnDrawGizmosSelected()
    {
        Gizmos.DrawWireCube(roomBounds.center + parent.transform.position, roomBounds.size);
    }
    #endif
    public void DestroyTexture()
    {
        if (roomTexture == null)
            return;
        using (NativeArray<byte> collection = roomTexture.GetPixelData<byte>(0))
        {
            roomTextureData = new NativeArray<byte>(collection.Length, Allocator.Persistent);
            collection.CopyTo(roomTextureData);
        }
        roomTexture = null;
    }
}