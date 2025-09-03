using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MagueFactory : MonoBehaviour
{
    public static MagueFactory instance { get; private set; }

    private PoolGeneral<MagueEnemyModel> _bulletPool;

    [SerializeField] private MagueEnemyModel _prefab;
    [SerializeField] private int _initialAmount;
    [SerializeField] private Transform _parent;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            //DontDestroyOnLoad(gameObject);
        }
        _bulletPool = new PoolGeneral<MagueEnemyModel>(() => Instantiate(_prefab, _parent), (bullet) => bullet.gameObject.SetActive(false), (bullet) => bullet.gameObject.SetActive(true), _initialAmount);
        //_bulletPool = new PoolGeneral<MoveEnemy>(() => CreateBullet(), (bullet) => bullet.gameObject.SetActive(false), (bullet) => bullet.gameObject.SetActive(true), _initialAmount);
    }
    private MagueEnemyModel CreateBullet()
    {
        MagueEnemyModel p;
        p = Instantiate(_prefab);
        //DontDestroyOnLoad(p);
        return p;
    }
    public void ReturnObj(MagueEnemyModel obj)
    {
        _bulletPool.ReturnObj(obj);
    }
    public MagueEnemyModel GetObj()
    {
        return _bulletPool.GetObject();
    }
}
