using System;
using System.Collections.Generic;
using UnityEngine;


public class EnemyPool<T> where T : Entity
{
    private Func<EnemyCatalogue, T> _factoryMethod;
    private Action<T> _turnOff;
    private Action<T> _turnOn;

    private Dictionary<EnemyCatalogue, List<T>> _poolStockCategory = new Dictionary<EnemyCatalogue, List<T>>();

    public EnemyPool(Func<EnemyCatalogue, T> factoryMethod, Action<T> turnOff, Action<T> turnOn, IEnumerable<EnemyCatalogue> types, int initialStock/*, Dictionary<EnemyCatalogue, List<T>> _poolCategory*/)
    {
        _factoryMethod = factoryMethod;
        _turnOff = turnOff;
        _turnOn = turnOn;

        foreach (var type in types)
        {
            _poolStockCategory[type] = new List<T>();
            for (int i = 0; i < initialStock; i++)
            {
                T obj = _factoryMethod(type);
                _turnOff(obj);
                _poolStockCategory[type].Add(obj);
            }
        }
    }

    public T GetObject(EnemyCatalogue type, Vector3 position)
    {
        T result;

        if (!_poolStockCategory.ContainsKey(type))
        {
            _poolStockCategory[type] = new List<T>();
        }
        if (_poolStockCategory[type].Count > 0)
        {
            result = _poolStockCategory[type][0];
            _poolStockCategory[type].RemoveAt(0);
        }
        else
        {
            result = _factoryMethod(type);
        }

        result.transform.position = position;

        _turnOn(result);
        return result;
    }

    public void ReturnObj(EnemyCatalogue type, T obj)
    {
        _turnOff(obj);

        if (!_poolStockCategory.ContainsKey(type))
            _poolStockCategory[type] = new List<T>();

        _poolStockCategory[type].Add(obj);
    }
}