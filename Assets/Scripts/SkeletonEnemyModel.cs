using System.Collections;
using System;
using UnityEngine;
using Random = UnityEngine.Random;
using System.Collections.Generic;

[RequireComponent(typeof(Rigidbody))]
public class SkeletonEnemyModel : Entity, Idamageable
{
    [SerializeField] private Rigidbody _rb;
    public FsmEnemyEsqueleton _fsm = new FsmEnemyEsqueleton();
    public LayerMask _nodeLayer;
    public Vector3 _dir;
    public bool IsDodge;
    public bool IsDamageable = true;
    public List<PathNode> _paths = new List<PathNode>();

    private float _stuntPercent;
    private bool _isReady = false;
    private bool _useGravity = true;
    private bool _stuned = false;
    private float _actualAirTime = 0;
    private Coroutine _orbsRoutine;

    [SerializeField] private Animator _anim;
    [SerializeField] private LifeOrb _lifeOrbPrefab;
    [SerializeField] private ParticleSystem _damageParticles;
    [SerializeField] private AudioSource _mySource;
    //Eventos
    public event Action<Vector3> OnMove = delegate { };
    public event Action OnAttack = delegate { };
    public event Action<float> OnDamage = delegate { };
    public event Action OnDeath = delegate { };
    [Header("Air Settings")]
    [SerializeField] private float _onAirTime = 3;
    [SerializeField] int _airLayer;
    [SerializeField] private float _ceilingOffset = 0.2f;
    private int _groundLayer;
    private void Awake()
    {
        Kind = KindOfEntity.Enemy;
        if (_rb == null)
            _rb = GetComponent<Rigidbody>();
        _rb.useGravity = false;
    }

    private void OnEnable()
    {
        if (_isReady)
        {
            _fsm.ChangeState(FsmEnemyEsqueleton.AgentStates.OnPatrol);
            GameManager.Instance.AddEntity(this, Kind);
            Life = GameManager.Instance.EnemyConfiguration[EnemyCatalogue.Esqueleton].Life;
        }
    }

    private void OnDisable()
    {
        GameManager.Instance.RemoveEntity(this, Kind);
    }
    private void Start()
    {
        _fsm.AddState(FsmEnemyEsqueleton.AgentStates.OnPatrol, new OnPatrol(this, _nodeLayer, () => _fsm.ChangeState(FsmEnemyEsqueleton.AgentStates.OnCombat), OnMovePj));
        _fsm.AddState(FsmEnemyEsqueleton.AgentStates.OnCombat, new OnCombatEsqueleton(_fsm, this, _anim));
        _fsm.AddState(FsmEnemyEsqueleton.AgentStates.OnDeath, new OnDeath());
        _fsm.AddState(FsmEnemyEsqueleton.AgentStates.OnStunt, new OnStunt(GameManager.Instance.EnemyConfiguration[EnemyCatalogue.Esqueleton].StuntTime, () => _fsm.ChangeState(FsmEnemyEsqueleton.AgentStates.OnCombat), _anim, "Stunt"));
        _fsm.AddState(FsmEnemyEsqueleton.AgentStates.OnTakeDamage, new OnTakeDamage(_onAirTime, () => _fsm.ChangeState(FsmEnemyEsqueleton.AgentStates.OnCombat), _anim, "TakeDamage"));

        _fsm.ChangeState(FsmEnemyEsqueleton.AgentStates.OnPatrol);
        _groundLayer=gameObject.layer;
        GameManager.Instance.AddEntity(this, Kind);
        Life = GameManager.Instance.EnemyConfiguration[EnemyCatalogue.Esqueleton].Life;
        _isReady = true;
    }
    private void Update()
    {
        _fsm.ArtificialUpdate();
    }

    private void FixedUpdate()
    {
        IsGroundedDetector();
        if(_useGravity&&IsGrounded)
        {
            gameObject.layer = _groundLayer;
        }
        if (IsDodge)
        {
            _rb.AddForce(-transform.forward * 1400, ForceMode.Impulse);
            IsDodge = false;
        }

        if (_useGravity)
        {
            _rb.AddForce(-transform.up * Mathf.Pow(GameManager.Instance.EnemyConfiguration[EnemyCatalogue.Esqueleton].GravityForce, 2), ForceMode.Acceleration);
        }

        if (Physics.Raycast(transform.position, -Vector3.up, 1.2f) && !_stuned)
        {
            _rb.MovePosition(transform.position + (transform.forward * _dir.z * GameManager.Instance.EnemyConfiguration[EnemyCatalogue.Esqueleton].Velocity * Time.fixedDeltaTime));
        }
    }
    public void TakeDamage(float dmg,float stunt, Vector3 pushDirection)
    {
        if (!IsDamageable) { return; }
        _stuntPercent += stunt;
        Life -=dmg;
        if(!IsGrounded)
        {
            MantainOnAir();
        }
        _fsm.ChangeState(FsmEnemyEsqueleton.AgentStates.OnTakeDamage);
        SoundManager.Instance.PlayOneShot(entityType.basic, soundType.attack, _mySource);
        _damageParticles.Play();
        //lifebar.value=life/maxlife;
        if (Life <= 0 && _orbsRoutine==null)
        {
            if (_lifeOrbPrefab != null)
            {
               _orbsRoutine = StartCoroutine(SpawnOrbs());
            }
            GameManager.Instance.RemoveEntity(this, Kind);
            GetComponentInChildren<RagdollOnOff>().RagdollModeOn(pushDirection,50);
            EventManager.Ejecute(EventManager.KindOfEvent.OnEnemyKilled,gameObject,EnemyCatalogue.Esqueleton);
            enabled = false;
            _fsm.ChangeState(FsmEnemyEsqueleton.AgentStates.OnPatrol);
            StartCoroutine(Restart());
        }
        /*if(pushDirection != Vector3.zero)
        {
            _rb.AddForce(pushDirection * 1000, ForceMode.Impulse);
        }*/
        if (_stuntPercent >= GameManager.Instance.EnemyConfiguration[EnemyCatalogue.Esqueleton].StuntResistance)
        {
            _stuntPercent = 0;
            _fsm.ChangeState(FsmEnemyEsqueleton.AgentStates.OnStunt);
        }
    }
    IEnumerator SpawnOrbs()
    {
        for (int i = 0; i < 3; i++)
        {
            Vector2 offset = Random.insideUnitCircle * 5;
            LifeOrb p = Instantiate(_lifeOrbPrefab, transform.position + Vector3.up * 1.2f, transform.rotation);
            p.transform.parent = GameManager.Instance.Gameplay;
            p.Amount = Random.Range(15, 25);
            GameManager.Instance.LaunchProjectile(p.gameObject, transform.position + new Vector3(offset.x, 0, offset.y));
            yield return new WaitForSeconds(0.5f);
        }
        _orbsRoutine = null;
    }
    public void OnMovePj(Transform target)
    {
        if(!IsGrounded)
        {
            return;
        }
        if (target == null)
        {
            _dir=Vector3.zero;
            if (OnMove != null)
            {
                OnMove(_dir);
            }
            return;
        }
        _dir = Vector3.forward*0.2f;
        if (OnMove != null)
        {
            OnMove(_dir);
        }
    }
    public void OnMovePj(Vector3 Dir)
    {
        if (!IsGrounded)
        {
            return;
        }
        _dir = Dir;
        if (OnMove != null)
        {
            OnMove(Dir);
        }
    }
    public void TakeHealt(float amount)
    {
        
    }
    IEnumerator Restart()
    {
       yield return new WaitForSeconds(15);
       Life = GameManager.Instance.EnemyConfiguration[EnemyCatalogue.Esqueleton].Life;
       GetComponentInChildren<RagdollOnOff>().RagdollModeOff();
       enabled = true;
       EsqueletonFctory.instance.ReturnObj(this);
    }
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        PathNode ant = null;
        if(_paths.Count > 0) { Gizmos.DrawRay(transform.position, _paths[0].transform.position-transform.position); }
        foreach(PathNode i in _paths)
        {
            if(ant != null)
            {
                Gizmos.DrawRay(ant.transform.position, i.transform.position - ant.transform.position);
            }
            ant = i;
        }
        Gizmos.color = Color.yellow;
        Gizmos.DrawRay(transform.position,-Vector3.up * 10);
        if (_groundDetect.point != null)
        {
            if (Vector3.Distance(transform.position, _groundDetect.point) <= GroundDistanceDetector)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawSphere(_groundDetect.point, 0.3f);
            }
        }
    }
    public override void  FlyFunct(float height = 4f)
    {
        _stuned = true;
        _actualAirTime = 0;
        _fsm.ChangeState(FsmEnemyEsqueleton.AgentStates.OnTakeDamage);

        _useGravity = false;
        if (!_rb.isKinematic)
            _rb.velocity = Vector3.zero;

        float targetY = transform.position.y + height;

        if (Physics.SphereCast(transform.position, GameManager.Instance.EnemyConfiguration[EnemyCatalogue.Esqueleton].Radius, Vector3.up, out RaycastHit hit, height, ~0))
        {
            targetY = hit.point.y - _ceilingOffset;
        }

       gameObject.layer = _airLayer;
        StartCoroutine(GoUpAndFloat(targetY));
    }
                                
    private IEnumerator GoUpAndFloat(float targetY)
    {
        while (transform.position.y < targetY)
        {
            yield return new WaitUntil(() => !GameManager.Instance.IsPaused);
            if (!_rb.isKinematic)
            {
                Vector3 pos = transform.position;
                pos.y = Mathf.MoveTowards(pos.y, targetY, 50f * Time.deltaTime);
                _rb.MovePosition(pos);
            }
            yield return null;
        }

        while (_actualAirTime < _onAirTime - 0.5f)
        {
            yield return new WaitUntil(() => !GameManager.Instance.IsPaused);
            if (!_rb.isKinematic)
            {
                _rb.MovePosition(transform.position);
            }
            _actualAirTime += 0.1f;
            yield return new WaitForSeconds(0.1f);
        }
        _stuned = false;
        _useGravity = true;
    }
    public override void GetToTheGround()
    {
        _actualAirTime = 10f;
    }
    private void MantainOnAir()
    {
        _actualAirTime=0;
    }
    private void OnDestroy()
    {
      
    }
}
