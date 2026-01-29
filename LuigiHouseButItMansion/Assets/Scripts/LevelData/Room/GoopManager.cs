
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
    [SerializeField]
    private Bounds roomBounds;
    [CanBeNull] public Texture3D roomTexture = null;
    [HideInInspector] public Texture3D usedRoomTexture;
    private byte[] roomTextureData;

#if UNITY_EDITOR
    /// <summary>
    /// Editor function for initializing values
    /// </summary>
    public void Init(Bounds goopBounds, GoopData goopData1)
    {
        roomBounds = goopBounds;
        goopData = goopData1;
        Regenerate();
        SetToCurrent();
    }
#endif
    
    private static Vector3 DivideV3(Vector3 a, Vector3 b) => new Vector3(a.x / b.x, a.y / b.y, a.z / b.z);
    private Vector3 GetRoomMin() => roomBounds.min + parent.transform.position;
    
    public void SetGlobalShaderData()
    {
        Shader.SetGlobalVector(RoomMin, GetRoomMin());
        Shader.SetGlobalVector(RoomSize, roomBounds.size);
        goopData.SetGlobalShaderData();
        Shader.SetGlobalTexture(GoopMask, usedRoomTexture);
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
        var rOffsetX = UnityEngine.Random.Range(-width, width);
        var rOffsetY = UnityEngine.Random.Range(-height, height);
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                float nx = x * scale + seed + rOffsetX;
                float ny = y * scale + seed + rOffsetY;
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
    
    private struct TexelData
    {
        public Vector3 baseUV;
        public Vector3 texelSize;
        public Vector3Int res;
    }
    
    public void RemoveGoopAt(Vector3 contactPoint, Func<Vector3Int, Bounds, float> calculateBrush)
    {
        if (!ValidateGoop()) return;
        
        TexelData texelData = CalculateTexelData(contactPoint);
        float brushRadius = calculateBrush.Invoke(texelData.res, roomBounds);
        var amountOfTexelsChanged = ApplyCircleToTexture(brushRadius, texelData, (current, distance, brushRadius) =>
        {
            float interpolationFactor = 1f - (distance / (brushRadius + 1f));
            return (byte)(current * interpolationFactor);
        });

        var scoreCounter = SceneData.instance.GetRegisteredObject<ScoreCounter>();
        if (scoreCounter == null) return;
        const float modifier = 20f;
        scoreCounter.CleanCount += TexelCountToWorldVolume(amountOfTexelsChanged) / modifier;
    }
    
    public void ApplyGoopAt(Vector3 contactPoint, Func<Vector3Int, Bounds, float> calculateBrush)
    {
        if (!ValidateGoop()) return;
        
        TexelData texelData = CalculateTexelData(contactPoint);
        float brushRadius = calculateBrush.Invoke(texelData.res, roomBounds);
        ApplyCircleToTexture(brushRadius, texelData, (current, distance, brushRadius) =>
        {
            float interpolationFactor = 1f - (distance / (brushRadius + 1f));
            return (byte)(current + (255 - current) * interpolationFactor);
        });
    }

    /// <summary>
    /// Validates current state of manager, and texture
    /// </summary>
    /// <returns>false when not passing validation, true when validation was passed</returns>
    private bool ValidateGoop()
    {
        if (!parent || !parent.gameObject || !parent.transform)
        {
            Debug.LogError($"Missing required component in {gameObject?.name}: Parent={parent}, ParentGO={parent?.gameObject}, ParentTransform={parent?.transform}");
            return false;
        }
        if (!usedRoomTexture)
        {
            Debug.Log("usedroomTex missing"); // not the issue doesnt get triggered
            SetUsedRoomTexture();
            if (!usedRoomTexture)
            {
                Debug.LogError($"Failed to initialize room texture in {gameObject.name}");
                return false;
            }
        }
        return true;
    }

    private TexelData CalculateTexelData(Vector3 point)
    {
        Vector3 baseUV = CalculateGoopTexUV(point);
        var res = GetTextureResolution();
        var texelSize = new Vector3(
            1f / (res.x - 1),
            1f / (res.y - 1),
            1f / (res.z - 1)
        );
        
        return new TexelData
        {
            baseUV = baseUV,
            texelSize = texelSize,
            res = res
        };
    }
    
    private float TexelCountToWorldVolume(int texelCount)
    {
        var res = GetTextureResolution();
        Vector3 roomSize = roomBounds.size;

        float texelVolume =
            (roomSize.x / res.x) *
            (roomSize.y / res.y) *
            (roomSize.z / res.z);

        return texelCount * texelVolume;
    }


    private int ApplyCircleToTexture(float brushRadius, TexelData texelData, Func<byte, float, float, byte> mutation)
    {
        var madeChanges = false;
        var amountOfChange = 0;
        for (var dx = -brushRadius; dx <= brushRadius; dx++)
        for (var dy = -brushRadius; dy <= brushRadius; dy++)
        for (var dz = -brushRadius; dz <= brushRadius; dz++)
        {
            var translation = new Vector3(dx, dy, dz);
            var pos = GetIntArrayUvFromTranslation(texelData.baseUV, translation, texelData.texelSize, texelData.res);
            
            float distance = math.length(new float3(dx, dy, dz));
            if (distance > brushRadius)
                continue;
            
            int index = VectorUvToIndex(pos[0], pos[1], pos[2], texelData.res);
            if (roomTextureData[index] == 0)
                continue;
            
            roomTextureData[index] = mutation.Invoke(roomTextureData[index], distance, brushRadius);
            madeChanges = true;
            amountOfChange++;
        }

        if (madeChanges)
            usedRoomTexture.SetPixelData(roomTextureData, 0);
        usedRoomTexture.Apply();
        return amountOfChange;
    }

#if UNITY_EDITOR
    public void OnDrawGizmosSelected()
    {
        Gizmos.DrawWireCube(roomBounds.center + parent.transform.position, roomBounds.size);
    }
    
    public void Regenerate()
    {
        DestroyTexture();
        ClearTextureData();
        GenerateTexture();
    }
#endif

    public void SetToCurrent()
    {
        if (!usedRoomTexture)
            SetUsedRoomTexture();
        UpdateTexture();
        SetGlobalShaderData();
    }
}