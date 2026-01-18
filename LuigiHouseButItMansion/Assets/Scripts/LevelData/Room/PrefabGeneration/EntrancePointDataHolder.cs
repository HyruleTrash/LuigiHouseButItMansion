
using UnityEngine;
using UnityEditor;

public class EntrancePointDataHolder : PointDataHolder
{
    protected override void AddSelfToParent()
    {
        if (!parentGenerator.entrancePoints.Contains(this))
            parentGenerator.entrancePoints.Add(this);
    }

    public override Color GetColor() => Color.orangeRed;

    protected override void OnDrawGizmosSelected()
    {
        base.OnDrawGizmosSelected();
        
        if (!parentGenerator)
            return;
        Gizmos.color = GetColor();
        Mesh mesh = parentGenerator.entrancePrefab.GetComponent<RoomEntrance>().doorRenderObject.sharedMesh;
        Gizmos.DrawWireMesh(mesh, transform.position, interactableObjRotation * Quaternion.Euler(-90, 0, 0));
    }
}