using System;
using System.Collections.Generic;
using System.Linq;
using LucasCustomClasses;
using UnityEngine;
using UnityEngine.Events;

public class Health : MonoBehaviour, IDamagable
{
    public UnityEvent<GameObject> OnDeath = new ();
    public UnityEvent<GameObject> OnHit = new ();
    public bool isDead = false;
    public float maxHealth = 20;
    public float health;
    public float invincibilityFrames = 0.1f;
    private List<DamagerRegistration> damagers = new();
    public int HitFlashKey { get => hitFlashKey; set => hitFlashKey = value; }
    private int hitFlashKey;
    
    private class DamagerRegistration
    {
        public Timer timer;
        public object damager;
    }

    private void Start()
    {
        Revive();
    }

    private void Update()
    {
        if (isDead || damagers == null || damagers.Count < 1)
            return;
        var temp = damagers.ToList();
        foreach (DamagerRegistration damager in temp)
            damager.timer.Update(Time.deltaTime);
    }

    public void Hit(object damager, float damage)
    {
        if (isDead || HasHitEnemy(damager))
            return;
        health -= damage;
        OnHit.Invoke(gameObject);
        
        if (!(health <= 0))
        {
            damagers.Add(new DamagerRegistration {
                damager = damager,
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

    private void RemoveFromDamagers(object damager)
    {
        var temp = damagers.FirstOrDefault(x => x.damager == damager);
        if (temp != null)
            damagers.Remove(temp);
    }

    private bool HasHitEnemy(object damager)
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

    public override string ToString()
    {
        return $"[{health} / {maxHealth}]";
    }
}