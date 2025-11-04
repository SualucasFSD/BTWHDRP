using System.Collections.Generic;
using UnityEngine;

public class PowerEffectsManager : MonoBehaviour
{
    private struct PoisonData
    {
        public Entity target;
        public float remainingTime;
        public float tickTimer;
        public float tickRate;
        public float damagePerTick;

        public PoisonData(Entity target, float duration, float tickRate, float damage)
        {
            this.target = target;
            remainingTime = duration;
            this.tickRate = tickRate;
            damagePerTick = damage;
            tickTimer = 0;
        }
    }

    private readonly List<PoisonData> _activePoisons = new List<PoisonData>();

    private void Start()
    {
        EventManager.Suscribe(EventManager.KindOfEvent.GetVenemous, ApplyPoison);
        EventManager.Suscribe(EventManager.KindOfEvent.PopVenemous, RemovePoison);
    }
    private void Update()
    {
        if (GameManager.Instance.IsPaused)
            return;

        for (int i = _activePoisons.Count - 1; i >= 0; i--)
        {
            PoisonData p = _activePoisons[i];

            if (p.target == null)
            {
                _activePoisons.RemoveAt(i);
                continue;
            }

            p.remainingTime -= Time.deltaTime;
            p.tickTimer += Time.deltaTime;

            if (p.tickTimer >= p.tickRate)
            {
                p.tickTimer = 0;
                if (p.target.TryGetComponent<Idamageable>(out var dmg))
                {
                    dmg.TakeDamage(p.damagePerTick, 0, Vector3.zero, false, false);
                }
            }

            if (p.remainingTime <= 0)
            {
                _activePoisons.RemoveAt(i);
                continue;
            }

            _activePoisons[i] = p;
        }
    }

    //public void ApplyPoison(Entity target, float duration, float tickRate, float damage)
    public void ApplyPoison(params object[] Obj)
    {
        for (int i = 0; i < _activePoisons.Count; i++)
        {
            PoisonData p = _activePoisons[i];
            if (p.target == (Entity)Obj[0])
            {
                p.remainingTime = (float)Obj[1];
                _activePoisons[i] = p;
                return;
            }
        }

        _activePoisons.Add(new PoisonData((Entity)Obj[0], (float)Obj[1], (float)Obj[2], (float)Obj[3]));
    }

    public void RemovePoison(params object[] Obj)
    {
        _activePoisons.RemoveAll(p => p.target == (Entity)Obj[0]);
    }
    private void OnDestroy()
    {
        EventManager.Unscribe(EventManager.KindOfEvent.GetVenemous, ApplyPoison);
        EventManager.Unscribe(EventManager.KindOfEvent.PopVenemous, RemovePoison);
    }
}