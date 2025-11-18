using UnityEngine;

public interface Idamageable 
{
    public void TakeDamage(float dmg,float stunt, Vector3 pushDirection, bool downHit=false,bool airHit=false,bool isStuntDamage=false,float pushForce=1000);
    public void TakeHealt(float amount);
}
