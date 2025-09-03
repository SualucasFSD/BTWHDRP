using System;
using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using Random = UnityEngine.Random;
[RequireComponent(typeof(Rigidbody))]
public class MagueEnemyModel : Entity, Idamageable
{
    [SerializeField] private Rigidbody _rb;
    public FsmMague _fsm = new FsmMague();
    public LayerMask _nodeLayer;
    public bool IsCharging = false;
    private float _stuntPercent;
    //Eventos
    public event Action<Vector3> OnMove = delegate { };
    public event Action OnAttack = delegate { };
    public event Action OnCharging= delegate { };
    public event Action<float> OnDamage = delegate { };
    public event Action OnDeath = delegate { };
    [SerializeField] private Animator _anim;
    [SerializeField] private MagueBullet _bulletPrefab;
    private List<MagueBullet> Bullets=new List<MagueBullet>();
    [SerializeField] private Transform[] _bulletPos=new Transform[3];
    [SerializeField] private LifeOrb _lifeOrbPrefab;
    [SerializeField] ParticleSystem _damageParticles;
    [SerializeField] AudioSource _mySource;
    private int _numbOfBullets=0;
    private bool _isReady=false;
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
        //_fsm.ChangeState(FsmMague.MagueStates.OnPatrol);
        if (_isReady)
        {
            _fsm.ChangeState(FsmMague.MagueStates.OnPatrol);
        }
        GameManager.Instance.AddEntity(this, Kind);
    }
    private void OnDisable()
    {
        //_fsm.ChangeState(FsmMague.MagueStates.OnDeath);
        GameManager.Instance.RemoveEntity(this, Kind);
    }
    private void Start()
    {
         GameManager.Instance.AddEntity(this, Kind);
        _fsm.AddState(FsmMague.MagueStates.OnPatrol, new OnPatrol(this,_nodeLayer,()=>_fsm.ChangeState(FsmMague.MagueStates.OnCombat),OnMovePj));
        _fsm.AddState(FsmMague.MagueStates.OnCombat, new OnCombatMague(_fsm, this));
        _fsm.AddState(FsmMague.MagueStates.OnDeath, new OnDeath());
        _fsm.AddState(FsmMague.MagueStates.OnStunt, new OnStunt(GameManager.Instance.EnemyConfiguration[EnemyCatalogue.Esqueleton].StuntTime, () => _fsm.ChangeState(FsmMague.MagueStates.OnCombat), _anim, "Stunt"));
        _fsm.ChangeState(FsmMague.MagueStates.OnPatrol);
        _isReady = true;
    }
    private void Update()
    {
        _fsm.ArtificialUpdate();
    }
    private void FixedUpdate()
    {
        _rb.AddForce(-transform.up * Mathf.Pow(GameManager.Instance.EnemyConfiguration[EnemyCatalogue.Mague].GravityForce, 2), ForceMode.Acceleration);
        _rb.MovePosition(transform.position + Dir*Time.fixedDeltaTime);
    }
    public void TakeDamage(float dmg, float stunt, Vector3 pushDirection)
    {
        _stuntPercent += stunt;
        Life -= dmg;
        _damageParticles.Play();
        SoundManager.Instance.PlayOneShot(entityType.basic, soundType.attack, _mySource);
        //lifebar.value=life/maxlife;
        //y ejecuto OnTakeDamageEvento
        if (Life <= 0)
        {
            if (_lifeOrbPrefab!=null)
            {
                StartCoroutine(SpawnOrbs());
            }
            foreach(MagueBullet b in Bullets)
            {
                Destroy(b.gameObject);
            }
            Bullets.Clear();
            _numbOfBullets = 0;
            GameManager.Instance.RemoveEntity(this, Kind);
            GetComponentInChildren<RagdollOnOff>().RagdollModeOn(pushDirection,30);
            EventManager.Ejecute(EventManager.KindOfEvent.OnEnemyKilled, GetComponent<Collider>(),EnemyCatalogue.Mague);
            enabled = false;
            StartCoroutine(Restart());
        }
        if (_stuntPercent>= GameManager.Instance.EnemyConfiguration[EnemyCatalogue.Mague].StuntResistance)
        {
            _stuntPercent = 0;
            foreach (MagueBullet b in Bullets)
            {
                Destroy(b.gameObject);
            }
            _numbOfBullets=0;
            Bullets.Clear();
            _fsm.ChangeState(FsmMague.MagueStates.OnStunt);
        }
    }
    IEnumerator Restart()
    {
        yield return new WaitForSeconds(15);
        Life = GameManager.Instance.EnemyConfiguration[EnemyCatalogue.Mague].Life;
        GetComponentInChildren<RagdollOnOff>().RagdollModeOff();
        enabled = true;
        _fsm.ChangeState(FsmMague.MagueStates.OnPatrol);
        MagueFactory.instance.ReturnObj(this);
    }
    public void MagicInstance(Transform _tg)
    {
       Bullets.Add(Instantiate(_bulletPrefab, _bulletPos[_numbOfBullets].position, transform.rotation));
       Bullets[_numbOfBullets].Tg = _tg;
       Bullets[_numbOfBullets].Kind = Kind;
       _numbOfBullets++;
    }
    public void Shoot()
    {
        IsCharging = false;
        foreach (MagueBullet b in Bullets)
        {
            b.Fire = true;
        }
        Bullets.Clear();
        _numbOfBullets = 0;
        if (OnAttack != null)
        {
            OnAttack();
        }
    }
    public void StartCharging()
    {
        IsCharging = true;
        Dir=Vector3.zero;
        if (OnCharging != null) { OnCharging(); }
    }
    IEnumerator SpawnOrbs()
    {
        for (int i = 0; i < 3; i++)
        {
            Vector2 offset = Random.insideUnitCircle * 5;
            LifeOrb p = Instantiate(_lifeOrbPrefab, transform.position + Vector3.up * 1.2f, transform.rotation);
            p.transform.parent = GameManager.Instance.Gameplay;
            p.Amount = Random.Range(20, 30);
            GameManager.Instance.LaunchProjectile(p.gameObject, transform.position + new Vector3(offset.x, 0, offset.y));
            yield return new WaitForSeconds(0.5f);
        }
    }
    public void OnMovePj(Transform tg)
    {
       if(tg == null)
        {
            Dir = Vector3.zero;
            if (OnMove != null)
            {
                OnMove(Vector3.zero);
            }
            return;
        }
        AddForce(IaMov.Instance.Arrive(this, tg, GameManager.Instance.EnemyConfiguration[EnemyCatalogue.Mague].Velocity, GameManager.Instance.EnemyConfiguration[EnemyCatalogue.Mague].RotForce));
        if (OnMove != null)
        {
            OnMove(Dir);
        }
    }
    public void TakeHealt(float amount)
    {

    }
    private void AddForce(Vector3 target)
    {
        if (target.magnitude == 0){ return; }
        Dir = Vector3.ClampMagnitude(Dir + target, GameManager.Instance.EnemyConfiguration[EnemyCatalogue.Mague].Velocity);
    }
    private void OnDestroy()
    {
       
    }
}
