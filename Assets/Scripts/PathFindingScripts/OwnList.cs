using System;
using System.Collections.Generic;
using UnityEngine;

public class OwnList<T> 
{
    //List<T> LowCostCompareList;
    Dictionary<T,float> _elements= new Dictionary<T, float>();
    public void Add(T element,float Cost)
    {
        if (!_elements.ContainsKey(element))
        {
            _elements.Add(element, Cost);
        }
        else
        {
            _elements[element] = Cost;
        }
    }
    public T ObtainAndRemove()
    {
        T min = default;
        float minCost = Mathf.Infinity;
        foreach (var item in _elements)
      {
            if (item.Value < minCost)
            {
                min = item.Key;
                minCost = item.Value;
            }
      }
        _elements.Remove(min);
        return min;
    }
    public int Count {  get { return _elements.Count; } }
}
