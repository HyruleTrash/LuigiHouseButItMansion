
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public class EnemySpawnManager : MonoBehaviour
{
    public RoomObjectData parentRoom;
    private Transform[] spawnPoints = Array.Empty<Transform>();
    [SerializeField]
    private int spawnCount = 1;
    [SerializeField]
    private List<ClassReference<BaseEnemy>> enemyReferences = new();
    private List<ClassReference<BaseEnemy>> enemyReferencesThatUseSpawnPoints = new();
    private List<ClassReference<BaseEnemy>> enemyReferencesWithoutSpawnPoints = new();
    private List<GameObject> enemies = new();

    private void Awake()
    {
        if (parentRoom == null)
        {
            enabled = false;
            return;
        }

        parentRoom.OnReadyRoom += Spawn;

        spawnPoints = new Transform[transform.childCount];
        for (int i = 0; i < transform.childCount; i++)
        {
            spawnPoints[i] = transform.GetChild(i);
        }
    }

    private void Start()
    {
        if (enemyReferences.Count <= 1)
            return;
        foreach (var reference in enemyReferences)
        {
            var data = EnemyDataBase.Instance.GetData(reference.ResolvedType);
            if (data != null && data.usesSpawnPoint)
                enemyReferencesThatUseSpawnPoints.Add(reference);
            else
                enemyReferencesWithoutSpawnPoints.Add(reference);
        }
    }

    private void Spawn()
    {
        if (!enabled || spawnCount <= 0)
            return;
        spawnCount--;

        if (enemyReferences.Count > 1)
        {
            var spawnPointsToUse = spawnPoints.ToList();
            var enemyReferencesCopy = enemyReferencesThatUseSpawnPoints.ToList();
            while (spawnPointsToUse.Count != 0)
            {
                ClassReference<BaseEnemy> enemyReference;
                if (enemyReferencesCopy.Count == 0)
                    enemyReference = enemyReferencesThatUseSpawnPoints[Random.Range(0, enemyReferencesThatUseSpawnPoints.Count - 1)];
                else
                {
                    enemyReference = enemyReferencesCopy[Random.Range(0, enemyReferencesThatUseSpawnPoints.Count - 1)];
                    enemyReferencesCopy.Remove(enemyReference);
                }
                var spawnPoint = spawnPointsToUse.FirstOrDefault();
                if (spawnPoint == null)
                    continue;
                enemyReference.CallMethod("Spawn", new object[] { this, spawnPoint.position });
                spawnPointsToUse.Remove(spawnPoint);
            }

            foreach (var enemyReference in enemyReferencesWithoutSpawnPoints)
            {
                enemyReference.CallMethod("Spawn", new object[] {this, Vector3.zero});
            }
        }
        else if (enemyReferences.Count == 1)
        {
            foreach (var spawnPoint in spawnPoints)
            {
                enemyReferences[0].CallMethod("Spawn", new object[] {this, spawnPoint.position});
            }
        }

        parentRoom.LockDoors();
    }

    public void Add(GameObject instance)
    {
        enemies.Add(instance);
    }

    public void CheckLiveEnemyState()
    {
        if (enemies.Count <= 0)
            parentRoom.UnLockDoors();
    }

    public void Remove(GameObject instance)
    {
        enemies.Remove(instance);
    }
}
