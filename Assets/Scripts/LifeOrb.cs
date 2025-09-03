using UnityEngine;
[RequireComponent(typeof(Rigidbody))]
public class LifeOrb : MonoBehaviour
{
    PjModel _pj;
    public float Amount = 0;
    private Collider[] _close;
    [SerializeField] private LayerMask _pjLayer;
    float _time = 0;
    Rigidbody _rb;
    [SerializeField] private LayerMask _noIgnore;
    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();
    }
    void Update()
    {
        if(Physics.Raycast(transform.position,-transform.up,1.5f,_noIgnore))
        {
            _rb.useGravity = false;
            _rb.constraints = RigidbodyConstraints.FreezeAll;
        }
        _time += Time.deltaTime;
            if (_pj != null)
            {
                if (Vector3.Distance(_pj.transform.position, transform.position) < 1)
                {
                    _pj.GetComponent<Idamageable>().TakeHealt(Amount);
                    Destroy(gameObject);
                }
                else
                {
                    transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(_pj.transform.position - transform.position), 0.40f);
                    transform.position += transform.forward * Time.deltaTime * 4;
                }
            }
    }

    private void OnTriggerStay(Collider other)
    {
        if (_time > 1f)
        {
            _pj = other.GetComponent<PjModel>();
            GetComponent<Rigidbody>().isKinematic = true;
        }
        
    }
}
