using UnityEngine;

public interface Idamageable 
{
    public void TakeDamage(float dmg,float stunt, Vector3 pushDirection, bool downHit=false,bool isStuntDamage=false);
    public void TakeHealt(float amount);
}
