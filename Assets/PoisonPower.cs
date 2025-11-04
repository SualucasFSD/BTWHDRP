using System;
using UnityEngine;

public class PoisonPower : MonoBehaviour, IPjPower
{
    [SerializeField] private PjModel _model;
    [SerializeField] private LayerMask _enemyLayer;
    [SerializeField] private float _radius = 10f;
    [SerializeField] private float _cooldown = 10f;
    [SerializeField] private float _poisonDuration = 5f;
    [SerializeField] private float _tickRate = 1f;
    [SerializeField] private float _damagePerTick = 5f;
    [SerializeField] private int _howMany;

    private float _powerTimer = 0f;

    private void Start()
    {
        if (_model == null)
            _model = GetComponentInParent<PjModel>();

        _model.AddPower(EnemyCatalogue.VenomLancer, Tuple.Create(_howMany, GetComponent<IPjPower>()));
        gameObject.SetActive(false);
    }

    private void FalseUpdate()
    {
        if (GameManager.Instance.IsPaused)
            return;

        _powerTimer += Time.deltaTime;

        if (_powerTimer >= _cooldown)
        {
            _powerTimer = 0;
            ActivatePoisonPulse();
        }
    }

    private void ActivatePoisonPulse()
    {
        Collider[] colliders = Physics.OverlapSphere(transform.position, _radius, _enemyLayer);

        foreach (Collider col in colliders)
        {
            Entity entity = col.GetComponent<Entity>();
            if (entity == null || entity.Kind == _model.Kind)
                continue;

            entity.GetVenemous(_poisonDuration, _tickRate, _damagePerTick);
            //PowerEffectsManager.Instance.ApplyPoison(entity, _poisonDuration, _tickRate, _damagePerTick);
        }
    }

    public void Active()
    {
        gameObject.SetActive(true);
        _model.EjecutePower += FalseUpdate;
    }
}