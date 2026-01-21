
using System;
using LucasCustomClasses;
using UnityEditor;
using UnityEngine;
using Random = UnityEngine.Random;

public class InteractionPointDataHolder : PointDataHolder
{
    public PossibleInteractableList possibleInteractables;
    private Timer visualCycleTimer;
    private int usedVisualCycleIndex = 0;
    private float lastTime;
    private float gizmoDeltaTime;
    [HideInInspector]
    public bool justSelected = false;

    protected override BaseRoomGeneratorComponent GetParentComponent()=> transform.parent.GetComponent<InteractionPointsGenerator>();

    protected override void AddSelfToParent()
    {
        InteractionPointsGenerator parent = (InteractionPointsGenerator)parentGenerator;
        if (!parent.interactionPoints.Contains(this))
            parent.interactionPoints.Add(this);
    }

    private void OnEnable()
    {
        Selection.selectionChanged += OnSelectionChanged;
    }

    private void OnDisable()
    {
        Selection.selectionChanged -= OnSelectionChanged;
    }

    private void OnDestroy()
    {
        Selection.selectionChanged -= OnSelectionChanged;
    }

    private void OnSelectionChanged()
    {
        if (gameObject == null) return;
        if (Selection.activeGameObject == gameObject) justSelected = true;
    }

    public override Color GetColor() => Color.yellow;

    protected override void OnDrawGizmosSelected()
    {
        base.OnDrawGizmosSelected();
        
        if (parentGenerator == null || possibleInteractables == null || possibleInteractables.prefabs.Count == 0)
            return;
        Gizmos.color = GetColor();

        if (Selection.activeGameObject == gameObject && justSelected)
        {
            justSelected = false;
            usedVisualCycleIndex = Random.Range(0, possibleInteractables.prefabs.Count - 1);
        }
        
        var now = Time.realtimeSinceStartup;
        gizmoDeltaTime = now - lastTime;
        lastTime = now;
        
        visualCycleTimer ??= new Timer(3, () =>
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