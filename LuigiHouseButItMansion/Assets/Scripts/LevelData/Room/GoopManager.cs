
using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using Random = System.Random;
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

    private static Vector3 DivideV3(Vector3 a, Vector3 b) => new Vector3(a.x / b.x, a.y / b.y, a.z / b.z);
    private Vector3 GetRoomMin() => roomBounds.min + parent.transform.position;
    
    public void SetGlobalShaderData()
    {
        Shader.SetGlobalVector(RoomMin, GetRoomMin());
        Shader.SetGlobalVector(RoomSize, roomBounds.size);
        goopData.SetGlobalShaderData();
        Shader.SetGlobalTexture(GoopMask, usedRoomTexture);
    }
    
    private void Start()
    {
        actualBrushSize = null;
    }

    public void SetUsedRoomTexture()
    {
        if (!roomTexture)
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

    private void ApplyDefaultTexture()
    {
        if (!roomTexture)
            return;
        var res = GetTextureResolution();
        var rawData = new byte[res.x * res.y * res.z];
        
        const float scale = 0.1f;
        var perlinDataX = GeneratePerlinArray(res.y, res.z, scale);
        var perlinDataY = GeneratePerlinArray(res.x, res.z, scale);
        var perlinDataZ = GeneratePerlinArray(res.y, res.x, scale);
        
        // insert perlin data into 3d texture
        for (int z = 0; z < res.z; z++)
        {
            for (int y = 0; y < res.y; y++)
            {
                for (int x = 0; x < res.x; x++)
                {
                    float usedX = perlinDataX[Idx2D(y, z, res.y)];
                    float usedY = perlinDataY[Idx2D(x, z, res.x)];
                    float usedZ = perlinDataZ[Idx2D(y, x, res.y)];
                    
                    var value = (usedX + usedY + usedZ) / 3.0f;
                    if (value > 0.5f)
                        rawData[z * res.x * res.y + y * res.x + x] = (byte)(value * 255f);
                    else
                        rawData[z * res.x * res.y + y * res.x + x] = (byte)0f;
                }
            }
        }

        rawData = SmoothNoiseTexData(rawData, res);
        rawData = SmoothNoiseTexData(rawData, res);
        
        roomTexture.SetPixelData(rawData, 0);
    }

    int Idx2D(int x, int y, int width) => y * width + x;

    private static float[] GeneratePerlinArray(int width, int height, float scale)
    {
        var perlinData = new float[height * width];
        var seed = UnityEngine.Random.Range(-100000, 100000);
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                float nx = x * scale + seed;
                float ny = y * scale + seed;
                float v = Mathf.PerlinNoise(nx, ny);
                perlinData[y * width + x] = v;
            }
        }
        return perlinData;
    }
    
    private byte[] SmoothNoiseTexData(byte[] rawData, Vector3Int res)
    {
        var newData = new byte[res.x * res.y * res.z];
        for (int z = 0; z < res.z; z++)
        {
            for (int y = 0; y < res.y; y++)
            {
                for (int x = 0; x < res.x; x++)
                {
                    int i = z * res.x * res.y + y * res.x + x;
                    float opacityStack = 0;
                    int neighbors = 0;

                    for (int dz = -1; dz <= 1; dz++)
                    for (int dy = -1; dy <= 1; dy++)
                    for (int dx = -1; dx <= 1; dx++)
                    {
                        int nx = x + dx;
                        int ny = y + dy;
                        int nz = z + dz;

                        if (nx < 0 || nx >= res.x ||
                            ny < 0 || ny >= res.y ||
                            nz < 0 || nz >= res.z)
                            continue;

                        int ni = nz * res.x * res.y + ny * res.x + nx;

                        neighbors++;
                        opacityStack += rawData[ni];
                    }
                    
                    newData[i] = (byte)(opacityStack / neighbors);
                }
            }
        }
        return newData;
    }
    
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

    #if UNITY_EDITOR
    public void OnDrawGizmosSelected()
    {
        Gizmos.DrawWireCube(roomBounds.center + parent.transform.position, roomBounds.size);
    }
    #endif
}