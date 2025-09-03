using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DamageTramp : MonoBehaviour
{
    [SerializeField] private int _dmg;
    private List<Idamageable> _damageables=new List<Idamageable>();
    //private float _time=0;
    private void OnTriggerEnter(Collider other)
    {
        Idamageable p=other.GetComponent<Idamageable>();
        if (p != null)
        {
            p.TakeDamage(_dmg,0, Vector3.zero);
        }
            /* Idamageable p=other.GetComponent<Idamageable>();
             if(p != null )
             {
                 if (_damageables.Contains(p))
                 {
                     return;
                 }
                 _damageables.Add(p);
                 if(_damageables.Count <=1 )
                 {
                     //StartCoroutine(MakeDamage());
                 }
             }*/
        }
    private void OnTriggerExit(Collider other)
    {
        /*
        Idamageable p = other.GetComponent<Idamageable>();
        if (p != null)
        {
            if (!_damageables.Contains(p))
            {
                return;
            }
            _damageables.Remove(p);
            if (_damageables.Count <= 0)
            {
                //StopAllCoroutines();
            }
        }*/
    }
    /*private IEnumerator MakeDamage()
    {
        while (_time>=0.4f)
        {
            yield return new WaitUntil(()=>GameManager.Instance.IsPaused);
            _time += 0.1f;
            yield return new WaitForSeconds(0.1f);
        }
        foreach (var p in _damageables)
        {
            if (p == null) { _damageables.Remove(p); continue; }
            p.TakeDamage(_dmg);
        }
        if(_damageables.Count <= 0)
        { StopAllCoroutines(); }
        else
        {
            StartCoroutine(MakeDamage());
        }
    }*/
}
