using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class EnemyFactory : MonoBehaviour
{/*
   public static EnemyFactory instance { get; private set; }

    private PoolEnemy _bulletPool;

    [SerializeField] private List<Entity> _prefab;
    [SerializeField] private int _initialAmount;
    [SerializeField] private Transform _parent;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            //DontDestroyOnLoad(gameObject);
        }
        _bulletPool = new PoolEnemy(()=>CreateBullet<T> (), (bullet) => bullet.gameObject.SetActive(false), (bullet) => bullet.gameObject.SetActive(true), _initialAmount);
        //_bulletPool = new PoolGeneral<MoveEnemy>(() => CreateBullet(), (bullet) => bullet.gameObject.SetActive(false), (bullet) => bullet.gameObject.SetActive(true), _initialAmount);
    }
    private Entity CreateBullet<T>() where T : Entity
    {
        Entity p;
        p = Instantiate(_prefab);
        DontDestroyOnLoad(p);
        return p;
    }
    public void ReturnObj(Entity obj)
    {
        _bulletPool.ReturnObj(obj);
    }
    public Entity GetObj<T>() where T : Entity
    {
        return _bulletPool.GetObject<T>();
    }*/
}
