using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class RayShoot : MonoBehaviour
{
    [SerializeField] private float _vel;
    [SerializeField] private float _dmg;
    [SerializeField] private string _matValueName;
    [SerializeField] private Rigidbody _rb;
    [SerializeField] private Entity.KindOfEntity Kind;
    [SerializeField] private LayerMask _obstacleMask;

    private Material _mat;
    private bool _fire = false;
    private bool _hasTarget = false;
    private float _lifeTimer = 0f;

    private void Awake()
    {
        if (_rb == null) _rb = GetComponent<Rigidbody>();
       // if (_mat == null) _mat = GetComponent<Renderer>().material;
    }

    public void Active()
    {
        StartCoroutine(Initialize());
    }

    //public void GetTg(Vector3 tg)
    //{
    //    Debug.DrawLine(transform.position, tg);
    //    transform.parent = GameManager.Instance.Gameplay;
    //    //transform.SetParent(null);
    //    transform.forward = (tg - _rb.position).normalized;
    //    _hasTarget = true;
    //}
    public void GetTg(Vector3 tg)
    {
        transform.SetParent(GameManager.Instance.Gameplay, true);

        transform.rotation = Quaternion.Euler(0, 0, 0);

        Vector3 dir = (tg - transform.position).normalized;

        transform.forward = dir;

        _hasTarget = true;
    }

    private void Update()
    {
        if (GameManager.Instance.IsPaused)
        {
            return;
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

    IEnumerator Initialize()
    {
       /* float t = 0f;
        float start = _mat.GetFloat(_matValueName);

        while (t < 1f)
        {
            t += Time.deltaTime;
            float val = Mathf.Lerp(start, 1f, t);
            _mat.SetFloat(_matValueName, val);
            yield return null;
        }*/

       yield return new WaitForSeconds(1);

        _fire = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!_fire || !_hasTarget || other.isTrigger)
            return;

        Entity e = other.GetComponent<Entity>();

        if (e != null)
        {
            if (e.IsGrounded)
                EventManager.Ejecute(EventManager.KindOfEvent.PauseOneEnemy, other.gameObject, 1.5f);

            if (e.Kind == Kind)
                return;

            if (e.TryGetComponent<Idamageable>(out var damageable))
                damageable.TakeDamage(_dmg, 15f, Vector3.zero);
        }

        Destroy(gameObject);
    }
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawLine(transform.position, transform.position + transform.forward * 2f);
    }
}
//using System.Collections;
//using TMPro; 
//using UnityEngine;
//[RequireComponent(typeof(Rigidbody))] public class RayShoot : MonoBehaviour
//{
//    [SerializeField] private float _vel;
//    [SerializeField] private float _dmg;
//    [SerializeField] private string _matValueName;
//    [SerializeField] private Rigidbody _rb;
//    [SerializeField] private Entity.KindOfEntity Kind;
//    [SerializeField] private LayerMask _obstacleMask;
//    private bool _hasTarget = false;
//    private Material _mat;
//    private bool _fire = false; private float _lifeTimer = 0f;
//    private void Awake()
//    {
//        if (_rb == null)
//        {
//            _rb = GetComponent<Rigidbody>();
//        }
//       /* if (_mat == null)
//        { 
//            _mat = GetComponent<Renderer>().material; 
//        }*/
//    }
//    public void Active()
//    {
//        StartCoroutine(Initialize()); 
//    }
//    public void GetTg(Vector3 tg)
//    {
//        transform.parent=GameManager.Instance.Gameplay;
//        _hasTarget=true;
//        transform.forward = (tg - _rb.position).normalized;
//    }
//    private void Update()
//    {
//        if (_fire&&_hasTarget)
//        { 
//            _lifeTimer += Time.deltaTime; if (_lifeTimer >= 10f) { Destroy(gameObject); } 
//        } 
//    }
//    private void FixedUpdate()
//    { 
//        if (_fire&&_hasTarget) 
//        { 
//            _rb.MovePosition(_rb.position + transform.forward * _vel * Time.fixedDeltaTime); 
//        }
//    } 
//    IEnumerator Initialize()
//    {
//       /* float t = 0; float start = _mat.GetFloat(_matValueName);
//        while (t < 1f)
//        { 
//            t += Time.deltaTime; float val = Mathf.Lerp(start, 1f, t);
//            _mat.SetFloat(_matValueName, val); yield return null;
//        }*/
//        yield return new WaitForSeconds(1);
//        _fire = true;
//    }
//    private void OnTriggerEnter(Collider other)
//    {
//        if (!_fire || other.isTrigger)
//        { 
//            return;
//        } 
//        Entity e = other.GetComponent<Entity>();
//        if (e != null)
//        {
//            if (e.IsGrounded)
//            {
//                EventManager.Ejecute(EventManager.KindOfEvent.PauseOneEnemy, other.gameObject, 1.5f);
//            }
//            if (e.Kind == Kind) 
//            {
//                return;
//            } 
//            if (e.TryGetComponent<Idamageable>(out var damageable))
//            { 
//                damageable.TakeDamage(_dmg, 15f, Vector3.zero);
//            }
//        }
//        Destroy(gameObject);
//    }
//}