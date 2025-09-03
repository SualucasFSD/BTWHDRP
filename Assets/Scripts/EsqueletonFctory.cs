using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EsqueletonFctory : MonoBehaviour
{
    public static EsqueletonFctory instance { get; private set; }

    private PoolGeneral<SkeletonEnemyModel> _bulletPool;
    //[SerializeField] private SkeletonEnemyModel _bulletPreFab;

    [SerializeField] private SkeletonEnemyModel _prefab;
    [SerializeField] private int _initialAmount;
    [SerializeField] private Transform _parent;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            //DontDestroyOnLoad(gameObject);
        }
        _bulletPool = new PoolGeneral<SkeletonEnemyModel>(() => Instantiate(_prefab,_parent), (bullet) => bullet.gameObject.SetActive(false), (bullet) => bullet.gameObject.SetActive(true), _initialAmount);
        //_bulletPool = new PoolGeneral<MoveEnemy>(() => CreateBullet(), (bullet) => bullet.gameObject.SetActive(false), (bullet) => bullet.gameObject.SetActive(true), _initialAmount);
    }
    
    private SkeletonEnemyModel CreateBullet() 
    {
        SkeletonEnemyModel p;
        p = Instantiate(_prefab);
        //DontDestroyOnLoad(p);
        return p;
    }
    public void ReturnObj(SkeletonEnemyModel obj)
    {
        _bulletPool.ReturnObj(obj);
    }
    public SkeletonEnemyModel GetObj()
    {
        return _bulletPool.GetObject();
    }
}
