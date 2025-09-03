using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class CameraFallow : MonoBehaviour
{
    [SerializeField] private GameObject _camera;
    [SerializeField] private PjModel _pjModel;
    //Values
    [SerializeField][Range(50,120)] private float _fov;
    [SerializeField][Range(1, 1000)] private float _mouseSens = 600;
    [SerializeField][Range(2, 10)] private float _maxDistanceCamera=3f;
    [SerializeField][Range(.125f, 2)] private float _minDistanceCamera=.25f;
    [Range(-90f, 0f)][SerializeField] private float _minAngleCam=-67f;
    [Range(0f, 90f)][SerializeField] private float _maxAngleCam=67f;
    [SerializeField] private LayerMask _noIgnoreLayer;
    private bool _isBlocked;
    private RaycastHit _camHit;
    private Ray _ray;
    private float _mouseX, _mouseY;
    private Action<float,float> _falseUpdate=delegate { };
    [SerializeField] private LayerMask _lockeableLayer;
    private float _minDistanceLocked;
    [SerializeField] private Transform _targetLocked;
    [SerializeField] private Collider[] _targets;
    private float _visionAngleTarget;
    private int _tgNum;
    private bool _onLocked=false;
    [SerializeField][Range(5,50)]private int _distanceLock;
    [SerializeField][Range(0, 10)] private float _lockRotForce;
    private void Awake()
    {
        EventManager.Suscribe(EventManager.KindOfEvent.OnEnemyKilled,OnEnemyKilledChangeTarget);
        _falseUpdate += RotateCameraUpdate;
        _pjModel=GetComponentInParent<PjModel>();
        _camera=Camera.main.gameObject;
    }
    private void Start()
    {
        EventManager.Suscribe(EventManager.KindOfEvent.OnChangeTarget,ChangeTarget);
        EventManager.Suscribe(EventManager.KindOfEvent.OnLockCamera,ChangeLookMode);
        //_pjModel.OnLockCamera += ChangeLookMode;
        //_pjModel.OnChangeLockTarget += ChangeTarget;
        _mouseX = transform.eulerAngles.y;
        _mouseY = transform.eulerAngles.x;
        Cursor.lockState= CursorLockMode.Locked;
        Cursor.visible= false;
        EventManager.Suscribe(EventManager.KindOfEvent.ChangeSensibilitieMouse,UpdateMouseSens);
    }
    private void Update()
    {
        if (_falseUpdate != null)
        {
            _falseUpdate(Input.GetAxisRaw("Mouse X"), Input.GetAxisRaw("Mouse Y"));
        }
    }
    private void FixedUpdate()
    { 
        _ray.direction=-transform.forward;
        _ray.origin=transform.position;
        _isBlocked = Physics.SphereCast(_ray, 0.5f,out _camHit,_maxDistanceCamera,_noIgnoreLayer);
    }
    private void LateUpdate()
    {
        UpdateCameraLimit();
        if(_pjModel!=null)
        {
            _pjModel.RotationPj(_mouseX, _mouseY);
        }
    }
    private void RotateCameraUpdate(float x,float y)
    {
        if (x == 0 && y == 0) return;
        if (x != 0 && GameManager.Instance.IsPaused == false)
        {
            _mouseX += x * _mouseSens * Time.deltaTime;
            if (_mouseX > 360 || _mouseX < -360)
            {
                _mouseX -= 360 * Mathf.Sign(_mouseX);
            }
        }
        if (y != 0 && GameManager.Instance.IsPaused == false)
        {
            _mouseY += y * _mouseSens * Time.deltaTime;
            _mouseY = Mathf.Clamp(_mouseY, _minAngleCam, _maxAngleCam);
        }
        transform.rotation = Quaternion.Euler(-_mouseY, _mouseX, 0f);
    }
    private void UpdateCameraLimit()
    {
        if (_isBlocked) 
        {
            Vector3 _dirTest = (_camHit.point - transform.position) + (_camHit.normal * 0.2f);
            if (_dirTest.sqrMagnitude <= Mathf.Pow(_minDistanceCamera, 2))
            {
                _camera.transform.position = transform.position -transform.forward * _minDistanceCamera;
            }
            else
            {
                _camera.transform.position = transform.position+_dirTest;
            }
        }
        else 
        {
            _camera.transform.position = transform.position-transform.forward * _maxDistanceCamera;
        }
        _camera.transform.LookAt(transform.position);
    }

    public void ChangeLookMode(params object[] p)
    {
        if(_onLocked)
        {
            _falseUpdate += RotateCameraUpdate;
            _falseUpdate -= UpdateTarget;
            _onLocked = false;
            return;
        }
        _targets = Physics.OverlapSphere(_pjModel.transform.position,100f,_lockeableLayer);
        if (_targets.Length <= 0) { return;}
        else
        {
            _falseUpdate -= RotateCameraUpdate;
            _falseUpdate += UpdateTarget;
        }
        _targetLocked = null;
        _minDistanceLocked=Mathf.Infinity;

        foreach(Collider c in _targets)
        {
            if(!GameManager.Instance.LineOfSight(transform.position,c.transform.position))
            {
                continue;
            }

            _visionAngleTarget = Vector3.Dot(transform.forward, (c.transform.position - transform.position).normalized);
            if (_visionAngleTarget<=0.75f)
            {
                continue;
            }
            
            if (Vector3.Distance(c.transform.position, _pjModel.transform.position) < _minDistanceLocked)
            {
                _onLocked = true;
                _targetLocked = c.transform;
                _minDistanceLocked = Vector3.Distance(c.transform.position, _pjModel.transform.position);
            }
        }
    }
    private void ChangeLookModeWithOutLineOfSingh()
    {
        if (_onLocked)
        {
            _falseUpdate += RotateCameraUpdate;
            _falseUpdate -= UpdateTarget;
            _onLocked = false;
            return;
        }
        _targets = Physics.OverlapSphere(_pjModel.transform.position, 100f, _lockeableLayer);
        if (_targets.Length <= 0) { return; }
        else
        {
            _falseUpdate -= RotateCameraUpdate;
            _falseUpdate += UpdateTarget;
        }
        _targetLocked = null;
        _minDistanceLocked = Mathf.Infinity;

        foreach (Collider c in _targets)
        {
            if (!GameManager.Instance.LineOfSight(transform.position, c.transform.position))
            {
                continue;
            }

            if (Vector3.Distance(c.transform.position, _pjModel.transform.position) < _minDistanceLocked)
            {
                _onLocked = true;
                _targetLocked = c.transform;
                _minDistanceLocked = Vector3.Distance(c.transform.position, _pjModel.transform.position);
            }
        }
    }
    private void UpdateTarget(float X, float Y)
    {
        if (_targetLocked==null)
        {
            _falseUpdate += RotateCameraUpdate;
            _falseUpdate -= UpdateTarget;
            _onLocked = false;
            return;
        }
        if (Vector3.Distance(_targetLocked.position, transform.position) > _distanceLock)
        {
            _targetLocked = null;
            _falseUpdate += RotateCameraUpdate;
            _falseUpdate -= UpdateTarget;
            _onLocked = false;
            return;
        }
        transform.rotation=Quaternion.Slerp(_pjModel.transform.rotation, Quaternion.LookRotation(_targetLocked.position - _pjModel.transform.position), _lockRotForce);
        _mouseX =transform.eulerAngles.y;
        if (_mouseX > 360 || _mouseX < -360)
        {
            _mouseX -= 360 * Mathf.Sign(_mouseX);
        }
    }
    private void ChangeTarget(params object[] p)
    {
        if (_targetLocked == null || _targets.Length <= 1)
            return;

        int dir = (int)p[0];

        Vector3 playerPos = _pjModel.transform.position;
        Vector3 forward = (_targetLocked.position - playerPos).normalized;

        Transform bestTarget = null;
        float bestAngle = dir == 1 ? 360f : -360f;

        foreach (var t in _targets)
        {
            if (t == null || t.transform == _targetLocked) continue;

            Vector3 toTarget = (t.transform.position - playerPos).normalized;

            float angle = Vector3.SignedAngle(forward, toTarget, Vector3.up);

            if (Vector3.Distance(playerPos, t.transform.position) <= _distanceLock &&
                GameManager.Instance.LineOfSight(playerPos, t.transform.position))
            {
                if (dir == 1)
                {
                    if (angle > 0 && angle < bestAngle)
                    {
                        bestAngle = angle;
                        bestTarget = t.transform;
                    }
                }
                else if (dir == -1)
                {
                    if (angle < 0 && angle > bestAngle)
                    {
                        bestAngle = angle;
                        bestTarget = t.transform;
                    }
                }
            }
        }
        if (bestTarget != null)
            _targetLocked = bestTarget;
    }
    public void UpdateMouseSens(params object[] obj)
    {
        _mouseSens = (float)obj[0];
    }
    private void OnEnemyKilledChangeTarget(params object[] obj)
    {
        if ((Collider)obj[0]!=null)
        {
            if (_targets.ToList().Contains(obj[0])) { ChangeLookModeWithOutLineOfSingh(); ChangeLookModeWithOutLineOfSingh(); }
        }
    }
    private void OnDestroy()
    {
        EventManager.Unscribe(EventManager.KindOfEvent.ChangeSensibilitieMouse, UpdateMouseSens);
        EventManager.Unscribe(EventManager.KindOfEvent.OnEnemyKilled, OnEnemyKilledChangeTarget);
        EventManager.Suscribe(EventManager.KindOfEvent.OnChangeTarget, ChangeTarget);
        EventManager.Suscribe(EventManager.KindOfEvent.OnLockCamera, ChangeLookMode);
    }
}
