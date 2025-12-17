
using UnityEngine;

public interface IDamagable
{
    public int HitFlashKey { get; set; }
    public void Hit(Component damager, float damage);
}