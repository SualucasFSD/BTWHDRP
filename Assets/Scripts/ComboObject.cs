using System.Collections.Generic;
using UnityEngine;
public enum KindOfCombo
{
   Light,
   Strong,
   LightLong,
   StrongLong
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
    [Range(0.1f, 1f)] public float Angle;
    [Range(0.1f, 1f)] public float FlyAngle;
}
