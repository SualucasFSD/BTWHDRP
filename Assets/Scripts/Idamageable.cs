using UnityEngine;

public interface Idamageable 
{
    public void TakeDamage(float dmg,float stunt, Vector3 pushDirection);
    public void TakeHealt(float amount);
}
