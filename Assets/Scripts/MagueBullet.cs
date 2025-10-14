using UnityEngine;

[RequireComponent(typeof(Rigidbody), typeof(Collider))]
public class MagueBullet : MonoBehaviour
{
    [Header("References")]
    public Transform Tg;
    [SerializeField] private Rigidbody _rb;

    [Header("Stats")]
    [SerializeField] private float _speed = 15f;
    [SerializeField, Range(0.001f, 0.125f)] private float _rotForce = 0.05f;
    [SerializeField] private float _lifeTime = 10f;

    [Header("State")]
    public bool Fire = false;
    private bool _follow = true;
    private bool _wasClose = false;
    private float _dmg;

    public Entity.KindOfEntity Kind;

    private void Awake()
    {
        if (_rb == null)
            _rb = GetComponent<Rigidbody>();

        _rb.isKinematic = true;
    }

    private void OnEnable()
    {
        _follow = true;
        _wasClose = false;
        Invoke(nameof(DestroySelf), _lifeTime);
    }

    private void OnDisable()
    {
        CancelInvoke();
    }

    private void FixedUpdate()
    {
        if (!Fire) return;
        transform.parent = GameManager.Instance.Gameplay;
        _rb.isKinematic = false;

        if (Tg != null && _follow)
        {
            if (Vector3.Distance(Tg.position ,transform.position) < 4f)
            {
                _follow=false;
            }
            Quaternion targetRot = Quaternion.LookRotation((Tg.position - transform.position).normalized);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, _rotForce);
        }

        _rb.MovePosition(transform.position + transform.forward * _speed * Time.fixedDeltaTime);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!Fire) return;
        if (other.isTrigger) return;

        Entity e = other.GetComponent<Entity>();
        if (e != null)
        {
            if (e.Kind == Kind) return;

            Idamageable damageable = e.GetComponent<Idamageable>();
            if (damageable != null)
            {
                _dmg = GameManager.Instance.EnemyConfiguration[EnemyCatalogue.Mague].Damage;/*(Kind == Entity.KindOfEntity.Allies)
                    ? GameManager.Instance.EnemyConfiguration[EnemyCatalogue.Mague].Damage
                    : GameManager.Instance.EnemyConfiguration[EnemyCatalogue.Mague].Damage;
                    */
                damageable.TakeDamage(_dmg, 15f, Vector3.zero);
            }

            DestroySelf();
            return;
        }
        DestroySelf();
    }

    private void DestroySelf()
    {
        CancelInvoke();
        Destroy(gameObject);
    }
}

