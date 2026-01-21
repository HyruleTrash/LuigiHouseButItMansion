using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "PossibleEnemyPool", menuName = "ScriptableObjects/RoomData/PossibleEnemyPoolData")]
public class PossibleEnemyPoolData : ScriptableObject
{
    public List<ClassReference<BaseEnemy>> enemyReferences = new();
    public Vector2Int minMaxSpawnCount;
}