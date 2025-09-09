using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;
[RequireComponent(typeof(Rigidbody))]
public class SkeletonEnemyModel : Entity, Idamageable
{
    [SerializeField] private Rigidbody _rb;
    public FsmEnemyEsqueleton _fsm = new FsmEnemyEsqueleton();
    public LayerMask _nodeLayer;
    public Vector3 _dir;
    public bool IsDodge;
    public bool IsDamageable = true;
    private float _stuntPercent;
    //Eventos
    public event Action<Vector3> OnMove = delegate { };
    public event Action OnAttack = delegate { };
    public event Action<float> OnDamage = delegate { };
    public event Action OnDeath = delegate { };
    //Cosas Varias
    public List<PathNode> _paths = new List<PathNode>();
    [SerializeField] private Animator _anim;
    [SerializeField] private LifeOrb _lifeOrbPrefab;
    [SerializeField] ParticleSystem _damageParticles;
    [SerializeField] AudioSource _mySource;
    private bool _isReady = false;
    [SerializeField]private float _onAirTime=3;
    [SerializeField] private LayerMask _airLayer;
    private bool _useGravity=true;
    private bool _stuned = false;
    private float _actualAirTime=0;
    private void Awake()
    {
        Kind = KindOfEntity.Enemy;
        if (_rb == null)
        {
            _rb = GetComponent<Rigidbody>();
        }
        _rb.useGravity = false;
    }
    private void OnEnable()
    {
        if(_isReady)
        {
          _fsm.ChangeState(FsmEnemyEsqueleton.AgentStates.OnPatrol);
          GameManager.Instance.AddEntity(this, Kind);
           Life = GameManager.Instance.EnemyConfiguration[EnemyCatalogue.Esqueleton].Life;
        }
    }
    private void OnDisable()
    {
        //_fsm.ChangeState(FsmEnemyEsqueleton.AgentStates.OnDeath);
        GameManager.Instance.RemoveEntity(this, Kind);
    }
    private void Start()
    {
        _fsm.AddState(FsmEnemyEsqueleton.AgentStates.OnPatrol, new OnPatrol(this, _nodeLayer,()=>_fsm.ChangeState(FsmEnemyEsqueleton.AgentStates.OnCombat),OnMovePj));
        _fsm.AddState(FsmEnemyEsqueleton.AgentStates.OnCombat, new OnCombatEsqueleton(_fsm, this,_anim));
        _fsm.AddState(FsmEnemyEsqueleton.AgentStates.OnDeath, new OnDeath());
        _fsm.AddState(FsmEnemyEsqueleton.AgentStates.OnStunt, new OnStunt(GameManager.Instance.EnemyConfiguration[EnemyCatalogue.Esqueleton].StuntTime,()=> _fsm.ChangeState(FsmEnemyEsqueleton.AgentStates.OnCombat),_anim,"Stunt"));
        _fsm.AddState(FsmEnemyEsqueleton.AgentStates.OnTakeDamage, new OnTakeDamage(_onAirTime, () => _fsm.ChangeState(FsmEnemyEsqueleton.AgentStates.OnCombat), _anim, "TakeDamage"));
        _fsm.ChangeState(FsmEnemyEsqueleton.AgentStates.OnPatrol);
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
        if (IsDodge)
        {
            _rb.AddForce(-transform.forward*1400,ForceMode.Impulse);
            IsDodge = false;
        }
        if(_useGravity)
        {
          _rb.AddForce(-transform.up * Mathf.Pow(GameManager.Instance.EnemyConfiguration[EnemyCatalogue.Esqueleton].GravityForce, 2), ForceMode.Acceleration);
        }
        if(Physics.Raycast(transform.position,-Vector3.up,1.2f)&&!_stuned)
        {
          _rb.MovePosition(transform.position + (transform.forward * _dir.z * GameManager.Instance.EnemyConfiguration[EnemyCatalogue.Esqueleton].Velocity * Time.fixedDeltaTime));
        }
    }
    public void TakeDamage(float dmg,float stunt, Vector3 pushDirection)
    {
        if (!IsDamageable) { return; }
        _stuntPercent += stunt;
        Life -=dmg;
        if(Physics.Raycast(transform.position, -Vector3.up, 1.2f))
        {
            MantainOnAir();
        }
        _fsm.ChangeState(FsmEnemyEsqueleton.AgentStates.OnTakeDamage);
        SoundManager.Instance.PlayOneShot(entityType.basic, soundType.attack, _mySource);
        _damageParticles.Play();
        //lifebar.value=life/maxlife;
        if (Life <= 0)
        {
            if (_lifeOrbPrefab != null)
            {
                StartCoroutine(SpawnOrbs());
            }
            GameManager.Instance.RemoveEntity(this, Kind);
            GetComponentInChildren<RagdollOnOff>().RagdollModeOn(pushDirection,50);
            EventManager.Ejecute(EventManager.KindOfEvent.OnEnemyKilled,GetComponent<Collider>(),EnemyCatalogue.Esqueleton);
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
    }
    public void OnMovePj(Transform target)
    {
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
    }
    public void FlyFunct(float height = 4f)
    {
        _stuned =true;
        _actualAirTime = 0;
        _fsm.ChangeState(FsmEnemyEsqueleton.AgentStates.OnTakeDamage);

        _useGravity = false;
        _rb.velocity = Vector3.zero;

        float targetY = transform.position.y + height;

        StartCoroutine(GoUpAndFloat(targetY));
    }

    private IEnumerator GoUpAndFloat(float targetY)
    {
        while (transform.position.y < targetY)
        {
            if (!_rb.isKinematic)
            {
                Vector3 pos = transform.position;
                pos.y = Mathf.MoveTowards(pos.y, targetY, 50f * Time.deltaTime);
                _rb.MovePosition(pos);
                yield return new WaitUntil(() => !GameManager.Instance.IsPaused);
            }
            yield return null;
        }

        while (_actualAirTime < _onAirTime - 0.5f)
        {
            if (!_rb.isKinematic)
            {
                _rb.MovePosition(transform.position);
                yield return new WaitUntil(() => !GameManager.Instance.IsPaused);
            }
            _actualAirTime += 0.1f;
            yield return new WaitForSeconds(0.1f);
        }

        _stuned = false;
        _useGravity = true;
    }

    private void MantainOnAir()
    {
        _actualAirTime=0;
    }
    private void OnDestroy()
    {
      
    }
}
