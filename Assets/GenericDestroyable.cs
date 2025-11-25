using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GenericDestroyable : MonoBehaviour, Idamageable
{
    [SerializeField] private MeshRenderer[] _mesh;
    [SerializeField] private MeshRenderer _fakeMesh;
    [SerializeField] private float _maxLife = 100f;
    [SerializeField] private ParticleSystem _damageEffect;
    [SerializeField] private bool _isDestroyable;
    [SerializeField] private bool _hitDestroyableEnemy = false;
    [SerializeField] Collider _fixedCol;
    [SerializeField] bool _meshStay = false;
    private float _life;
    private int _currentStateIndex = -1;

    private void Start()
    {
        _life = _maxLife;
        //UpdateMeshState();
    }

    public void TakeDamage(float dmg, float stunt, Vector3 pushDirection, bool downHit = false, bool airHit = false, bool isStunDamage = false, float pushForce = 1000)
    {
        if (_life <= 0)
        { 
            return;
        }
        _life -= dmg;
        _life = Mathf.Max(0, _life);

        if(_fixedCol != null) 
            _fixedCol.enabled = false;

        UpdateMeshState();

        if (_damageEffect != null)
        {
            _damageEffect.Play();
        }

        if (_life <= 0&&_isDestroyable)
        {
            // destruir el objeto
            Invoke(nameof(InvokeDestroyable),5f);
        }
    }
    private void InvokeDestroyable()
    {
        Destroy(gameObject);
    }
    public void TakeHealt(float amount)
    {
        /*_life += amount;
        _life = Mathf.Min(_maxLife, _life);
        UpdateMeshState();*/
    }

    private void OnTriggerEnter(Collider other)
    {
        if (_hitDestroyableEnemy)
        {
            if (other.gameObject.TryGetComponent<Entity>(out var compo))
            {
                if (compo.Kind == Entity.KindOfEntity.Enemy)
                {
                    TakeDamage(100, 0, Vector3.zero);
                    return;
                }
            }
        }
        else
        {
            TakeDamage(100, 0, Vector3.zero);
        }
    }

    private void UpdateMeshState()
    {
        if (_mesh == null || _mesh.Length == 0)
        {
            return;
        }

        if(_fakeMesh != null && _fakeMesh.enabled == true)
            _fakeMesh.enabled = false;

        int targetStateIndex = Mathf.FloorToInt((1 - _life / _maxLife) * _mesh.Length);

        targetStateIndex = Mathf.Clamp(targetStateIndex, 0, _mesh.Length - 1);

        if (targetStateIndex == _currentStateIndex)
        {
            return;
        }
        _currentStateIndex = targetStateIndex;

        for (int i = 0; i < _mesh.Length; i++)
        {
            if (_meshStay)
            {
                _mesh[i].enabled = true;
                return;
            }
            _mesh[i].enabled = false;
        }
    }
}
