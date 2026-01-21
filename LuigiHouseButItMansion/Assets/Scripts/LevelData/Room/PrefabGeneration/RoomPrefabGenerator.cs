using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using Random = System.Random;

#if UNITY_EDITOR
public class RoomPrefabGenerator : MonoBehaviour
{
    [Header("Room data")]
    public GameObject levelCollision;
    public GoopData goopData;
    public Bounds goopBounds;
    public RoomCameraConfig roomCameraConfigRef;
    public Vector3 cameraViewPoint;
    [HideInInspector]
    public List<BaseRoomGeneratorComponent> roomGeneratorComponents = new();

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(goopBounds.center + levelCollision.transform.localPosition + transform.position, goopBounds.size);
    }

    public Vector3 GetPositionFromPointData<T>(T point) where T : PointDataHolder
    {
        Vector3 offset = point.transform.position - levelCollision.transform.position;
        Vector3 rotated = levelCollision.transform.rotation * offset;
        return rotated + levelCollision.transform.position;
    }

    public void UpdateAllLists() => roomGeneratorComponents.ForEach(x => x.UpdateList());

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        var offset = Vector3.up * 5;
        var startPosition = transform.position;
        var endPosition = cameraViewPoint * 2;
        Gizmos.DrawLine(startPosition + offset, startPosition + endPosition + offset);
    }

    public void SaveAndGenerateAsPrefab()
    {
        if (levelCollision == null)
        {
            Debug.LogError("Cannot generate prefabs if levelCollision is null");
            return;
        }

        var roomObjectData = new GameObject(levelCollision.name).AddComponent<RoomObjectData>();
        var goopManager = roomObjectData.GetComponent<GoopManager>();
        Instantiate(levelCollision, roomObjectData.transform);

        roomGeneratorComponents.ForEach(x => x.Generate(roomObjectData));

        // goopManager.Init(goopBounds, goopData); TODO
        // roomObjectData.Init(roomCameraConfigRef, cameraViewPoint); TODO

        // string prefabPath = GetPrefabPath();
    }

    private string GetPrefabPath()
    {
        string basePath = "Assets/Resources/Rooms";
        string roomName = levelCollision.name;

        bool TryString(string bP, string rN, out string fP)
        {
            fP = $"{basePath}/{roomName}";
            var folderExists = AssetDatabase.IsValidFolder(fP);
            if (folderExists) return false;
            
            AssetDatabase.CreateFolder(basePath, roomName);
            return true;
        }
        
        if (TryString(basePath, roomName, out string prefabPath))
            return prefabPath;

        int counter = 0;
        while (true)
        {
            counter++;
            string toAdd = $"({counter})";
            if (TryString(basePath, roomName+toAdd, out string pP))
                return pP;
        }
    }
}
#endif