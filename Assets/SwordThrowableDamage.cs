//using UnityEngine;
//[RequireComponent(typeof(Rigidbody))]
//public class SwordThrowableDamage : MonoBehaviour
//{
//    [SerializeField] private float _duration;
//    [SerializeField] private float _speed;
//    [SerializeField] private float _swordDistance;
//    [SerializeField] private float _pushForce;
//    [SerializeField] private Rigidbody _rigidbody;
//    [SerializeField] private float _dmg=70;
//    public GameObject Tg;
//    private float _iti = 0;
//    private float _moveIti = 0;
//    private bool _canMove=true;
//    //private LayerMask _moveLayer;
//    [SerializeField]private LayerMask _hitLayer;
//    private void Awake()
//    {
//        if(_rigidbody!=null)
//        {
//            return;
//        }
//        _rigidbody=GetComponent<Rigidbody>();
//    }
//    void Update()
//    {
//        _iti+= Time.deltaTime;
//        _duration -= Time.deltaTime;
//        _moveIti+= Time.deltaTime;
//        if(_moveIti>0.2f)
//        {
//            _canMove=false;
//        }
//        if (_iti>0.5f)
//        {
//            _iti = 0;
//            CauseDamage();
//        }
//        if (_duration <= 0)
//        {
//            Destroy(gameObject);
//        }
//    }
//    private void CauseDamage()
//    {
//        Collider[] colliders = Physics.OverlapSphere(transform.position, _swordDistance, _hitLayer);

//        foreach (Collider collider in colliders)
//        {
//            if (collider.gameObject == gameObject)
//            {
//                continue;
//            }
//            Entity entity = collider.GetComponent<Entity>();

//            if (entity == null)
//            {
//                GenericDestroyable destro = collider.GetComponent<GenericDestroyable>();
//                if (destro != null)
//                {
//                    destro.GetComponent<Idamageable>().TakeDamage(500, 0, Vector3.zero);
//                }
//                continue;
//            }
//            float verticalDiff = Mathf.Abs(entity.transform.position.y - transform.position.y);
//            if (verticalDiff > 2f)
//            {
//                continue;
//            }
//            if (!GameManager.Instance.LineOfSight(transform.position, entity.transform.position))
//            {
//                continue;
//            }
//            Vector3 origin = transform.position + Vector3.up * 1f - transform.forward * 0.5f;
//            Vector3 dirToEnemy = (entity.transform.position - origin).normalized;

//            if (Physics.Raycast(origin, dirToEnemy, out RaycastHit hit, _swordDistance, _hitLayer))
//            {
//                if (hit.collider.transform.root != entity.transform.root)
//                {
//                    continue;
//                }
//                Idamageable damageable = entity.GetComponent<Idamageable>();
//                if (damageable != null)
//                {
//                    Vector3 pushDir = new Vector3((entity.transform.position - transform.position).x, 0f, (entity.transform.position - transform.position).z).normalized;

//                    damageable.TakeDamage(_dmg /** _dmgMultiply*/, 0/*_stuntDmg * _dmgMultiply / 2f*/, pushDir, false, false, true, _pushForce);
//                }
//            }
//        }
//    }
//    private void FixedUpdate()
//    {
//        if (_canMove)
//        {
//            _rigidbody.MovePosition(transform.position + transform.forward * _speed * Time.fixedDeltaTime);
//        }
//    }
//    private void OnTriggerEnter(Collider other)
//    {
//        if(other.gameObject.layer==7)
//        {
//            _canMove = false;
//        }
//    }
//}
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class SwordThrowableDamage : MonoBehaviour
{
    [SerializeField] private float _duration;
    [SerializeField] private float _speed;
    [SerializeField] private float _returnSpeed = 20f;
    [SerializeField] private float _swordDistance;
    [SerializeField] private float _pushForce;
    [SerializeField] private Rigidbody _rigidbody;
    [SerializeField] private float _dmg = 70;
    [SerializeField] private LayerMask _hitLayer;
    [SerializeField] private float _tickRate;
    private GameObject _tg;
    private KnightView _view;

    private float _iti;
    private float _moveIti;
    private bool _canMove = true;
    private bool _returning = false;

    void Awake()
    {
        if (_rigidbody != null) return;
        _rigidbody = GetComponent<Rigidbody>();
    }

    public void Init(GameObject tg, KnightView view)
    {
        _tg = tg;
        _view = view;
    }

    public void Cancel()
    {
        _returning = true;
    }

    void Update()
    {
        _iti += Time.deltaTime;
        _moveIti += Time.deltaTime;
        _duration -= Time.deltaTime;

        if (_moveIti > 0.2f) _canMove = false;

        if (_iti > _tickRate)
        {
            _iti = 0;
            CauseDamage();
        }

        if (_duration <= 0)
        {
            _returning = true;
        }

        if (_returning && _tg != null)
        {
            Vector3 dir = (_tg.transform.position - transform.position).normalized;
            transform.position += dir * _returnSpeed * Time.deltaTime;

            if (Vector3.Distance(transform.position, _tg.transform.position) < 0.5f)
            {
                if(_view!=null)
                {
                    _view.ActiveSword();
                }
                Destroy(gameObject);
            }
        }
    }

    private void FixedUpdate()
    {
        if (_canMove && !_returning)
        {
            _rigidbody.MovePosition(transform.position + transform.forward * _speed * Time.fixedDeltaTime);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == 7)
        {
            _canMove = false;
        }
    }

    private void CauseDamage()
    {
        Collider[] colliders = Physics.OverlapSphere(transform.position, _swordDistance, _hitLayer);

        foreach (Collider collider in colliders)
        {
            if (collider.gameObject == gameObject) continue;

            Entity entity = collider.GetComponent<Entity>();
            if (entity == null)
            {
                GenericDestroyable destro = collider.GetComponent<GenericDestroyable>();
                if (destro != null)
                    destro.GetComponent<Idamageable>().TakeDamage(500, 0, Vector3.zero);
                continue;
            }

            float verticalDiff = Mathf.Abs(entity.transform.position.y - transform.position.y);
            if (verticalDiff > 2f) continue;
            if (!GameManager.Instance.LineOfSight(transform.position, entity.transform.position)) continue;

            Vector3 origin = transform.position + Vector3.up * 1f - transform.forward * 0.5f;
            Vector3 dirToEnemy = (entity.transform.position - origin).normalized;

            if (Physics.Raycast(origin, dirToEnemy, out RaycastHit hit, _swordDistance, _hitLayer))
            {
                if (hit.collider.transform.root != entity.transform.root) continue;

                Idamageable damageable = entity.GetComponent<Idamageable>();
                if (damageable != null)
                {
                    Vector3 pushDir = new Vector3(
                        (entity.transform.position - transform.position).x,
                        0f,
                        (entity.transform.position - transform.position).z
                    ).normalized;

                    damageable.TakeDamage(_dmg, 0, pushDir, false, false, true, _pushForce);
                }
            }
        }
    }
}
