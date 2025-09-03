using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Sword : MonoBehaviour
{
    [SerializeField] private PjModel _model;
    private Dictionary<Collider, bool> Damageable = new Dictionary<Collider, bool>();
    public int Dmg;

    private Vector3 _lastPosition;
    private Vector3 _velocity;

    private void Update()
    {
        _velocity = (transform.position - _lastPosition) / Time.deltaTime;
        _lastPosition = transform.position;
    }

    private void OnTriggerStay(Collider other)
    {
        if (Dmg == 0)
            return;

        if (!other.TryGetComponent<Idamageable>(out var damageable))
            return;

        Entity entity = other.GetComponent<Entity>();

        if (Damageable.TryGetValue(other, out bool canDamage))
        {
            if (!canDamage)
                return;

            if (entity == null || entity.Kind != _model.Kind)
            {
                Vector3 knockbackDir = _velocity.normalized;
                damageable.TakeDamage(Dmg, 0.5f * Dmg, knockbackDir);
                Damageable[other] = false;
            }
            return;
        }

        if (entity == null || entity.Kind != _model.Kind)
        {
            Vector3 knockbackDir = _velocity.normalized;
            Damageable.Add(other, true);
            damageable.TakeDamage(Dmg, 0.5f * Dmg, knockbackDir);
            Damageable[other] = false;
        }
    }

    public void ResetDicctionary()
    {
        foreach (var key in Damageable.Keys.ToList())
        {
            if (key != null)
                Damageable[key] = true;
        }
    }
    /* [SerializeField]private PjModel _model;
     private Dictionary<Collider, bool> Damageable = new Dictionary<Collider, bool>(); 
     public int Dmg;
     private void OnTriggerStay(Collider other)
     {
         if (Dmg == 0)
             return;

         if (!other.TryGetComponent<Idamageable>(out var damageable))
             return;

         Entity entity = other.GetComponent<Entity>();

         if (Damageable.TryGetValue(other, out bool canDamage))
         {
             if (!canDamage)
                 return;

             if (entity == null || entity.Kind != _model.Kind)
             {
                 damageable.TakeDamage(Dmg, 0.5f*Dmg, direcction);
                 Damageable[other] = false;
             }

             return;
         }
         if (entity == null || entity.Kind != _model.Kind)
         {
             Damageable.Add(other, true);
             damageable.TakeDamage(Dmg, 0.5f * Dmg, direcction);
             Damageable[other] = false;
         }
     }
     public void ResetDicctionary()
     {
         foreach (var key in Damageable.Keys.ToList())
         {
             if (key != null)
                 Damageable[key] = true;
         }
     }*/
}
