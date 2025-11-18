using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class SwordDamage : MonoBehaviour
{
    [Header("Opciones de daño")]
    [Tooltip("Si está activado, la espada puede hacer daño.")]
    public bool canDealDamage = false;

    private List<GameObject> hitEnemies = new List<GameObject>();

    private Collider swordCollider;

    private float _dmg;
    private float _swordArea;
    private float _stuntDmg;
    private float _flyAngle;
    private bool _getGround;
    private float _pushForce;
    private bool _airHit;

    private void Awake()
    {
        swordCollider = GetComponent<Collider>();

        swordCollider.isTrigger = true;

        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody>();
        }

        rb.isKinematic = true;
       rb.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;
    }
    private void Start()
    {
        EventManager.Suscribe(EventManager.KindOfEvent.RespecificSword, SwordSpecified);
        EventManager.Suscribe(EventManager.KindOfEvent.SwordDamage, SetCanDealDamage);
    }
    private void SwordSpecified(params object[] p)
    {
        _dmg = (float)p[0];
        _swordArea = (float)p[1];
        _flyAngle = (float)p[2];
        _getGround = (bool)p[3];
        _stuntDmg = (float)p[4];
        _pushForce = (float)p[5];
        _airHit = (bool)p[6];
    }
    public void SetCanDealDamage(params object[] p)
    {
        canDealDamage = (bool)p[0];
        if(!swordCollider.enabled)
        {
            swordCollider.enabled=true;
        }
        if ((bool)p[0])
        {
            hitEnemies.Clear();
            EventManager.Ejecute(EventManager.KindOfEvent.RefreshEnemyHitList,hitEnemies);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!canDealDamage) return;
        GenericDestroyable destro = other.GetComponent<GenericDestroyable>();
        if (destro != null)
        {
            destro.GetComponent<Idamageable>().TakeDamage(500, 0, Vector3.zero);
            return;
        }

        if (IsEnemy(other.gameObject))
        {
            if (!GameManager.Instance.LineOfSight(transform.position, other.transform.position)){return;}

            if (!hitEnemies.Contains(other.gameObject))
            {
                hitEnemies.Add(other.gameObject);
                EventManager.Ejecute(EventManager.KindOfEvent.RefreshEnemyHitList, hitEnemies);
                Vector3 pushDirection = new Vector3((other.transform.position - transform.position).x, 0, (other.transform.position - transform.position).z).normalized;
                Idamageable l = other.GetComponent<Idamageable>();
                l.TakeDamage(_dmg * 1, _stuntDmg * 1 / 2, pushDirection, _getGround,_airHit,true,_pushForce);
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (hitEnemies.Contains(other.gameObject))
        {
            hitEnemies.Remove(other.gameObject);
            EventManager.Ejecute(EventManager.KindOfEvent.RefreshEnemyHitList, hitEnemies);
        }
    }

    private bool IsEnemy(GameObject obj)
    {
        Entity j = obj.GetComponent<Entity>();
        if (j != null)
        {
            return true;
        }
        else
        {
            return false;
        }
    }
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(transform.position, 0.1f);
    }
    private void OnDestroy()
    {
        EventManager.Unscribe(EventManager.KindOfEvent.SwordDamage, SetCanDealDamage);
        EventManager.Unscribe(EventManager.KindOfEvent.RespecificSword, SwordSpecified);
    }
}