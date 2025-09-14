using System;
using UnityEngine;
using System.Linq;
using System.Collections.Generic;

public class CameraManager : MonoBehaviour
{
    public static CameraManager Instance;
    [Header("Target")]
    public Transform _target;
    [SerializeField] private Transform _lookTarget;
    [SerializeField] private PjModel _pjModel;
    public bool _focusing = false;
    [Header("Settings")]
    [SerializeField][Range(5, 50)] private float _distanceLocked = 5f;
    [SerializeField][Range(5, 10)] private float _distance = 5f;
    [SerializeField][Range(0.1f, 10)] private float _sensitivity = 3f;
    [SerializeField][Range(0,0.5f)] private float _smoothTime = 0.15f;
    [SerializeField][Range(20,50)]private float _zoomFov = 30f;
    [SerializeField] private LayerMask _collisionMask;
    [SerializeField] private LayerMask _lockeableLayer;
    [Header("Lock-on Camera Settings")]
    [SerializeField] private float _lockSmooth = 15f;
    [SerializeField] private float _lockCamDistance = 6f;
    [SerializeField] private float _lockHeight = 1.5f;
    [Header("Internals")]
    [SerializeField] private float _minPitch = -30f;
    [SerializeField] private float _maxPitch = 60f;
    [SerializeField][Range(0.3f,1f)] private float _camRadius=0.3f;
    [SerializeField][Range(0.5f, 1)] private float _minDistance;
    private float _yaw;
    private float _pitch;
    private Vector3 _currentVelocity;
    private Vector3 _desiredPosition;
    private Camera _cam;
    private float _defaultFov;
    private bool _zoomed = false;
    private Action _falseUpdate = delegate { };
    [SerializeField] private List<GameObject> _targets;
    private float _minDistanceLocked;
    private float _visionAngleTarget;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
    }
    void Start()
    {
        if (_cam == null)
        {
            _cam = Camera.main;
        }

        _defaultFov = _cam.fieldOfView;

        Vector3 euler = transform.eulerAngles;
        _yaw = euler.y;
        _pitch = euler.x;

        EventManager.Suscribe(EventManager.KindOfEvent.OnChangeTarget, ChangeTarget);
        EventManager.Suscribe(EventManager.KindOfEvent.OnLockCamera, ChangeLookMode);
        EventManager.Suscribe(EventManager.KindOfEvent.ChangeSensibilitieMouse, UpdateMouseSens);
        EventManager.Suscribe(EventManager.KindOfEvent.OnEnemyKilled, OnEnemyKilledChangeTarget);
        EventManager.Suscribe(EventManager.KindOfEvent.CameraSmooth, SmoothPercent);
        EventManager.Suscribe(EventManager.KindOfEvent.CameraDistance, DistancePercent);
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        _pjModel.OnAim += HandleRotation;
        _falseUpdate += RotateCameraLateUpdate;
    }

    private void SmoothPercent(params object[] p)
    {
        _smoothTime = (float)p[0];
    }
    private void DistancePercent(params object[] p)
    {
        _distance = (float)p[0];
    }
    void LateUpdate()
    {
        _falseUpdate();
        _cam.transform.position = transform.position;
        _cam.transform.rotation = transform.rotation;
    }

    private void RotateCameraLateUpdate()
    {
        if (_target == null) return;

        HandleCollision();
        HandleFollow();
        //HandleZoom();
    }

    private void HandleRotation(float mouseX, float mouseY)
    {
        if(_focusing)
        {
            return;
        }
        mouseX *= _sensitivity;
        mouseY *= _sensitivity;
        _yaw += mouseX;
        _pitch -= mouseY;
        _pitch = Mathf.Clamp(_pitch, _minPitch, _maxPitch);
    }

    private void HandleCollision()
    {
        if(_target==null)
        {
            return;
        }
        Quaternion rotation = Quaternion.Euler(_pitch, _yaw, 0);
        Vector3 desiredDir = -(rotation * Vector3.forward);
        Vector3 targetPos = _target.position;

        float finalDistance = _distance;

        if (Physics.SphereCast(_target.position, _camRadius, desiredDir, out RaycastHit hit, _distance, _collisionMask))
        {
            finalDistance = Mathf.Max(_minDistance, hit.distance - 0.1f);
        }

        Vector3 desiredPos = targetPos + desiredDir * finalDistance;
        transform.position = Vector3.SmoothDamp(transform.position,desiredPos,ref _currentVelocity,_smoothTime);

        _desiredPosition = desiredPos;
    }

    private void HandleFollow()
    {
        transform.LookAt(_target.position);
    }

    private void HandleZoom()
    {
        if (_focusing) return;

        if (Input.GetMouseButtonDown(1))
            _zoomed = !_zoomed;

        float targetFOV = _zoomed ? _zoomFov : _defaultFov;
        _cam.fieldOfView = Mathf.Lerp(_cam.fieldOfView, targetFOV, Time.deltaTime * 5f);
    }

    public void UpdateMouseSens(params object[] obj)
    {
        _sensitivity = (float)obj[0];
    }

    public void ChangeLookMode(params object[] p)
    {
        if (_focusing)
        {
            _falseUpdate += RotateCameraLateUpdate;
            _falseUpdate -= UpdateTarget;
            _focusing = false;
            return;
        }
        _targets = GameManager.Instance.RefreshEnemy(Entity.KindOfEntity.Allies).Select(x => x.gameObject).ToList();
        if (_targets.Count <= 0) return;

        _falseUpdate -= RotateCameraLateUpdate;
        _falseUpdate += UpdateTarget;

        _lookTarget = null;
        _minDistanceLocked = Mathf.Infinity;

        foreach (GameObject c in _targets)
        {
            float dist = Vector3.Distance(c.transform.position, _pjModel.transform.position);
            if (dist>=_distanceLocked)
            {
                continue;
            }
            if (!GameManager.Instance.LineOfSight(transform.position, c.transform.position))
            {
                continue;
            }
            _visionAngleTarget = Vector3.Dot(transform.forward, (c.transform.position - transform.position).normalized);

            if (_visionAngleTarget <= 0.75f)
            {
                continue;
            }

            if (dist < _minDistanceLocked)
            {
                _focusing = true;
                _lookTarget = c.transform;
                _minDistanceLocked = dist;
            }
        }
    }

    private void UpdateTarget()
    {
        if (_target==null)
        {
            return;
        }
        if (_lookTarget == null)
        {
            _falseUpdate += RotateCameraLateUpdate;
            _falseUpdate -= UpdateTarget;
            _focusing = false;
            Vector3 euler = transform.eulerAngles;
            _yaw = euler.y;
            _pitch = euler.x;
            return;
        }

        if (Vector3.Distance(_lookTarget.position, _pjModel.transform.position) >= _distanceLocked)
        {
            _lookTarget = null;
            _falseUpdate += RotateCameraLateUpdate;
            _falseUpdate -= UpdateTarget;
            _focusing = false;
            return;
        }

        Vector3 dirToEnemy = (_pjModel.transform.position - _lookTarget.position).normalized;

        Vector3 desiredPos = _pjModel.transform.position+ Vector3.up * _lockHeight+ dirToEnemy * _lockCamDistance;

        if (Physics.Linecast(_pjModel.transform.position + Vector3.up * _lockHeight, desiredPos, out RaycastHit hit, _collisionMask))
        {
            desiredPos = hit.point + hit.normal * 0.2f;
        }
        transform.position = Vector3.Lerp(transform.position, desiredPos, Time.deltaTime * _lockSmooth);

        Vector3 lookDir = (_lookTarget.position - transform.position).normalized;
        Quaternion targetRot = Quaternion.LookRotation(lookDir, Vector3.up);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, Time.deltaTime * _lockSmooth);

        _pjModel.RotatePlayer(transform.eulerAngles.y, 0);
    }

    private void ChangeTarget(params object[] p)
    {
        List<GameObject> Targets = GameManager.Instance.RefreshEnemy(Entity.KindOfEntity.Allies).Select(x => x.gameObject).ToList();
        if (_lookTarget == null || _targets.Count <= 1)
            return;

        int dir = (int)p[0];
        Vector3 playerPos = _pjModel.transform.position;
        Vector3 forward = (_lookTarget.position - playerPos).normalized;

        Transform bestTarget = null;
        float bestAngle = dir == 1 ? 360f : -360f;

        foreach (GameObject t in Targets)
        {
            if (t == null || t.transform == _lookTarget) continue;

            Vector3 toTarget = (t.transform.position - playerPos).normalized;
            float angle = Vector3.SignedAngle(forward, toTarget, Vector3.up);

            if (Vector3.Distance(playerPos, t.transform.position) <= _distanceLocked &&
                GameManager.Instance.LineOfSight(playerPos, t.transform.position))
            {
                if (dir == 1 && angle > 0 && angle < bestAngle)
                {
                    bestAngle = angle;
                    bestTarget = t.transform;
                }
                else if (dir == -1 && angle < 0 && angle > bestAngle)
                {
                    bestAngle = angle;
                    bestTarget = t.transform;
                }
            }
        }

        if (bestTarget != null)
        {
            _lookTarget = bestTarget;
        }
    }

    private void ChangeLookModeWithOutLineOfSingh()
    {
        if (_focusing)
        {
            _falseUpdate += RotateCameraLateUpdate;
            _falseUpdate -= UpdateTarget;
            _focusing = false;
            return;
        }

        _targets = GameManager.Instance.RefreshEnemy(Entity.KindOfEntity.Allies).Select(x => x.gameObject).ToList();
        if (_targets.Count <= 0) return;

        _falseUpdate -= RotateCameraLateUpdate;
        _falseUpdate += UpdateTarget;

        _lookTarget = null;
        _minDistanceLocked = Mathf.Infinity;

        foreach (GameObject c in _targets)
        {
            float dist = Vector3.Distance(c.transform.position, _pjModel.transform.position);
            if (dist >= _distanceLocked)
            {
                continue;
            }
            if (!GameManager.Instance.LineOfSight(transform.position, c.transform.position))
            {
                continue;
            }
            if (dist < _minDistanceLocked)
            {
                _focusing = true;
                _lookTarget = c.transform;
                _minDistanceLocked = dist;
            }
        }
    }

    private void OnEnemyKilledChangeTarget(params object[] obj)
    {
        if ((GameObject)obj[0] != null)
        {
            if (_targets.Contains(obj[0]))
            {
                ChangeLookModeWithOutLineOfSingh();
                ChangeLookModeWithOutLineOfSingh();
            }
        }
    }

    private void OnDestroy()
    {
        EventManager.Unscribe(EventManager.KindOfEvent.ChangeSensibilitieMouse, UpdateMouseSens);
        EventManager.Unscribe(EventManager.KindOfEvent.OnEnemyKilled, OnEnemyKilledChangeTarget);
        EventManager.Unscribe(EventManager.KindOfEvent.OnChangeTarget, ChangeTarget);
        EventManager.Unscribe(EventManager.KindOfEvent.OnLockCamera, ChangeLookMode);
        EventManager.Unscribe(EventManager.KindOfEvent.CameraSmooth, SmoothPercent);
        EventManager.Unscribe(EventManager.KindOfEvent.CameraDistance, DistancePercent);
    }
}
