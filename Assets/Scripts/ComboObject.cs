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
}
