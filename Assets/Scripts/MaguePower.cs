using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MaguePower : MonoBehaviour, IPjPower
{
    [SerializeField]MagueBullet _bulletPrefab;
    [SerializeField]Transform[] _bulletPos=new Transform[3];
    private int _numbOfBullets=0;
    private List<MagueBullet> _bullets=new List<MagueBullet>();
    [SerializeField]private Transform _tg=null;
    private List<Entity> _tgOp=new List<Entity>();
    private float _magicTimer=0;
    [SerializeField] Entity.KindOfEntity Kind;
    [SerializeField][Range(5,15)]private float _distanceShoot;
    [SerializeField] private PjModel _model;
    [SerializeField] private int _howMany;
    [SerializeField] private FallowObject _viewModel;
    private Coroutine _destroyRoutine;
    private GameObject _shooter;
    private void Start()
    {
        if(_model == null)
        {
            _model=GetComponentInParent<PjModel>();
        }
        _model.AddPower(EnemyCatalogue.Mague, Tuple.Create(_howMany, GetComponent<IPjPower>()));
        gameObject.SetActive(false);
    }

    private void FalseUpdate()
    {
        if (_tg == null)
        {
            if (_numbOfBullets > 0 && _destroyRoutine == null)
            {
                _destroyRoutine = StartCoroutine(DestroyBullets());
            }
            TakeCloseEnemy();
            return;
        }
        gameObject.transform.position=_shooter.transform.position;
        if (_destroyRoutine != null)
        {
            StopCoroutine(_destroyRoutine);
            _destroyRoutine = null;
        }

        TakeCloseEnemy();

        if (_numbOfBullets < 3)
        {
            _magicTimer += Time.deltaTime;

            if (_magicTimer > 1.5f)
            {
                transform.rotation = Quaternion.Slerp(transform.rotation,Quaternion.LookRotation(_tg.position - transform.position),0.05f);
                MagicInstance();
                _magicTimer = 0;
            }
        }
        else
        {
            _numbOfBullets = 0;
            _magicTimer = 0;
            Fire();
        }
    }

    public void MagicInstance()
    {
        if (_numbOfBullets >= _bulletPos.Length) return;

        //var bullet = Instantiate(_bulletPrefab, _bulletPos[_numbOfBullets].position, transform.rotation);
        GameObject p = GameObjectFactory.Instance.GetObj(GenericObjectType.MagueBullet, _bulletPos[_numbOfBullets].position, transform.rotation);
        p.transform.parent = _shooter.transform;
        MagueBullet bullet= p.GetComponent<MagueBullet>();
        bullet.Kind = Kind;
        bullet.transform.parent = transform;
       
        _bullets.Add(bullet);
        _numbOfBullets++;
    }

    private void Fire()
    {
        foreach (MagueBullet b in _bullets)
        {
            if (b != null)
            {
                b.SetTarget(_tg);
                b.Fire = true;
            }
        }

        _bullets.Clear();
        _numbOfBullets = 0;
    }

    private IEnumerator DestroyBullets()
    {
        while (_tg == null && _bullets.Count > 0)
        {
            yield return new WaitUntil(() => !GameManager.Instance.IsPaused);
            yield return new WaitForSeconds(1);

            if (_bullets.Count > 0)
            {
                var bullet = _bullets[_bullets.Count - 1];
                if (bullet != null)
                {
                    GameObjectFactory.Instance.ReturnObj(GenericObjectType.MagueBullet, bullet.gameObject);
                    //Destroy(bullet.gameObject);
                }

                _bullets.RemoveAt(_bullets.Count - 1);
                _numbOfBullets--;
            }
        }
        _destroyRoutine = null;
    }

    private void TakeCloseEnemy()
    {
        _tgOp = GameManager.Instance.RefreshEnemy(Kind);

        if (_tgOp.Count <= 0) { _tg = null; return; }

        _tg = null;
        float bestDist = Mathf.Infinity;

        foreach (Entity j in _tgOp)
        {
            if (!GameManager.Instance.LineOfSight(j.transform.position, transform.position))
                continue;

            float d = Vector3.Distance(transform.position, j.transform.position);

            if (d > _distanceShoot) continue;
            if (d < bestDist)
            {
                bestDist = d;
                _tg = j.transform;
            }
        }
    }

    public void Active()
    {
        var p= Instantiate(_viewModel, transform.position + Vector3.up * 5, transform.rotation).Target = transform;
        _shooter = p.gameObject;
        gameObject.SetActive(true);
        _model.EjecutePower += FalseUpdate;
    }

    private void OnDestroy()
    {
        if (_destroyRoutine != null) StopCoroutine(_destroyRoutine);
    }

}
