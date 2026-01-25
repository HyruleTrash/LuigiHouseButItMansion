using System;
using System.Collections.Generic;
using UnityEngine;

public abstract class BaseRoomGeneratorComponent : MonoBehaviour
{
    protected RoomPrefabGenerator parent;
    public Vector2Int minMaxChosenFromList;
    protected abstract List<PointDataHolder> GetList();
    protected abstract Type GetGenType();
    
    protected virtual void OnValidate()
    {
        parent = GetComponent<RoomPrefabGenerator>();
        if (parent ==null || parent.levelCollision == null)
            enabled = false;
        if (!parent.roomGeneratorComponents.Contains(this))
            parent.roomGeneratorComponents.Add(this);
        TurnNullIntoChild(GetList());
    }
    
    private void OnDrawGizmos() => DrawPositions(GetList());

    public abstract void UpdateList();
    public abstract bool CanGenerate();
    public abstract void Generate(RoomObjectData roomObjectData);

    private void OnDestroy()
    {
        if (parent == null)
            return;
        if (parent.roomGeneratorComponents.Contains(this))
            parent.roomGeneratorComponents.Remove(this);
    }

    private void TurnNullIntoChild(List<PointDataHolder> list)
    {
        for (var i = 0; i < list.Count; i++)
        {
            if (list[i] != null)
                continue;
            var obj = new GameObject(GetGenType().Name);
            obj.transform.SetParent(parent.transform);
            list[i] = (PointDataHolder)obj.AddComponent(GetGenType());
        }
    }

    private void DrawPositions<T>(List<T> list) where T : PointDataHolder
    {
        foreach (var point in list)
        {
            Gizmos.color = point.GetColor();
            if (point == null)
                continue;
            Gizmos.DrawSphere(parent.GetPositionFromPointData(point), 0.1f);
        }
    }
}