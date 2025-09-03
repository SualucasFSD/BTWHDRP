using System;
using System.Collections.Generic;
using System.Linq;
public class PoolEnemy //<T> where T : Entity
{/*
    private Func<Entity> _factoryMethod<T>() where T:Entity;
    private Action<Entity> _turnOff;
    private Action<Entity> _turnOn;

    private List<Entity> _poolStock;
    public PoolEnemy(Func<Entity> factoryMethod, Action<Entity> turnOff, Action<Entity> turnOn, int _stockInicial)
    {
        _factoryMethod = factoryMethod;
        _turnOff = turnOff;
        _turnOn = turnOn;
        _poolStock = new List<Entity>();
        for (int i = 0; i < _stockInicial; i++)
        {
            Entity obj = _factoryMethod();
            _turnOff(obj);
            _poolStock.Add(obj);
        }
    }
    public Entity GetObject<T>() where T : Entity
    {
        Entity result;
        var _stock= _poolStock.OfType<T>().ToList();
        if (_stock.Count > 0)
        {
            result = _stock[0];
            _poolStock.Remove(_stock[0]);
            //_poolStock.RemoveAt(0);
        }
        else
        {
            result = _factoryMethod();
        }
        _turnOn(result);
        return result;
    }
    public void ReturnObj(Entity obj)
    {
        _turnOff(obj);
        _poolStock.Add(obj);
    }*/
}
