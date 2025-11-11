using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using static UnityEngine.EventSystems.EventTrigger;
public class BossModel : Entity, Idamageable
{
    [Header("Spawn Settings")]
    [SerializeField] private GameObject _venemousBulletPrefab;
    [SerializeField] private BigBullets _bulletPrefab;
    [SerializeField] private int _bulletCount = 12;
    [SerializeField] private float _radius = 5f;
    [SerializeField] private float _yOffset = -1;
    [SerializeField] private Transform _spitPoint;
    [SerializeField] private LayerMask _obstacleMask;
    [SerializeField] private float _fallDamage;
   //[SerializeField] private LayerMask _areaDamageMask;
    [SerializeField] private Rigidbody _rb;
    [SerializeField] private float _rotationForce;
    //[SerializeField] private float _gravityValue=9.8f;
    [SerializeField] private ParticleSystem _bloodVfx;
    //Privada y Opcional
    private float _spitTimer;
    private float _spitFinishTimer;
    private bool _prob=false;
    private bool _prob2 = false;
    //Private
    private bool _rotationActivate=false;
    private float _predictionTime = 0.5f;
    private Vector3 _lastPos;
    private Vector3 _tgPos;
    private List<BigBullets> _bullets = new List<BigBullets>();
    private float _notCloseTimer=0;
    private bool _isSpiting;
    private bool _inAction;
    #region Eventos
    public event Action<bool> Grounded = delegate { };
    public event Action Fallen=delegate { };
    public event Action JumpPrepare=delegate { };
    public event Action JumpExecute=delegate { };
    public event Action<bool> Spiting=delegate { };
    public event Action FalseUpdate=delegate { };
    public event Action MaxHeigh=delegate { };
    #endregion
    private void Awake()
    {
       if(_rb==null)
       {
          _rb = GetComponent<Rigidbody>();
       }
    }
    private void Start()
    {
        EventManager.Suscribe(EventManager.KindOfEvent.OnPjChangePosition, TakePjPosition);
        FalseUpdate += ChangeOperation;
        //_rotationActivate=true;
    }
    private void Update()
    {
        if(GameManager.Instance.IsPaused)
        {
            return;
        }
        FalseUpdate();
        if(_prob2)
        {
            _spitFinishTimer += Time.deltaTime;
            _spitTimer += Time.deltaTime;
            if(_spitTimer>0.3f)
            {
                _spitTimer = 0;
                SpitVenemousParabolicBullets();
            }
            if(_spitFinishTimer>3)
            {
                _rotationActivate = false;
                _spitTimer = 0;
                _spitFinishTimer = 0;
                Spiting(false);
                _prob2 = false;
            }
        }
        Grounded(IsGrounded);
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            Spiting(true);
            _rotationActivate = true;
        }
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            _prob = true;
        }
        if(_prob)
        {
            _prob = false;
            JumpPrepare();
            _rotationActivate =true;
        }
    }
    private void FixedUpdate()
    {
        if (GameManager.Instance.IsPaused)
        {
            return;
        }

        IsGroundedDetector();
        if (UseGravity)
        {
            _rb.AddForce(-Vector3.up * Mathf.Pow(GravValue, 2), ForceMode.Acceleration);
        }
        if(_rotationActivate)
        {
            RotateToTarget(_tgPos);
        }
    }
    private void ChangeOperation()
    {
        if (Vector3.Distance(_tgPos, transform.position) > 5f)
        {

        }
        else if (Vector3.Distance(_tgPos, transform.position) > 5f && !_inAction)
        {
            _notCloseTimer += Time.deltaTime;
        }
        if (_notCloseTimer > 8f && !_inAction)
        { 
            _inAction = true;
            _notCloseTimer = 0;
        }
    }
  
    public void JumpExecuteModel()
    {
        _rotationActivate=false;
        Vector3 AirPos = _tgPos + Vector3.up * 15;
        UseGravity = false;
        StartCoroutine(GoAir(AirPos)); 
    }
    IEnumerator GoAir(Vector3 targetPos)
    {
        Vector3 startPos = transform.position;

        float midY = targetPos.y * 0.8f;
        float maxY = targetPos.y;

        float horizontalSpeed = 40f;
        float verticalSpeed = 50f;
        float fallSpeed = 70f;
        JumpExecute();
        gameObject.layer = 18;
        while (transform.position.y < midY)
        {
            yield return new WaitUntil(() => !GameManager.Instance.IsPaused);
            Vector3 dir = (targetPos - transform.position);
            dir.y *= 2f;
            dir.Normalize();
            //RotateToTarget(_tgPos);
            transform.position += dir * verticalSpeed * Time.deltaTime;
            yield return null;
        }
        while (transform.position.y < maxY)
        {
            yield return new WaitUntil(() => !GameManager.Instance.IsPaused);
            Vector3 dir = (targetPos - transform.position);
            dir.y *= 0.7f;
            dir.Normalize();
            //RotateToTarget(_tgPos);
            transform.position += dir * horizontalSpeed * Time.deltaTime;
            yield return null;
        }
        yield return new WaitForSeconds(0.25f);
        MaxHeigh();
        Vector3 fallTarget = new Vector3(transform.position.x, startPos.y, transform.position.z);

        while (transform.position.y > fallTarget.y)
        {
            yield return new WaitUntil(() => !GameManager.Instance.IsPaused);
            transform.position = Vector3.MoveTowards(
                transform.position,
                fallTarget,
                fallSpeed * Time.deltaTime
            );
            yield return null;
        }
        UseGravity=true;
        IsGrounded = true;
        gameObject.layer = 10;
        SpawnCircularBullets();
        AreaDamage();
        print("Impacto y disparo circular realizado.");
    }
    private void AreaDamage()
    {
        var p= GameManager.Instance.RefreshEnemy(Kind);
        foreach(var r in p)
        {
            if(Vector3.Distance(transform.position-Vector3.up*2,r.transform.position)<7)
            {
                Idamageable damageable = r.GetComponent<Idamageable>();
                if(damageable!=null)
                {
                    Vector3 pushDir = new Vector3((r.transform.position - transform.position).x,0f,(r.transform.position - transform.position).z).normalized;
                    damageable.TakeDamage(_fallDamage,0,pushDir);
                }
            }
        }
    }
    private void PunchTheGround()
    {

    }
    private void CoolDown()
    {

    }
    private void TakePjPosition(params object[] p)
    {
        Vector3 playerPos = (Vector3)p[0];

        Vector3 playerVel = (playerPos - _lastPos) / Time.deltaTime;
        _lastPos = playerPos;

        Vector3 predictedPos = playerPos;
        if (playerVel.magnitude > 0.1f)
        {
            predictedPos += playerVel.normalized * playerVel.magnitude * _predictionTime;
        }
        Vector3 shootOrigin = transform.position;
        Vector3 dir = (predictedPos - shootOrigin).normalized;
        float dist = Vector3.Distance(shootOrigin, predictedPos);

        if (Physics.Raycast(shootOrigin, dir, out RaycastHit hit, dist, _obstacleMask))
        {
            predictedPos = hit.point;
        }

        _tgPos = predictedPos;
    }
    #region Powers
    public void SpitVenemousParabolicBullets()
    {
        if(_tgPos!=Vector3.zero)
        {
            GameObject bullet = Instantiate(_venemousBulletPrefab);
            bullet.transform.position = _spitPoint.position;
            GameManager.Instance.LaunchProjectile(bullet, _tgPos,45);
        }
    }
    public void SpawnCircularBullets()
    {
        if (_bulletPrefab == null) return;

        for (int i = 0; i < _bulletCount; i++)
        {
            float angle = i * (360f / _bulletCount);
            Vector3 dir = new Vector3(Mathf.Cos(angle * Mathf.Deg2Rad), 0, Mathf.Sin(angle * Mathf.Deg2Rad));
            Vector3 spawnPos = transform.position + dir * _radius;
            Quaternion rot = Quaternion.LookRotation(dir);
            BigBullets bullet = GameObjectFactory.Instance.GetObj(GenericObjectType.BigBullet, spawnPos + Vector3.up * _yOffset,rot).GetComponent<BigBullets>();
            //BigBullets bullet = Instantiate(_bulletPrefab, spawnPos+Vector3.up*_yOffset, rot);
            //bullet.transform.rotation = rot;
            _bullets.Add(bullet);
        }
        foreach(BigBullets b in _bullets)
        {
            b.Kind=KindOfEntity.Enemy;
            b.Fire=true;
        }

    }
    #endregion
    private void OnDestroy()
    {
       EventManager.Unscribe(EventManager.KindOfEvent.OnPjChangePosition, TakePjPosition);
    }
    #region Damageable
    public void TakeDamage(float dmg, float stunt, Vector3 pushDirection, bool downHit = false, bool isStuntDamage = false)
    {
       if(Life<=0)
       {
         return;
       }
       Life-=dmg;
       if(_bloodVfx!=null)
       {
          _bloodVfx.Play();
       }
       if(Life<=0)
       {
            UseGravity=true;
            StopAllCoroutines();
         print("Big Boss Dead");
       }
    }
    private void RotateToTarget(Vector3 Direction)
    {
        if (_rb == null && _tgPos != Vector3.zero) { return; }
        if (Direction.sqrMagnitude <= 0.001f) { return; }

            Direction.y = 0f;
        if (Direction.sqrMagnitude < 0.0001f) { return; }

        _rb.MoveRotation(Quaternion.Slerp(_rb.rotation, Quaternion.LookRotation(Direction.normalized, Vector3.up), _rotationForce * Time.fixedDeltaTime));
    }
    public void BeganSpitVenemous()
    {
        _prob2 = true;
    }
    public void TakeHealt(float amount)
    {
     
    }
    #endregion
    private void OnDrawGizmos()
    {
        if (_groundDetect.point != null)
        {
            if (Vector3.Distance(transform.position, _groundDetect.point) <= GroundDistanceDetector)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawSphere(_groundDetect.point, 0.3f);
            }
        }
    }
}
