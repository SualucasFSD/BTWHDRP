using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HandsDamage : MonoBehaviour
{
    [SerializeField] private GameObject pivot;
    private Animator animator;
    [SerializeField] private FirstBoss _boss;
    private void Awake()
    {
        animator = GetComponent<Animator>();
    }
    public void MakeDamage()
    {
        Collider[] c = Physics.OverlapSphere(pivot.transform.position, 6);
       foreach (Collider col in c)
        {
            if (col.gameObject==_boss.gameObject)
            {
                continue;
            }
            if (col.GetComponent<Idamageable>() != null)
            {
                if (Vector3.Dot(transform.forward, (col.transform.position - transform.position).normalized) > 0.65f && Vector3.Distance(transform.position, col.transform.position) <= 10)
                {
                    col.GetComponent<Idamageable>().TakeDamage(70, 0, Vector3.zero);
                }
            }
        }
    }
    public void StopAnim()
    {
        if (_boss != null)
        {
            _boss._handsInUse = false;
        }
        animator.SetBool("Slap", false);
    }
}
