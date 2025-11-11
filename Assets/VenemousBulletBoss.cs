//using UnityEngine;

//public class VenemousBulletBoss : MonoBehaviour
//{
//    [SerializeField] private float _dmg;
//    private void OnTriggerEnter(Collider other)
//    {
//        PlayerController pj= other.GetComponent<PlayerController>();
//        if(pj!=null)
//        {
//            Idamageable idamageable = pj.GetComponent<Idamageable>();
//            if(idamageable!=null) 
//            {
//                idamageable.TakeDamage(_dmg, 0, Vector3.zero);
//            }
//        }
//        else
//        {
//            Destroy(gameObject);
//        }
//    }
//}
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class VenemousBulletBoss : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PoisonArea _poisonPrefab;
    [SerializeField] private LayerMask _groundLayer;

    [Header("Stats")]
    [SerializeField] private float _dmg = 15f;
    [SerializeField] private float _poisonSpawnOffset = 0.5f;

    private bool _hasSpawned = false;

    private void OnTriggerEnter(Collider other)
    {
        if (_hasSpawned)
            return;

        PlayerController pj = other.GetComponent<PlayerController>();
        if (pj != null)
        {
            if (pj.TryGetComponent<Idamageable>(out var idamageable))
            {
                idamageable.TakeDamage(_dmg, 0f, Vector3.zero);
            }

            SpawnPoisonArea();
            _hasSpawned = true;
            Destroy(gameObject);
            return;
        }

        if (((1 << other.gameObject.layer) & _groundLayer) != 0 || other.CompareTag("Ground"))
        {
            SpawnPoisonArea();
            _hasSpawned = true;
            Destroy(gameObject);
        }
    }

    private void SpawnPoisonArea()
    {
        Vector3 spawnPos = transform.position + Vector3.down * _poisonSpawnOffset;

        if (Physics.Raycast(transform.position, Vector3.down, out RaycastHit hit, 5f, _groundLayer))
        {
            spawnPos = hit.point;
        }

        PoisonArea area = Instantiate(_poisonPrefab, spawnPos, Quaternion.identity);
        Debug.Log($" Área de veneno instanciada en {spawnPos} por {name}");
    }
}