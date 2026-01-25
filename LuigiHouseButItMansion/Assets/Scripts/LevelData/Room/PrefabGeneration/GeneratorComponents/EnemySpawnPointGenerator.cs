using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public class EnemySpawnPointGenerator : BaseRoomGeneratorComponent
{
    public List<PointDataHolder> enemySpawnPoints = new();
    public List<PossibleEnemyPoolData> possibleEnemyPoolDatas = new();

    protected override List<PointDataHolder> GetList() => enemySpawnPoints;
    protected override Type GetGenType() => typeof(EnemySpawnPointDataHolder);

    public override void UpdateList()
    {
        enemySpawnPoints.Clear();
        foreach (var data in GetComponentsInChildren<EnemySpawnPointDataHolder>())
        {
            enemySpawnPoints.Add(data);
        }
    }

    public override bool CanGenerate() => enemySpawnPoints.Count != 0 && possibleEnemyPoolDatas != null;
    public override void Generate(RoomObjectData roomObjectData)
    {
        var enemySpawnManager = new GameObject("EnemySpawnManager").AddComponent<EnemySpawnManager>();
        enemySpawnManager.transform.SetParent(roomObjectData.transform);

        var pickedData = possibleEnemyPoolDatas[Random.Range(0, possibleEnemyPoolDatas.Count - 1)];
        var enemyPool = pickedData.enemyReferences;

        var possibleInteractionPoints = enemySpawnPoints.Cast<EnemySpawnPointDataHolder>().ToList();
        var result = new List<EnemySpawnPointDataHolder>();
        var amount = Random.Range(minMaxChosenFromList.x, minMaxChosenFromList.y);
        for (var i = 0; i < amount; i++)
        {
            var index = Random.Range(0, possibleInteractionPoints.Count);
            var point = possibleInteractionPoints[index];
            result.Add(point);
            possibleInteractionPoints.RemoveAt(index);
        }
        
        enemySpawnManager.Init(roomObjectData, Random.Range(pickedData.minMaxSpawnCount.x, pickedData.minMaxSpawnCount.y), enemyPool, result);
    }
}