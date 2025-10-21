using System;
using UnityEngine;

public class LizardPower : MonoBehaviour, IPjPower
{
    [SerializeField] private PjModel _model;
    [SerializeField] private LayerMask _enemyLayer;
    [SerializeField] private float _radius = 10f;
    [SerializeField] private float _pauseDuration = 1.5f;
    [SerializeField] private float _cooldown = 10f;

    private float _powerTimer = 0f;
    [SerializeField] private int _howMany;


    private void Start()
    {
        if (_model == null)
        {
            _model = GetComponentInParent<PjModel>();
        }
        _model.AddPower(EnemyCatalogue.Lizard, Tuple.Create(_howMany, GetComponent<IPjPower>()));
        gameObject.SetActive(false);

    }

    private void FalseUpdate()
    {
        _powerTimer += Time.deltaTime;

        if (_powerTimer >= _cooldown)
        {
            _powerTimer = 0;
            ActivateLizardPower();
        }
    }

    private void ActivateLizardPower()
    {

        Collider[] colliders = Physics.OverlapSphere(transform.position, _radius, _enemyLayer);

        foreach (Collider col in colliders)
        {
            Entity entity = col.GetComponent<Entity>();

            if (entity == null || entity.Kind == _model.Kind)
            {
                continue;
            }
            EventManager.Ejecute(EventManager.KindOfEvent.PauseOneEnemy, col.gameObject,_pauseDuration);
        }
        print("Lizard");
    }

    public void Active()
    {
        gameObject.SetActive(true);
        _model.EjecutePower += FalseUpdate;
    }
}