using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using Unity.VisualScripting;
using UnityEngine.VFX;

[RequireComponent(typeof(Rigidbody))]
public class FirstBoss : Entity, Idamageable
{
    private delegate void UpdateGeneral();
    UpdateGeneral FalseUpdate;
   // [SerializeField] private GameObject[] _handsPoint = new GameObject[2];
    [SerializeField] private GameObject _handsPivot;
   //[SerializeField] private GameObject _bulletPrefab;
    [SerializeField] private float _damage;
    //[SerializeField] private GameObject _camera;
    //private GameObject _cameraMain;
    [SerializeField] private float _life;
    [SerializeField] private float _lifeMax;
    [SerializeField] private Rigidbody _rig;
    private bool _activated = false;
    [SerializeField] private Image _healtBar;
    //[SerializeField] private GameObject _point;
    [SerializeField] private Animator _anim;
    [SerializeField] private Animator _handsAnim;
    [SerializeField] private bool[] _phase=new bool[3];
    [SerializeField] private float _impulseForce=10;
    [SerializeField] private float _rotationSpeed;
    //[SerializeField] private Spawner _spawner;
    //[SerializeField] private GameObject _cruz;
    [SerializeField] private GameObject lifebarToClose;
    [SerializeField] private AreaBoss _Area;
    [SerializeField] private GameObject _key;
    [SerializeField] MusicManager _myMusic;
    private float _randomEvent;
    private bool _isWalkin = false;
    private float _timeTackle;
    public bool _handsInUse=true;
    private float _spawnDelay=0;
    private float _spawnCoolDown=5;
    private float _clapDelay;
    private float _clapCoolDown=8;
    private bool _canHitCollider;
    [SerializeField] private GameObject Pj;
    [SerializeField] private LayerMask _hitAble;
    private bool _isActive =false;
    [SerializeField] ParticleSystem _myDamageParticles;
    [SerializeField] AudioSource _mySource;
    private void Awake()
    {
        _phase[0]=false;
        _phase[1]=false;
        _phase[2]=false;
        _life = _lifeMax;
        _healtBar.fillAmount=_life/_lifeMax;
        //_cameraMain = Camera.main.gameObject;
        _rig = GetComponent<Rigidbody>();
        GameManager.Instance.AddEntity(this,KindOfEntity.Enemy);
        EventManager.Suscribe(EventManager.KindOfEvent.OnDeath, OnPjKilled);
    }
    private void Update()
    {
        if (Vector3.Distance(transform.position, Pj.transform.position)<50&&!_isActive)
        {
            Begin();
            _isActive = true;
        }
        _timeTackle += Time.deltaTime;
        _spawnDelay += Time.deltaTime;
        _clapDelay += Time.deltaTime;
        if (FalseUpdate != null)
        {
            FalseUpdate();
        }
      
    }
    public void FallActivate()
    {
        _rig.constraints = ~RigidbodyConstraints.FreezePositionY;
    }
    private void SelectRoutine()
    {
        if (_activated)
        {
            if (_phase[0])
            {
                _randomEvent = Random.Range(0, 101);
                if (_randomEvent >= 35)
                {
                    StartCoroutine(BasicPunch());
                    FalseUpdate -= SelectRoutine;
                    _activated = false;
                }
                else
                {
                    StartCoroutine(Tackle());
                    FalseUpdate -= SelectRoutine;
                    _activated = false;
                }
            }
            else if (_phase[1])
            {
                _randomEvent = Random.Range(0, 101);
                if (_randomEvent >= 65)
                {
                    StartCoroutine(BasicPunch());
                    FalseUpdate -= SelectRoutine;
                    _activated = false;
                }
                else
                {
                    StartCoroutine(Tackle());
                    FalseUpdate -= SelectRoutine;
                    _activated = false;
                }
               
            }
            else if (_phase[2])
            {
                _randomEvent = Random.Range(0, 101);
                if (_randomEvent >= 65)
                {
                    StartCoroutine(BasicPunch());
                    FalseUpdate -= SelectRoutine;
                    _activated = false;
                }
                else if (_randomEvent < 65)
                {
                    StartCoroutine(Tackle());
                    FalseUpdate -= SelectRoutine;
                    _activated = false;
                }
            }
           if (_spawnDelay >_spawnCoolDown && (_phase[2] || _phase[1]))
            {
                // _spawner.SpawnReady();
                print("SpawneoBichos");
                _spawnCoolDown = (Random.Range(10, 16));
                _spawnDelay = 0;
            }
            if (_clapDelay > _clapCoolDown && _phase[2]&& _handsInUse==false)
            {
                StartCoroutine(HandsSlap());
                _clapCoolDown = (Random.Range(10, 16));
                _clapDelay = 0;
                _handsInUse = true;
            }
        }
    }
    private void FixedUpdate()
    {
        if( _isWalkin )
        {
            _rig.MovePosition(transform.position+(transform.forward.normalized*5*Time.fixedDeltaTime));
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(-transform.position + new Vector3(Pj.transform.position.x,transform.position.y, Pj.transform.position.z)),_rotationSpeed*2.25f*Time.fixedDeltaTime);
        }
    }
    IEnumerator HandsSlap()
    {

        while (Vector3.Distance(_handsPivot.transform.position, Pj.transform.position) > 1)
        {
            _handsPivot.GetComponent<HandsMove>()._onMove = true;
            yield return null;
        }
        new WaitUntil(() => !GameManager.Instance.IsPaused);
        _handsPivot.GetComponent<HandsMove>()._onMove = false;
        _handsAnim.SetBool("Slap",true);

    }
    public void Begin()
    {
        _anim.SetBool("Begin", true);
        _myMusic.StartMusic();
        _timeTackle = 0;
        StartCoroutine(_Area.Initialize());
        //_rig.constraints = RigidbodyConstraints.FreezePositionY;
        //_rig.constraints = ~RigidbodyConstraints.FreezePosition;
        lifebarToClose.SetActive(true);
        _activated = true;
        _phase[0] = true;
        FalseUpdate += SelectRoutine;
    }
    IEnumerator BasicPunch()
    {
        _anim.SetBool("Walk",true);
        while(Vector3.Distance(transform.position,Pj.transform.position)>2.5f)
        {
            _isWalkin = true;
            new WaitUntil(() => !GameManager.Instance.IsPaused);
            yield return null;
        }
        _isWalkin = false;
        while (Vector3.Dot(transform.forward, (Pj.transform.position - transform.position).normalized) < 0.60f)
        {
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(-transform.position + new Vector3(Pj.transform.position.x, transform.position.y, Pj.transform.position.z)), _rotationSpeed*2.25f * Time.fixedDeltaTime);
            new WaitUntil(() => !GameManager.Instance.IsPaused);
            yield return null;
        }
        _anim.SetBool("Walk", false);
        _anim.SetBool("Golpe", true);
        new WaitUntil(() => !GameManager.Instance.IsPaused);
        yield return new WaitForSeconds(4);
        _anim.SetBool("Golpe", false);
        _activated = true;
        FalseUpdate += SelectRoutine;
    }
   /* IEnumerator HandsShoot()
    {
        _handsAnim.SetBool("Shooting", true);
        yield return null;
        FalseUpdate += SelectRoutine;
        _activated = true;
    }*/
    IEnumerator Tackle()
    {
        _timeTackle = 0;
        int p;
        p = Random.Range(3, 5);
        _anim.SetBool("Charge", true);
        while (_timeTackle<=p)
        {
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(-transform.position + new Vector3(Pj.transform.position.x, transform.position.y, Pj.transform.position.z)), _rotationSpeed*0.75f * Time.fixedDeltaTime);
            new WaitUntil(() => !GameManager.Instance.IsPaused);
            yield return null;
        }
        _anim.SetBool("Charge", false);
        _canHitCollider = true;
        _anim.SetBool("Burst",true);
        //FrontBurst();
        new WaitUntil(() => !GameManager.Instance.IsPaused);
        yield return new WaitForSeconds(2.3f);
        _anim.SetBool("Burst", false);
        _canHitCollider = false;
        // yield return new WaitForSeconds(Random.Range(0, 2f));
        _activated = true;
        FalseUpdate += SelectRoutine;
    }
    public void CompleteActivate()
    {
        _rig.constraints = RigidbodyConstraints.FreezePositionY;
        _rig.constraints = ~RigidbodyConstraints.FreezePosition;
        //_camera.SetActive(false);
        //_cameraMain.SetActive(true);
        _activated = true;
        //_anim.SetBool("Start",true);
        //GameManager.Instance.PlayerGet().isPaused = false;
        _phase[0] = true;
        FalseUpdate += SelectRoutine;
        //Destroy(_cruz);
    }
    public void GolpeDamage()
    {
        Collider[] damageable = Physics.OverlapSphere(transform.position, 5,_hitAble);
        foreach (Collider c2 in damageable)
        {
            if (c2.gameObject == gameObject) continue;
            if (c2.GetComponent<Idamageable>() != null)
            {
                if (Vector3.Dot(transform.forward, (c2.transform.position - transform.position).normalized) > 0.55f && Vector3.Distance(transform.position, c2.transform.position) <= 11)
                {
                    c2.GetComponent<Idamageable>().TakeDamage(_damage, 0, Vector3.zero);
                }
            }
        }
        _anim.SetBool("Golpe", false);
    }
    public void FrontBurst()
    {
        _rig.AddForce(transform.forward * _impulseForce, ForceMode.Impulse);
    }
    public void TakeDamage(float Dmg, float stunt, Vector3 pushDirection, bool downHit = false, bool isStunDamage = false, float pushForce = 1000   )
    {
        _life -= Dmg;
        SoundManager.Instance.PlayOneShot(entityType.basic, soundType.attack, _mySource);
        _healtBar.fillAmount = _life / _lifeMax;
        _myDamageParticles.Play();

        if(_life<=_lifeMax*0.65f&&_life>_lifeMax*0.35f)
        {
            _phase[0] = false;
            _phase[1] = true;
            //StartCoroutine(HandsPrepare());
        }
        else if(_life<=_lifeMax*0.35f)
        {
            _phase[1]=false;
            _phase[2] = true;
            if (!_myMusic.isOndSecond)
                StartCoroutine(_myMusic.TransitionToSecondHalf());
            
            
            
           //StartCoroutine(HandsPrepare());
           StartCoroutine(_handsPivot.GetComponent<HandsMove>().Initialize());
        }
        if(_life<=0)
        {
            _myMusic.canEnd = true;
            GameManager.Instance.RemoveEntity(this, KindOfEntity.Enemy);
            _activated = false;
            _handsInUse = true;
            Destroy(_Area.gameObject);
            //Destroy(_spawner);
            //_nav.BuildNavMesh();
            Destroy(_handsPivot);
            Destroy(lifebarToClose);
            _key.SetActive(true);
            StopAllCoroutines();
            Destroy(gameObject);
        }
    }
    public void TakeHealt(float Healt) { }

    private void OnTriggerEnter(Collider other)
    {
        
    }

    private void OnCollisionEnter(Collision collision)
    {
        if(_canHitCollider)
        {
            Collider[] c = Physics.OverlapSphere(transform.position, 10,_hitAble);
            foreach(Collider c2 in c)
            {
                if (c2.gameObject == gameObject) continue;
                if (c2.GetComponent<Idamageable>() != null)
                {
                    c2.GetComponent<Idamageable>().TakeDamage(_damage * 1.5f, 0, Vector3.zero);
                }
            }
            _canHitCollider = false;
        }
    }
    private void OnPjKilled(params object[] p)
    {
        enabled=false;
    }
    private void OnDestroy()
    {
        EventManager.Unscribe(EventManager.KindOfEvent.OnDeath,OnPjKilled);
    }
}
