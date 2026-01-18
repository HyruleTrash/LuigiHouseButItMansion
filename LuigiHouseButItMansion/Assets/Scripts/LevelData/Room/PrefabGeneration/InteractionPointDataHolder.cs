
using LucasCustomClasses;
using UnityEngine;

public class InteractionPointDataHolder : PointDataHolder
{
    public PossibleInteractableList possibleInteractables;
    private Timer visualCycleTimer;
    private int usedVisualCycleIndex = 0;
    private float lastTime;
    private float gizmoDeltaTime;
    
    protected override void AddSelfToParent()
    {
        if (!parentGenerator.interactionPoints.Contains(this))
            parentGenerator.interactionPoints.Add(this);
    }

    public override Color GetColor() => Color.yellow;

    protected override void OnDrawGizmosSelected()
    {
        base.OnDrawGizmosSelected();
        
        if (parentGenerator == null || possibleInteractables == null || possibleInteractables.prefabs.Count == 0)
            return;
        Gizmos.color = GetColor();
        
        var now = Time.realtimeSinceStartup;
        gizmoDeltaTime = now - lastTime;
        lastTime = now;
        
        visualCycleTimer ??= new Timer(1, () =>
        {
            usedVisualCycleIndex++;
            if (usedVisualCycleIndex > possibleInteractables.prefabs.Count - 1)
                usedVisualCycleIndex = 0;
            visualCycleTimer.Reset();
        });
        visualCycleTimer.running = true;
        visualCycleTimer.Update(gizmoDeltaTime);
        
        if (usedVisualCycleIndex >= possibleInteractables.prefabs.Count || usedVisualCycleIndex < 0)
            return;
        GameObject foundObj = possibleInteractables.prefabs[usedVisualCycleIndex];
        Mesh mesh = foundObj.GetComponent<InteractableObject>().objectRepresentation.sharedMesh;
        if (mesh == null)
            return;
        Gizmos.DrawWireMesh(mesh, transform.position, interactableObjRotation * foundObj.transform.localRotation, foundObj.transform.localScale);
    }
}