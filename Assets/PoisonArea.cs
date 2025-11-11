using System.Collections.Generic;
using UnityEngine;
[RequireComponent(typeof(SphereCollider))]
public class PoisonArea : MonoBehaviour
{
    [SerializeField] private bool _bothDamageables=false;
    [SerializeField] private LayerMask _groundLayer;
    [SerializeField] private Entity.KindOfEntity _entityType;
    [SerializeField] private float _areaDuration;
    private float _iti=0;
    private List<GameObject> _poisoned = new List<GameObject>();
    private void Awake()
    {
        print("poison");
        GetComponent<SphereCollider>().isTrigger = true;
        transform.position += Vector3.up;
        Physics.Raycast(transform.position, -Vector3.up, out RaycastHit hit, 10, _groundLayer);
        if(!hit.collider)
        {
           Destroy(gameObject);
        }
        else
        {
            transform.position= hit.point;
        }
            
    }
    private void Update()
    {
        _iti += Time.deltaTime;
        if(_iti>_areaDuration) { Destroy(gameObject); }
    }
    private void OnTriggerEnter(Collider other)
    {
       if(_poisoned.Contains(other.gameObject))
       {
         return;
       }
       else
       {
         Entity ent= other.gameObject.GetComponent<Entity>();
         if(ent!=null)
         {
            if(ent.Kind==_entityType&&!_bothDamageables)
            {
              return ;
            }
            else
            {
               ent.GetVenemous(3, 1, 15);
            }
         }
       }
    }
    private void OnTriggerExit(Collider other)
    {
        _poisoned.Remove(other.gameObject);
    }
}
