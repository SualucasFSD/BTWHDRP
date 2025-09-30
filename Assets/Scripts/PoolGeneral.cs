using System;
using System.Collections.Generic;

public class PoolGeneral<T>
{
    
    private Func<T> _factoryMethod;
    private Action<T> _turnOff;
    private Action<T> _turnOn;

    private List<T> _poolStock;
    //private Dictionary<EnemyCatalogue, List<T>> _poolStockCategory;
    public PoolGeneral(Func<T> factoryMethod, Action<T> turnOff, Action<T> turnOn,int _stockInicial)
    {
        _factoryMethod = factoryMethod;
        _turnOff = turnOff;
        _turnOn = turnOn;
        _poolStock = new List<T>();
        for(int i=0; i<_stockInicial; i++)
        {
            T obj =_factoryMethod();
            _turnOff(obj);
            _poolStock.Add(obj);
        }
    }
    public T GetObject()
    {
        T result;
        if(_poolStock.Count > 0 )
        {
            result = _poolStock[0];
            _poolStock.RemoveAt(0);
        }
        else
        {
            result=_factoryMethod();
        }
        _turnOn(result);
        return result;
    }
    public void ReturnObj(T obj)
    {
        _turnOff(obj);
        //_poolStockCategory[p].Add(obj);
        _poolStock.Add(obj);
    }
}
