
using UnityEngine;

public interface IDamagable
{
    public int HitFlashKey { get; set; }
    public void Hit(object damager, float damage);
}