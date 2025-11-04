using System;
using System.Collections.Generic;
using UnityEngine;

public class GenericObjectPool<T> where T : UnityEngine.Object
{
    private Func<GenericObjectType, T> _factoryMethod;
    private Action<T> _turnOff;
    private Action<T> _turnOn;

    private Dictionary<GenericObjectType, List<T>> _poolStockCategory = new Dictionary<GenericObjectType, List<T>>();

    public GenericObjectPool(
        Func<GenericObjectType, T> factoryMethod,
        Action<T> turnOff,
        Action<T> turnOn,
        IEnumerable<GenericObjectType> types,
        int initialStock)
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
                if (obj != null)
                {
                    _turnOff(obj);
                    _poolStockCategory[type].Add(obj);
                }
            }
        }
    }

    public T GetObject(GenericObjectType type, Vector3 position, Quaternion rotation)
    {
        T result;

        if (!_poolStockCategory.ContainsKey(type))
            _poolStockCategory[type] = new List<T>();

        if (_poolStockCategory[type].Count > 0)
        {
            result = _poolStockCategory[type][0];
            _poolStockCategory[type].RemoveAt(0);
        }
        else
        {
            result = _factoryMethod(type);
        }

        if (result is GameObject go)
        {
            go.transform.position = position;
            go.transform.rotation = rotation;
        }

        _turnOn(result);
        return result;
    }

    public void ReturnObj(GenericObjectType type, T obj)
    {
        _turnOff(obj);

        if (!_poolStockCategory.ContainsKey(type))
            _poolStockCategory[type] = new List<T>();
        _poolStockCategory[type].Add(obj);
    }
}
