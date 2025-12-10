using System;
using UnityEngine;
using UnityEngine.Events;

[Serializable]
public class EnemyHealthData
{
    public float maxHealth;
}

public class EnemyHealth : MonoBehaviour, IDamagable
{
    public UnityEvent<GameObject> OnDeath = new ();
    public bool isDead = false;
    public float maxHealth;
    public float health;

    private void Start()
    {
        Revive();
    }

    public void Hit(Component damager, float damage)
    {
        if (isDead)
            return;
        health -= damage;
        
        if (!(health <= 0)) return;
        
        health = 0;
        isDead = true;
        OnDeath.Invoke(gameObject);
    }

    public void Heal(float amount)
    {
        if (isDead)
            return;
        health += amount;
        if (health > maxHealth)
            health = maxHealth;
    }
    
    public void Revive()
    {
        health = maxHealth;
        isDead = false;
    }
}