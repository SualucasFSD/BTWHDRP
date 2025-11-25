using System;
using System.Collections.Generic;
[Serializable]

public class DataSave 
{
    public bool Save = false;
    public float Gold = 0;
    public float Diamond=0;
    public HashSet<EnemyCatalogue> PowerObtained=new HashSet<EnemyCatalogue>();
}
