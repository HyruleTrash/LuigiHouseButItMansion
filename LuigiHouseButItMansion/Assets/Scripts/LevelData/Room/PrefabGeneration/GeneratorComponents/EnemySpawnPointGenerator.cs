using System.Collections.Generic;
using UnityEngine;

public class EnemySpawnPointGenerator : BaseRoomGeneratorComponent
{
    public List<PointDataHolder> enemySpawnPoints = new();
    public List<PossibleEnemyPoolData> possibleEnemyPoolDatas = new();

    public override List<PointDataHolder> GetList() => enemySpawnPoints;

    public override void UpdateList()
    {
        enemySpawnPoints.Clear();
        foreach (var data in GetComponentsInChildren<EnemySpawnPointDataHolder>())
        {
            enemySpawnPoints.Add(data);
        }
    }

    public override void Generate(RoomObjectData roomObjectData)
    {
        var enemySpawnManager = new GameObject("EnemySpawnManager").AddComponent<EnemySpawnManager>();
        enemySpawnManager.transform.SetParent(roomObjectData.transform);

        // Define EnemyPool TODO
        
        // enemySpawnManager.Init(roomObjectData, spawnCount, enemyPool); TODO
    }
}