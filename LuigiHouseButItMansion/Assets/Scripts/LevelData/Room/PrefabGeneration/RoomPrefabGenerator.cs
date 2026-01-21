using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using Random = System.Random;

#if UNITY_EDITOR
public class RoomPrefabGenerator : MonoBehaviour
{
    [Header("Room data")]
    public GameObject levelCollision;
    public List<GoopData> goopData;
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
        } // TODO add more validation, especially per generator component

        var roomObjectData = new GameObject(levelCollision.name).AddComponent<RoomObjectData>();
        var goopManager = roomObjectData.GetComponent<GoopManager>();
        Instantiate(levelCollision, roomObjectData.transform);

        string prefabPath = GetPrefabPath(out string addition);
        roomObjectData.name += addition;
        
        roomGeneratorComponents.ForEach(x => x.Generate(roomObjectData));

        roomObjectData.Init(roomCameraConfigRef, cameraViewPoint);
        goopManager.Init(goopBounds, goopData[UnityEngine.Random.Range(0, goopData.Count)]);

        PrefabUtility.SaveAsPrefabAsset(roomObjectData.gameObject, Path.Combine(prefabPath, $"{roomObjectData.name}.prefab"));
        DestroyImmediate(roomObjectData.gameObject);
    }

    private string GetPrefabPath(out string addition)
    {
        string basePath = "Assets/Resources/Rooms";
        string roomName = levelCollision.name;

        bool TryString(string bP, string rN, out string fP)
        {
            fP = $"{bP}/{rN}";
            var folderExists = AssetDatabase.IsValidFolder(fP);
            if (folderExists) return false;
            
            AssetDatabase.CreateFolder(bP, rN);
            return true;
        }

        int counter = -1;
        while (true)
        {
            counter++;
            addition = $"({counter})";
            if (counter == 0)
                addition = "";
            if (TryString(basePath, roomName+addition, out string pP))
                return pP;
        }
    }
}
#endif