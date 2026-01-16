
using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
#if UNITY_EDITOR
using System.IO;
using UnityEditor;
#endif

[RequireComponent(typeof(RoomObjectData))]
public class GoopManager : MonoBehaviour
{
    private static readonly int GoopMask = Shader.PropertyToID("_GoopMask");
    private static readonly int RoomMin = Shader.PropertyToID("_RoomMin");
    private static readonly int RoomSize = Shader.PropertyToID("_RoomSize");

    [HideInInspector] public RoomObjectData parent;
    
    [SerializeField] 
    private GoopData goopData;
    private int? actualBrushSize;
    [SerializeField]
    private Bounds roomBounds;
    [CanBeNull] public Texture3D roomTexture = null;
    [HideInInspector] public Texture3D usedRoomTexture;
    private byte[] roomTextureData;

    private void Start()
    {
        actualBrushSize = null;
    }

    public void SetUsedRoomTexture()
    {
        if (roomTexture == null)
            return;

        var res = GetTextureResolution();
        usedRoomTexture = new Texture3D(
            res.x,
            res.y,
            res.z,
            roomTexture.graphicsFormat,
            TextureCreationFlags.None)
        {
            wrapMode = TextureWrapMode.Clamp,
            filterMode = FilterMode.Point,
        };
        usedRoomTexture.SetPixelData(roomTexture.GetPixelData<byte>(0), 0);
        usedRoomTexture.Apply(false, false);
        SaveTextureData(usedRoomTexture);
    }

    private Vector3 CalculateGoopTexUV(Vector3 worldPos)
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
        goopData.SetGlobalShaderData();
        Shader.SetGlobalTexture(GoopMask, usedRoomTexture);
    }
    
    private Vector3Int GetTextureResolution()
    {
        Vector3 size = roomBounds.size;

        int x = goopData.textureSize;
        float ratioY = size.y / size.x;
        float ratioZ = size.z / size.x;

        int y = Mathf.Max(1, Mathf.RoundToInt(x * ratioY));
        int z = Mathf.Max(1, Mathf.RoundToInt(x * ratioZ));

        return new Vector3Int(x, y, z);
    }
    
    #if UNITY_EDITOR
    public void GenerateTexture()
    {
        var tempTex = Resources.Load<Texture3D>($"Rooms/{gameObject.name}/roomTexture");
        if (tempTex == null)
        {
            var res = GetTextureResolution();
            var format = GraphicsFormat.R8_UNorm;
            roomTexture = new Texture3D(
                res.x,
                res.y,
                res.z,
                format,
                TextureCreationFlags.DontUploadUponCreate)
            {
                wrapMode = TextureWrapMode.Clamp,
                filterMode = FilterMode.Point,
            };
            
            string basePath = Path.Combine(Application.dataPath, "Resources");
            string roomPath = Path.Combine(basePath, "Rooms", gameObject.name);
            Directory.CreateDirectory(roomPath);
            
            EditorUtility.SetDirty(roomTexture);
            AssetDatabase.CreateAsset(roomTexture, $"Assets/Resources/Rooms/{gameObject.name}/roomTexture.asset");
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            
            ApplyDefaultTexture();
        }
        else
        {
            roomTexture = tempTex;
        }
        SaveTextureData(roomTexture);
        roomTexture.Apply(false, false);
    }
    #endif
    
    public void UpdateTexture()
    {
        if (roomTextureData != null && usedRoomTexture)
        {
            usedRoomTexture.SetPixelData(roomTextureData, 0);
            usedRoomTexture.Apply(false, false);
            return;
        }
        
        if (!usedRoomTexture) SetUsedRoomTexture();
        if (!usedRoomTexture)
        {
            Debug.LogError($"Failed to initialize room texture in {gameObject.name}");
        }
    }

    private void ApplyDefaultTexture()
    {
        if (!roomTexture)
            return;
        var res = GetTextureResolution();
        var rawData = new byte[res.x * res.y * res.z];
            
        // Calculate gradient in 3D space
        float tempX = res.x - 1;
        float tempY = res.y - 1;
        float tempZ = res.z - 1;
        for (int z = 0; z < res.z; z++)
        {
            for (int y = 0; y < res.y; y++)
            {
                for (int x = 0; x < res.x; x++)
                {
                    // Calculate position in 0-1 range for each axis
                    float px = x / tempX;
                    float py = y / tempY;
                    float pz = z / tempZ;
                            
                    // Create a smooth 3D gradient
                    float value = (px + py + pz) / 3.0f;
                            
                    // Convert to byte value (0-255)
                    rawData[z * res.x * res.y + y * res.x + x] = (byte)(value * 255);
                }
            }
        }
        // var rand = new System.Random();
        // for (int z = 0; z < goopData.textureSize; z++)
        // {
        //     for (int y = 0; y < goopData.textureSize; y++)
        //     {
        //         for (int x = 0; x < goopData.textureSize; x++)
        //         {
        //             // Generate random value between 0 and 255
        //             byte value = (byte)rand.Next(0, 256);
        //         
        //             // Store in array
        //             rawData[z * goopData.textureSize * goopData.textureSize + y * goopData.textureSize + x] = value;
        //         }
        //     }
        // }
        
        roomTexture.SetPixelData(rawData, 0);
    }
    
    private int GetBrushSize(Vector3Int res)
    {
        var metersPerTexelX = roomBounds.size.x / (res.x - 1);
        actualBrushSize ??= Mathf.RoundToInt(goopData.brushSize / metersPerTexelX);
        return actualBrushSize.Value;
    }
    
    public void RemoveGoopAt(Vector3 contactPoint, Vector3 collisionNormal)
    {
        if (!parent || !parent.gameObject || !parent.transform)
        {
            Debug.LogError($"Missing required component in {gameObject?.name}: Parent={parent}, ParentGO={parent?.gameObject}, ParentTransform={parent?.transform}");
            return;
        }
        if (!usedRoomTexture)
        {
            SetUsedRoomTexture();
            if (!usedRoomTexture)
            {
                Debug.LogError($"Failed to initialize room texture in {gameObject.name}");
                return;
            }
        }
        
        Vector3 baseUV = CalculateGoopTexUV(contactPoint);
        var res = GetTextureResolution();
        var texelSize = new Vector3(
            1f / (res.x - 1),
            1f / (res.y - 1),
            1f / (res.z - 1)
        );
        
        int brushRadius = GetBrushSize(res);
        
        var madeChanges = false;
        for (var dx = -brushRadius; dx <= brushRadius; dx++)
        for (var dy = -brushRadius; dy <= brushRadius; dy++)
        for (var dz = -brushRadius; dz <= brushRadius; dz++)
        {
            var translation = new Vector3(dx, dy, dz);
            var pos = GetIntArrayUvFromTranslation(baseUV, translation, texelSize, res);
            
            float distance = math.length(new float3(dx, dy, dz));
            if (distance > brushRadius)
                continue;
            
            int index = VectorUvToIndex(pos[0], pos[1], pos[2], res);
            if (roomTextureData[index] == 0)
                continue;
            
            float interpolationFactor = 1f - (distance / (brushRadius + 1f));
            roomTextureData[index] = (byte)(roomTextureData[index] * interpolationFactor);
            madeChanges = true;
        }

        if (madeChanges)
            usedRoomTexture.SetPixelData(roomTextureData, 0);
        usedRoomTexture.Apply();
    }

    private int[] GetIntArrayUvFromTranslation(Vector3 baseUV, Vector3 translation, Vector3 texelSize, Vector3Int res)
    {
        Vector3 uv = baseUV + Vector3.Scale(translation, texelSize);
        uv = Vector3.Min(Vector3.one, Vector3.Max(Vector3.zero, uv));

        int x = Mathf.RoundToInt(uv.x * (res.x - 1));
        int y = Mathf.RoundToInt(uv.y * (res.y - 1));
        int z = Mathf.RoundToInt(uv.z * (res.z - 1));

        return new[] { x, y, z };
    }

    private int VectorUvToIndex(int x, int y, int z, Vector3Int res)
    {
        return z * res.x * res.y + y * res.x + x;
    }
    
    public void SaveTextureData(Texture3D texture)
    {
        if (!texture)
            return;
        using var collection = texture.GetPixelData<byte>(0);
        roomTextureData = new byte[collection.Length];
        collection.CopyTo(roomTextureData);
    }
    
    public void ClearTextureData() => roomTextureData = null;

    public void DestroyTexture()
    {
        roomTexture = null;
        #if UNITY_EDITOR
        var tempTex = Resources.Load<Texture3D>($"Rooms/{gameObject.name}/roomTexture");
        if (!tempTex)
            return;
        Resources.UnloadAsset(tempTex);
        var assetPath = Path.Combine(
            Application.dataPath,
            "Resources",
            "Rooms",
            gameObject.name,
            "roomTexture");
        File.Delete(assetPath + ".asset");
        File.Delete(assetPath + ".meta");
        AssetDatabase.Refresh();
        #endif
    }

    #if UNITY_EDITOR
    public void OnDrawGizmosSelected()
    {
        Gizmos.DrawWireCube(roomBounds.center + parent.transform.position, roomBounds.size);
    }
    #endif
}