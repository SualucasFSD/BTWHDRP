using UnityEngine;

public class Interact : MonoBehaviour
{
    public bool _isActive=false;
    public GameObject[] _interactObj;
    public Vector3 _offset;
    public virtual void Interacting()
    {
        if (_isActive)
        {
            Desactivate();
        }
        else
        {
            Activate();
        }
    }
    public virtual void Activate()
    {
        _isActive = true;
        foreach (GameObject obj in _interactObj)
        {
            obj.SetActive(false);
        }
    }
    public virtual void Desactivate()
    {
        _isActive = false;
        foreach (GameObject obj in _interactObj)
        {
            obj.SetActive(true);
        }
    }
}
