
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawnManager : MonoBehaviour
{
    private RoomObjectData parentRoom;
    private Transform[] spawnPoints;
    [SerializeField]
    private int spawnCount = 1;
    [SerializeField]
    private ClassReference<IEnemy> enemyReference;
    private List<GameObject> enemies = new();

    private void Awake()
    {
        parentRoom = transform.parent.GetComponent<RoomObjectData>();
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

    private void Spawn()
    {
        if (!enabled || spawnCount <= 0)
            return;
        spawnCount--;

        foreach (var spawnPoint in spawnPoints)
        {
            enemyReference.CallMethod("Spawn", new object[] {this, spawnPoint.position});
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
