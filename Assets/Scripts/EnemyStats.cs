using UnityEngine;
[CreateAssetMenu]
public class EnemyStats : ScriptableObject
{
    public EnemyCatalogue Kind;
    public float Velocity;
    public float Life;
    public float RotForce;
    public float Damage;
    public float GravityForce;
    public float Height;
    public float Radius;
    public int DefenseProb;
    public float DefenseDistance;
    public float VisionCone;
    public float VisionDistance;
    public float AttackDistance;
    public float StuntResistance;
    public float StuntTime;
}
