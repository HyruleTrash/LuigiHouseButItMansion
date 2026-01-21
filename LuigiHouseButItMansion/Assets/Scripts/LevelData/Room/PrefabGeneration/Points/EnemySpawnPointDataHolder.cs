using UnityEngine;

public class EnemySpawnPointDataHolder : PointDataHolder
{
    protected override BaseRoomGeneratorComponent GetParentComponent() => transform.parent.GetComponent<EnemySpawnPointGenerator>();
    protected override void AddSelfToParent()
    {
        EnemySpawnPointGenerator parent = (EnemySpawnPointGenerator)parentGenerator;
        if (!parent.enemySpawnPoints.Contains(this))
            parent.enemySpawnPoints.Add(this);
    }

    public override Color GetColor() => Color.red;
}