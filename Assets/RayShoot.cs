using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class RayShoot : MonoBehaviour
{
    [SerializeField] private float _vel;
    [SerializeField] private float _dmg;
    [SerializeField] private Rigidbody _rb;
    [SerializeField] private Entity.KindOfEntity Kind;
    [SerializeField] private LayerMask _obstacleMask;
    private float _rotationActive=1;
    private bool _fire = false;
    private bool _hasTarget = false;
    private float _lifeTimer = 0f;
    private Vector3 dir;
    private void Awake()
    {
        if (_rb == null) _rb = GetComponent<Rigidbody>();
    }

    /*public void Active()
    {
        _fire = true;
    }*/

    public void GetTg(Vector3 tg)
    {
        _fire = true;
        //transform.SetParent(GameManager.Instance.Gameplay);
        //var direct = (tg - _rb.position).normalized;
        dir = tg;
        _rotationActive = 0;
        //transform.forward = direct;
        _hasTarget = true;
    }

    private void Update()
    {
        if (GameManager.Instance.IsPaused)
        {
            return;
        }
        _rotationActive += Time.deltaTime;
        if(_rotationActive<0.3f)
        {
            transform.LookAt(dir);
        }
        if (_fire && _hasTarget)
        {
            _lifeTimer += Time.deltaTime;
            if (_lifeTimer >= 10f)
            {
                Destroy(gameObject);
            }
        }
    }

    private void FixedUpdate()
    {
        if(GameManager.Instance.IsPaused)
        {
            return;
        }
        if (_fire && _hasTarget)
        {
            _rb.MovePosition(_rb.position + transform.forward * _vel * Time.fixedDeltaTime);
        }
    }


    private void OnTriggerEnter(Collider other)
    {
        if (!_fire || !_hasTarget || other.isTrigger)
            return;

        Entity e = other.GetComponent<Entity>();

        if (e != null)
        {
            if (e.Kind == Kind)
            { return; }

            if (e.IsGrounded)
            {
                EventManager.Ejecute(EventManager.KindOfEvent.PauseOneEnemy, other.gameObject, 1.5f);
            }

            if (e.TryGetComponent<Idamageable>(out var damageable))
            { damageable.TakeDamage(_dmg, 15f, Vector3.zero); }
        }

        Destroy(gameObject);
    }
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawLine(transform.position, transform.position + transform.forward * 2f);
    }
}