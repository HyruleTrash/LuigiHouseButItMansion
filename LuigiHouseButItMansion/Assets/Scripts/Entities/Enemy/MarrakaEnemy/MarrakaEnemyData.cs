
using System;
using UnityEngine;

[CreateAssetMenu(fileName = "MarrakaEnemyData", menuName = "ScriptableObjects/Enemies/MarrakaEnemy")]
public class MarrakaEnemyData : BaseEnemyData
{
    public GameObject enemyPrefab;
    public float minPlayerHitDistance = 1;
    [Tooltip("Used for if the player walks out of attack range")]
    public float minPlayerNearDistance = 2;
    public int damageAmount = 1;
    public float hideAndSeekTime;
    public float spawnAnimSpeed = 1;
    public ChaseState strongState;
    public ChaseState weakState;

    [Serializable]
    public struct ChaseState
    {
        public float speed;
        public EnemyHealthData healthData;
        public float stunlockTime;
        public float stunInvincibilityTime;
    }

}