using System;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
public class RoomPrefabGenerator : MonoBehaviour
{
    [Header("Prefabs")]
    public GameObject entrancePrefab;
    [Header("Room data")]
    public GameObject levelCollision;
    public List<InteractionPointDataHolder> interactionPoints = new();
    public List<EntrancePointDataHolder> entrancePoints;

    private void OnValidate()
    {
        if (levelCollision == null)
            enabled = false;
        for (var i = 0; i < interactionPoints.Count; i++)
        {
            if (interactionPoints[i] != null)
                continue;
            var obj = new GameObject("InteractionPointDataHolder");
            obj.transform.SetParent(transform);
            interactionPoints[i] = obj.AddComponent<InteractionPointDataHolder>();
        }
        for (var i = 0; i < entrancePoints.Count; i++)
        {
            if (entrancePoints[i] != null)
                continue;
            var obj = new GameObject("EntrancePointDataHolder");
            obj.transform.SetParent(transform);
            entrancePoints[i] = obj.AddComponent<EntrancePointDataHolder>();
        }
    }

    private void OnDrawGizmos()
    {
        void DrawPositions<T>(List<T> list) where T : PointDataHolder
        {
            foreach (var point in list)
            {
                Gizmos.color = point.GetColor();
                if (point == null)
                    continue;
                Gizmos.DrawSphere(GetPositionFromPointData(point), 0.1f);
            }
        }
        DrawPositions(interactionPoints);
        DrawPositions(entrancePoints);
    }

    private Vector3 GetPositionFromPointData<T>(T point) where T : PointDataHolder
    {
        Vector3 offset = point.transform.position - levelCollision.transform.position;
        Vector3 rotated = levelCollision.transform.rotation * offset;
        return rotated + levelCollision.transform.position;
    }

    public void UpdateAllLists()
    {
        UpdateEntranceList();
        UpdateInteractableList();
    }

    public void UpdateInteractableList()
    {
        interactionPoints.Clear();
        foreach (var data in GetComponentsInChildren<InteractionPointDataHolder>())
        {
            interactionPoints.Add(data);
        }
    }
    
    public void UpdateEntranceList()
    {
        entrancePoints.Clear();
        foreach (var data in GetComponentsInChildren<EntrancePointDataHolder>())
        {
            entrancePoints.Add(data);
        }
    }
}
#endif