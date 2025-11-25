using UnityEngine;

public class ExpandingDamageField : MonoBehaviour
{
    [Header("Expansion")]
    public float expandSpeed = 2f;
    public Entity.KindOfEntity Kind;

    [Header("Damage")]
    public float damage = 20f;
    public float knockback = 15f;

    private void Start()
    {
        Destroy(gameObject, 10f);
    }

    private void Update()
    {
        ExpandXZ();
    }

    private void ExpandXZ()
    {
        transform.localScale += new Vector3(expandSpeed, 0f, expandSpeed) * Time.deltaTime;
    }

    /*private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent<Entity>(out var e))
        {
            if (e.Kind != Kind)
            {
                if (other.TryGetComponent<Idamageable>(out var damageable))
                {
                    damageable.TakeDamage(damage, knockback, Vector3.zero);
                }
            }

            return;
        }
        if (other.TryGetComponent<GenericDestroyable>(out var destro))
        {
            if (destro.TryGetComponent<Idamageable>(out var dmgD))
            {
                dmgD.TakeDamage(500, 0f, Vector3.zero);
            }
        }
    }*/
    private void OnCollisionEnter(Collision collision)
    {
        GameObject other = collision.gameObject;

        if (other.TryGetComponent<Entity>(out var e))
        {
            if (e.Kind != Kind)
            {
                if (other.TryGetComponent<Idamageable>(out var damageable))
                {
                    damageable.TakeDamage(damage, knockback, Vector3.zero);
                }
            }

            Physics.IgnoreCollision(
                collision.collider,
                GetComponent<Collider>(),
                true
            );

            return;
        }

        if (other.TryGetComponent<GenericDestroyable>(out var destro))
        {
            if (destro.TryGetComponent<Idamageable>(out var dmg))
            {
                dmg.TakeDamage(500, 0f, Vector3.zero);
            }

            Physics.IgnoreCollision(
                collision.collider,
                GetComponent<Collider>(),
                true
            );

            return;
        }

        Physics.IgnoreCollision(
            collision.collider,
            GetComponent<Collider>(),
            true
        );
    }
}
    /*private void OnCollisionEnter(Collision other)
    {
        if (other.TryGetComponent<Entity>(out var e))
        {
            if (e.Kind != Kind)
            {
                if (other.TryGetComponent<Idamageable>(out var damageable))
                {
                    damageable.TakeDamage(damage, knockback, Vector3.zero);
                }
            }

            return;
        }
        if (other.TryGetComponent<GenericDestroyable>(out var destro))
        {
            if (destro.TryGetComponent<Idamageable>(out var dmgD))
            {
                dmgD.TakeDamage(500, 0f, Vector3.zero);
            }
        }
    }*/
