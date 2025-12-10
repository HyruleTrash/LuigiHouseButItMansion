using System;
using System.Collections.Generic;
using System.Linq;
using LucasCustomClasses;
using UnityEngine;
using UnityEngine.Events;

[Serializable]
public class EnemyHealthData
{
    public float maxHealth;
    public float invincibilityFrames;
}

public class EnemyHealth : MonoBehaviour, IDamagable
{
    public UnityEvent<GameObject> OnDeath = new ();
    public bool isDead = false;
    public float maxHealth;
    public float health;
    public float invincibilityFrames = 0.1f;
    private List<DamagerRegistration> damagers = new();
    
    private class DamagerRegistration
    {
        public Timer timer;
        public Component damager;
    }

    private void Start()
    {
        Revive();
    }

    private void Update()
    {
        if (isDead || damagers == null || damagers.Count < 1)
            return;
        foreach (DamagerRegistration damager in damagers)
            damager.timer.Update(Time.deltaTime);
    }

    public void Hit(Component damager, float damage)
    {
        if (isDead || HasHitEnemy(damager))
            return;
        health -= damage;

        if (!(health <= 0))
        {
            damagers.Add(new DamagerRegistration {
                timer = new Timer(invincibilityFrames) { onEnd = () =>
                {
                    RemoveFromDamagers(damager);
                }}
            });
            return;
        }
        
        health = 0;
        isDead = true;
        OnDeath.Invoke(gameObject);
        damagers = new List<DamagerRegistration>();
    }

    private void RemoveFromDamagers(Component damager)
    {
        foreach (var registeredDamager in damagers)
        {
            if (!damager.Equals(registeredDamager.damager)) continue;
            damagers.Remove(registeredDamager);
            return;
        }
    }

    private bool HasHitEnemy(Component damager)
    {
        return damagers.Any(registeredDamager => damager.Equals(registeredDamager.damager));
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