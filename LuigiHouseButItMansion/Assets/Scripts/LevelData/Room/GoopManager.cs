
using System;
using System.Collections.Generic;
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
    private const int textureSize = 128;
    [HideInInspector]
    [CanBeNull] public Texture3D roomTexture = null;
    private NativeArray<byte> roomTextureData;
    
    private Vector3 CalculateGoopTexUV(Vector3 worldPos)
    {
        Vector3 goopUV = DivideV3(worldPos - GetRoomMin(), roomBounds.size);
        
        return new Vector3(
            Math.Clamp(goopUV.x, 0, 1),
            Math.Clamp(goopUV.y, 0, 1),
            Math.Clamp(goopUV.z, 0, 1)
        );
    }

    private bool RayProjectToBounds(
        Vector3 origin,
        Vector3 dir,
        out float tHit
    )
    {
        Bounds b = roomBounds;
        b.center += parent.transform.position;

        float tMin = float.NegativeInfinity;
        float tMax = float.PositiveInfinity;

        Vector3 min = b.min;
        Vector3 max = b.max;

        for (int i = 0; i < 3; i++)
        {
            float o = origin[i];
            float d = dir[i];

            if (Mathf.Abs(d) < 1e-6f)
            {
                if (o < min[i] || o > max[i])
                {
                    tHit = 0;
                    return false;
                }
            }
            else
            {
                float invD = 1f / d;
                float t1 = (min[i] - o) * invD;
                float t2 = (max[i] - o) * invD;

                if (t1 > t2) (t1, t2) = (t2, t1);

                tMin = Mathf.Max(tMin, t1);
                tMax = Mathf.Min(tMax, t2);

                if (tMin > tMax)
                {
                    tHit = 0;
                    return false;
                }
            }
        }

        // inside vs outside
        tHit = tMin >= 0f ? tMin : tMax;
        return tHit >= 0f;
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
        if (instance.roomTexture)
            return;
        
        var format = GraphicsFormat.R8_UNorm;
        const float temp = textureSize - 1;
        instance.roomTexture = new Texture3D(textureSize,
            textureSize,
            textureSize,
            format,
            TextureCreationFlags.DontUploadUponCreate)
        {
            wrapMode = TextureWrapMode.Clamp,
            filterMode = FilterMode.Point,
        };
                
        byte[] rawData = new byte[(int)MathF.Pow(textureSize,
            3)];
        
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
            // for (int z = 0; z < size; z++)
            // {
            //     for (int y = 0; y < size; y++)
            //     {
            //         for (int x = 0; x < size; x++)
            //         {
            //             // Calculate position in 0-1 range for each axis
            //             float px = x / temp;
            //             float py = y / temp;
            //             float pz = z / temp;
            //                 
            //             // Create a smooth 3D gradient
            //             float value = (px + py + pz) / 3.0f;
            //                 
            //             // Convert to byte value (0-255)
            //             rawData[z * size * size + y * size + x] = (byte)(value * 255);
            //         }
            //     }
            // }
            var rand = new System.Random();
            for (int z = 0; z < textureSize; z++)
            {
                for (int y = 0; y < textureSize; y++)
                {
                    for (int x = 0; x < textureSize; x++)
                    {
                        // Generate random value between 0 and 255
                        byte value = (byte)rand.Next(0, 256);
                    
                        // Store in array
                        rawData[z * textureSize * textureSize + y * textureSize + x] = value;
                    }
                }
            }
            instance.roomTextureData = new NativeArray<byte>(rawData.Length,
                Allocator.Persistent);
        }
                
        instance.roomTexture.SetPixelData(rawData,
            0);
        instance.roomTexture.Apply(false,
            false);
    }
    
    public void RemoveGoopAt(Vector3 contactPoint, Vector3 collisionNormal)
    {
        if (!parent || !parent.gameObject || !parent.transform)
        {
            Debug.LogError($"Missing required component in {gameObject?.name}: Parent={parent}, ParentGO={parent?.gameObject}, ParentTransform={parent?.transform}");
            return;
        }
        if (!roomTexture)
        {
            UpdateTexture(this);
            if (!roomTexture)
            {
                Debug.LogError($"Failed to initialize room texture in {gameObject.name}");
                return;
            }
        }
        
        const int brushSize = 5;
        
        using (NativeArray<byte> collection = roomTexture.GetPixelData<byte>(0))
        {
            var pixels = collection;

            Vector3 baseUV = CalculateGoopTexUV(contactPoint);
            float texel = 1f / (textureSize - 1);

            for (int dx = -brushSize; dx <= brushSize; dx++)
            for (int dy = -brushSize; dy <= brushSize; dy++)
            for (int dz = -brushSize; dz <= brushSize; dz++)
            {
                Vector3 uv = baseUV + new Vector3(dx, dy, dz) * texel;
                uv = Vector3.Min(Vector3.one, Vector3.Max(Vector3.zero, uv));

                int x = Mathf.RoundToInt(uv.x * (textureSize - 1));
                int y = Mathf.RoundToInt(uv.y * (textureSize - 1));
                int z = Mathf.RoundToInt(uv.z * (textureSize - 1));

                int index = z * textureSize * textureSize + y * textureSize + x;
                pixels[index] = 0;
            }
            
            roomTextureData = new NativeArray<byte>(pixels.Length,
                Allocator.Persistent);
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