using System.Collections.Generic;
using UnityEngine;
public enum KindOfCombo
{
   Light,
   Strong,
   LightLong,
   StrongLong,
   LightAir,
   StrongAir,
   LightLongAir,
   StrongLongAir,
   SprintLight,
   SprintLong
}

[CreateAssetMenu]
public class ComboObject : ScriptableObject
{
    public string Name;
    public List<KindOfCombo> ComboImput;
    public string TriggerAnimName;
    public float Dmg;
    public float SwordDistance;
    public float SwordFlyArea;
    public float StuntDmg;
    public float PushForce;
    public bool GetAir = false;
    public bool GetGround = false;
    public bool IsSwordDepend=false;
    [Range(0.1f, 1f)] public float Angle;
    [Range(0.1f, 1f)] public float FlyAngle;
}
