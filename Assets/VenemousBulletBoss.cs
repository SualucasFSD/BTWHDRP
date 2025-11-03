using UnityEngine;

public class VenemousBulletBoss : MonoBehaviour
{
    [SerializeField] private float _dmg;
    private void OnTriggerEnter(Collider other)
    {
        PlayerController pj= other.GetComponent<PlayerController>();
        if(pj!=null)
        {
            Idamageable idamageable = pj.GetComponent<Idamageable>();
            if(idamageable!=null) 
            {
                idamageable.TakeDamage(_dmg, 0, Vector3.zero);
            }
        }
        else
        {
            Destroy(gameObject);
        }
    }
}
