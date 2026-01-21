
using UnityEngine;
using UnityEditor;

public class EntrancePointDataHolder : PointDataHolder
{
    protected override BaseRoomGeneratorComponent GetParentComponent() => transform.parent.GetComponent<EntrancePointsGenerator>();

    protected override void AddSelfToParent()
    {
        EntrancePointsGenerator parent = (EntrancePointsGenerator)parentGenerator;
        if (!parent.entrancePoints.Contains(this))
            parent.entrancePoints.Add(this);
    }

    public override Color GetColor() => Color.orangeRed;

    protected override void OnDrawGizmosSelected()
    {
        base.OnDrawGizmosSelected();
        
        if (!parentGenerator)
            return;
        EntrancePointsGenerator parent = (EntrancePointsGenerator)parentGenerator;
        if (parent.entrancePrefab == null)
            return;
        Gizmos.color = GetColor();
        Mesh mesh = parent.entrancePrefab.GetComponent<RoomEntrance>().doorRenderObject.sharedMesh;
        Gizmos.DrawWireMesh(mesh, transform.position, interactableObjRotation * Quaternion.Euler(-90, 0, 0));
    }

    public void CreateInstance(RoomObjectData roomObjectData, RoomPrefabGenerator parent, GameObject entrancePrefab)
    {
        var instance = Instantiate(entrancePrefab, parent.GetPositionFromPointData(this), interactableObjRotation);
        instance.transform.SetParent(roomObjectData.transform);
    }
}