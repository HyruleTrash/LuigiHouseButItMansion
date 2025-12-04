using UnityEditor;
using UnityEngine;

[CreateAssetMenu(fileName = "BasicEnemyData", menuName = "ScriptableObjects/Enemies/BasicEnemy")]
public class BasicEnemyData : ScriptableObjectSingleton<BasicEnemyData>
{
    public GameObject enemyPrefab;
}