using UnityEngine;
using System.Collections;
using System.Collections.Generic;
public class BossModel : MonoBehaviour
{
    [Header("Spawn Settings")]
    [SerializeField] private GameObject _venemousBulletPrefab;
    [SerializeField] private MagueBullet _bulletPrefab;
    [SerializeField] private int _bulletCount = 12;
    [SerializeField] private float _radius = 5f;
    [SerializeField] private float _yOffset = -1;
    [SerializeField] private Transform _spitPoint;
    [SerializeField] private LayerMask _obstacleMask;

    //Privada y Opcional
    private float _spitTimer;
    //Private
    private float _predictionTime = 0.5f;
    private Vector3 _lastPos;
    private Vector3 _tgPos;
    private List<MagueBullet> _bullets=new List<MagueBullet>();

    private void Start()
    {
        EventManager.Suscribe(EventManager.KindOfEvent.OnPjChangePosition, TakePjPosition);
    }
    private void Update()
    {
        /*_spitTimer += Time.deltaTime;
        if(_spitTimer>0.2f)
        {
            _spitTimer = 0;
            SpitVenemousParabolicBullets();
        }*/
    }
    private void TakePjPosition(params object[] p)
    {
        Vector3 playerPos = (Vector3)p[0];

        Vector3 playerVel = (playerPos - _lastPos) / Time.deltaTime;
        _lastPos = playerPos;

        Vector3 predictedPos = playerPos;
        if (playerVel.magnitude > 0.1f)
        {
            predictedPos += playerVel.normalized * playerVel.magnitude * _predictionTime;
        }
        Vector3 shootOrigin = transform.position;
        Vector3 dir = (predictedPos - shootOrigin).normalized;
        float dist = Vector3.Distance(shootOrigin, predictedPos);

        if (Physics.Raycast(shootOrigin, dir, out RaycastHit hit, dist, _obstacleMask))
        {
            predictedPos = hit.point;
        }

        _tgPos = predictedPos;
    }
    #region Powers
    public void SpitVenemousParabolicBullets()
    {
        if(_tgPos!=Vector3.zero)
        {
            GameObject bullet = Instantiate(_venemousBulletPrefab);
            bullet.transform.position = _spitPoint.position;
            GameManager.Instance.LaunchProjectile(bullet, _tgPos,45);
        }
    }
    public void SpawnCircularBullets()
    {
        if (_bulletPrefab == null) return;

        for (int i = 0; i < _bulletCount; i++)
        {
            float angle = i * (360f / _bulletCount);
            Vector3 dir = new Vector3(Mathf.Cos(angle * Mathf.Deg2Rad), 0, Mathf.Sin(angle * Mathf.Deg2Rad));
            Vector3 spawnPos = transform.position + dir * _radius;
            Quaternion rot = Quaternion.LookRotation(dir);

            MagueBullet bullet = Instantiate(_bulletPrefab, spawnPos+Vector3.up*_yOffset, rot);
            _bullets.Add(bullet);
        }
        foreach(MagueBullet b in _bullets)
        {
            b.Kind=Entity.KindOfEntity.Enemy;
            b.Fire=true;
        }

    }
    #endregion
    private void OnDestroy()
    {
       EventManager.Unscribe(EventManager.KindOfEvent.OnPjChangePosition, TakePjPosition);
    }
}
