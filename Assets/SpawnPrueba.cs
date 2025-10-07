using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnPrueba : Interact
{
    public EnemyCatalogue EnemyCatalogue;
    private void Start()
    {
        InteractManager.Instance.AddInteract(this);
    }
    public override void Interacting()
    {
        base.Interacting();
    }
    public override void Activate()
    {
        base.Activate();
        GenericFactory.Instance.GetObj(EnemyCatalogue, transform.position + Vector3.up * 2 + Vector3.right * 2);
    }
    public override void Desactivate()
    {
        base.Desactivate();
    }
}
