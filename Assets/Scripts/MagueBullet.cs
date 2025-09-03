using UnityEngine;
[RequireComponent(typeof(Rigidbody))]
public class MagueBullet : MonoBehaviour
{
    public Transform Tg;
    [SerializeField]private Rigidbody _rb;
    [SerializeField] private float _speed;
    [SerializeField][Range(0.001f,.125f)] private float _rotForce;
    private bool _fallow=true;
    public Entity.KindOfEntity Kind;
    public bool Fire=false;
    private float _dmg;
    private float _followTimer = 0;
    private void OnEnable()
    {
        _fallow = true;  
    }
    private void Awake()
    {
        if (_rb == null)
        {
            _rb = GetComponent<Rigidbody>();
        }
    }
    private void FixedUpdate()
    {
        _followTimer += Time.deltaTime;
        if (!Fire) { return; }
        transform.parent = GameManager.Instance.Gameplay;
        _rb.isKinematic=false;
        if (Tg != null && _fallow)
        {
          if (Vector3.Distance(transform.position, Tg.position) < 3f&&_followTimer>1.5f)
          {
           _fallow = false;
           return;
          }
          transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(Tg.position - transform.position), _rotForce);
        }
       _rb.MovePosition(transform.position + transform.forward * _speed * Time.fixedDeltaTime);
    }
    private void OnTriggerEnter(Collider other)
    {
        Entity e = other.GetComponent<Entity>();
        if (e != null)
        {
            if (e.Kind != Kind)
            {
                Idamageable damageable = e.GetComponent<Idamageable>();
                if (damageable != null)
                {
                    damageable.TakeDamage((Kind==Entity.KindOfEntity.Allies)? _dmg = GameManager.Instance.EnemyConfiguration[EnemyCatalogue.Mague].Damage / 2: _dmg = GameManager.Instance.EnemyConfiguration[EnemyCatalogue.Mague].Damage,15, Vector3.zero);
                }
                Destroy(gameObject);
            }
          return;
        }
        Destroy(gameObject);
    }
    private void OnDisable()
    {
       
    }
    private void OnDestroy()
    {
    
    }
}
