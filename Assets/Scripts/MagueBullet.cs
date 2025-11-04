//using UnityEngine;

//[RequireComponent(typeof(Rigidbody), typeof(Collider))]
//public class MagueBullet : MonoBehaviour
//{
//    [Header("References")]
//    private Transform _tg;
//    [SerializeField] private Rigidbody _rb;

//    [Header("Stats")]
//    [SerializeField] private float _speed = 15f;
//    [SerializeField, Range(0.001f, 0.125f)] private float _rotForce = 0.05f;
//    //[SerializeField] private float _lifeTime = 10f;

//    [Header("State")]
//    public bool Fire = false;
//    private bool _follow = true;
//    private float _dmg;
//    public Entity.KindOfEntity Kind;

//    private Vector3 _lastDir;

//    private void Awake()
//    {
//        if (_rb == null)
//            _rb = GetComponent<Rigidbody>();

//        _rb.isKinematic = true;
//    }

//    private void OnDisable()
//    {
//        _tg = null;
//        Fire = false;
//        _follow = true;
//        CancelInvoke();
//    }

//    public void SetTarget(Transform target)
//    {
//        _tg = target;
//        if (_tg != null && Vector3.Distance(_tg.position, transform.position) < 4f)
//        {
//            transform.rotation = Quaternion.LookRotation((_tg.position - transform.position).normalized);
//            _follow = false;
//        }
//        _lastDir = transform.forward;
//    }

//    private void FixedUpdate()
//    {
//        if (GameManager.Instance.IsPaused) { return; }
//        if (!Fire) { return; }

//        transform.parent = GameManager.Instance.Gameplay;
//        _rb.isKinematic = false;

//        if (_tg != null && _follow)
//        {
//            float dist = Vector3.Distance(_tg.position, transform.position);
//            if (dist < 4f)
//            {
//                _follow = false;
//                _lastDir = transform.forward;
//            }
//            else
//            {
//                Quaternion targetRot = Quaternion.LookRotation((_tg.position - transform.position).normalized);
//                transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, _rotForce);
//                _lastDir = transform.forward;
//            }
//        }
//        else if (_tg == null && _follow)
//        {
//            _follow = false;
//            _lastDir = transform.forward;
//        }

//        _rb.MovePosition(transform.position + _lastDir * _speed * Time.fixedDeltaTime);
//    }

//    private void OnTriggerEnter(Collider other)
//    {
//        if (!Fire) return;
//        if (other.isTrigger) return;

//        Entity e = other.GetComponent<Entity>();
//        if (e != null)
//        {
//            if (e.Kind == Kind) return;

//            Idamageable damageable = e.GetComponent<Idamageable>();
//            if (damageable != null)
//            {
//                _dmg = GameManager.Instance.EnemyConfiguration[EnemyCatalogue.Mague].Damage;
//                damageable.TakeDamage(_dmg, 15f, Vector3.zero);
//            }

//            DestroySelf();
//            return;
//        }
//        DestroySelf();
//    }

//    private void DestroySelf()
//    {
//        CancelInvoke();
//        GameObjectFactory.Instance.ReturnObj(GenericObjectType.MagueBullet, gameObject);
//        //Destroy(gameObject);
//    }
//}
using UnityEngine;

[RequireComponent(typeof(Rigidbody), typeof(Collider))]
public class MagueBullet : MonoBehaviour
{
    [Header("References")]
    private Transform _tg;
    [SerializeField] private Rigidbody _rb;

    [Header("Stats")]
    [SerializeField] private float _speed = 15f;
    [SerializeField, Range(0.001f, 0.125f)] private float _rotForce = 0.05f;

    [Header("State")]
    public bool Fire = false;
    private bool _follow = true;
    private float _dmg;
    public Entity.KindOfEntity Kind;

    private Vector3 _lastDir;

    private void Awake()
    {
        if (_rb == null)
            _rb = GetComponent<Rigidbody>();

        _rb.isKinematic = true;
        _rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
    }

    private void OnDisable()
    {
        _rb.isKinematic = true;
        _rb.linearVelocity = Vector3.zero;
        _rb.angularVelocity = Vector3.zero;
        _tg = null;
        Fire = false;
        _follow = true;
        _lastDir = Vector3.zero;
        CancelInvoke();
    }

    public void SetTarget(Transform target)
    {
        _tg = target;

        if (_tg != null && Vector3.Distance(_tg.position, transform.position) < 4f)
        {
            transform.rotation = Quaternion.LookRotation((_tg.position - transform.position).normalized);
            _follow = false;
        }

        _lastDir = transform.forward;
    }

    private void FixedUpdate()
    {
        if (GameManager.Instance.IsPaused || !Fire)
            return;

        transform.parent = GameManager.Instance.Gameplay;
        _rb.isKinematic = false;

        if (_tg != null && _follow)
        {
            float dist = Vector3.Distance(_tg.position, transform.position);

            if (dist < 4f)
            {
                _follow = false;
                _lastDir = transform.forward;
            }
            else
            {
                Quaternion targetRot = Quaternion.LookRotation((_tg.position - transform.position).normalized);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, _rotForce);
                _lastDir = transform.forward;
            }
        }
        else if (_tg == null && _follow)
        {
            _follow = false;
            _lastDir = transform.forward;
        }

        _rb.MovePosition(transform.position + _lastDir * _speed * Time.fixedDeltaTime);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!Fire || other.isTrigger)
            return;

        Entity e = other.GetComponent<Entity>();

        if (e != null)
        {
            if (e.Kind == Kind)
                return;

            if (e.TryGetComponent<Idamageable>(out var damageable))
            {
                _dmg = GameManager.Instance.EnemyConfiguration[EnemyCatalogue.Mague].Damage;
                damageable.TakeDamage(_dmg, 15f, Vector3.zero);
            }
        }

        ReturnToPool();
    }

    private void ReturnToPool()
    {
        _rb.linearVelocity = Vector3.zero;
        _rb.angularVelocity = Vector3.zero;
        _rb.isKinematic = true;
        Fire = false;
        _follow = true;
        _tg = null;
        CancelInvoke();
        GameObjectFactory.Instance.ReturnObj(GenericObjectType.MagueBullet, gameObject);
    }
}
