using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[RequireComponent(typeof(Animator))]
public class HandPowerMakeDamage : MonoBehaviour
{
    [SerializeField] private Animator _animator;
    private float _iti = 0;
    private void Awake()
    {
        _animator = GetComponent<Animator>();
    }
    private void Update()
    {
        _iti += Time.deltaTime;
        if (_iti > 3) { _animator.SetBool("Attack", true);_iti = 0; _animator.SetBool("Attack", false); }
    }
    /*public void Damage()
    {
        Collider[] c = Physics.OverlapSphere(transform.position, 5f, _hitLayer);

        foreach (Collider col in c)
        {
            Entity p = col.GetComponent<Entity>();
            if (p != null)
            {
                if (p.Kind == _model.Kind)
                {
                    continue;
                }
            }
            else { continue; }
            if (Vector3.Dot(transform.forward, (col.transform.position - transform.position).normalized) > 0.55f)
            {
                col.GetComponent<Idamageable>().TakeDamage(15);
            }
            else
            {
                continue;
            }
        }
    }*/
}
