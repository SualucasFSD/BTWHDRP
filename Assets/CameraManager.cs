using System;
using UnityEngine;
using System.Linq;

public class CameraManager : MonoBehaviour
{
    [Header("Target")]
    [SerializeField] private Transform _target;
    [SerializeField] private Transform _lookTarget;
    [SerializeField] private PjModel _pjModel;

    [Header("Settings")]
    [SerializeField][Range(5, 50)] private float _distanceLocked = 5f;
    [SerializeField][Range(5, 10)] private float _distance = 5f;
    [SerializeField][Range(0.1f, 10)] private float _sensitivity = 3f;
    [SerializeField][Range(0, 10)] private float _lockRotForce = 5f;
    [SerializeField][Range(0,0.5f)] private float _smoothTime = 0.15f;
    [SerializeField][Range(20,50)]private float _zoomFov = 30f;
    [SerializeField] private LayerMask _collisionMask;
    [SerializeField] private LayerMask _lockeableLayer;

    [Header("Internals")]
    [SerializeField] private float _minPitch = -30f;
    [SerializeField] private float _maxPitch = 60f;

    private float _yaw;
    private float _pitch;
    private Vector3 _currentVelocity;
    private Vector3 _desiredPosition;
    private Camera _cam;
    private float _defaultFov;
    private bool _zoomed = false;
    private bool _focusing = false;
    private Action _falseUpdate = delegate { };
    [SerializeField] private Collider[] _targets;
    private float _minDistanceLocked;
    private float _visionAngleTarget;

    void Start()
    {
        if (_cam == null)
        {
            _cam = Camera.main;
        }

        if (_pjModel == null)
        {
            _pjModel = GetComponentInParent<PjModel>();
        }
        _pjModel.Camera = _cam.transform;
        _defaultFov = _cam.fieldOfView;

        Vector3 euler = transform.eulerAngles;
        _yaw = euler.y;
        _pitch = euler.x;

        EventManager.Suscribe(EventManager.KindOfEvent.OnChangeTarget, ChangeTarget);
        EventManager.Suscribe(EventManager.KindOfEvent.OnLockCamera, ChangeLookMode);
        EventManager.Suscribe(EventManager.KindOfEvent.ChangeSensibilitieMouse, UpdateMouseSens);

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        _pjModel.OnAim += HandleRotation;
        _falseUpdate += RotateCameraLateUpdate;
    }

    void LateUpdate()
    {
        _falseUpdate();
    }

    private void RotateCameraLateUpdate()
    {
        if (_target == null) return;

        HandleCollision();
        HandleFollow();
        //HandleZoom();
        _cam.transform.position=transform.position;
        _cam.transform.rotation=transform.rotation;
    }

    private void HandleRotation(float mouseX, float mouseY)
    {
        mouseX *= _sensitivity;
        mouseY *= _sensitivity;
        _yaw += mouseX;
        _pitch -= mouseY;
        _pitch = Mathf.Clamp(_pitch, _minPitch, _maxPitch);
    }

    private void HandleCollision()
    {
        Quaternion rotation = Quaternion.Euler(_pitch, _yaw, 0);
        _desiredPosition = _target.position - (rotation * Vector3.forward * _distance);

        RaycastHit hit;
        if (Physics.Linecast(_target.position, _desiredPosition, out hit, _collisionMask))
            _desiredPosition = hit.point + hit.normal * 0.2f;
    }

    private void HandleFollow()
    {
        //_cam.transform.position = Vector3.SmoothDamp(transform.position, _desiredPosition, ref _currentVelocity, _smoothTime);
        transform.position = Vector3.SmoothDamp(transform.position, _desiredPosition, ref _currentVelocity, _smoothTime);
        //_cam.transform.LookAt(_target.position);
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

        _targets = Physics.OverlapSphere(_pjModel.transform.position, 100f, _lockeableLayer);
        if (_targets.Length <= 0) return;

        _falseUpdate -= RotateCameraLateUpdate;
        _falseUpdate += UpdateTarget;

        _lookTarget = null;
        _minDistanceLocked = Mathf.Infinity;

        foreach (Collider c in _targets)
        {
            if (!GameManager.Instance.LineOfSight(transform.position, c.transform.position))
                continue;

            _visionAngleTarget = Vector3.Dot(transform.forward, (c.transform.position - transform.position).normalized);
            if (_visionAngleTarget <= 0.75f)
                continue;

            float dist = Vector3.Distance(c.transform.position, _pjModel.transform.position);
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
        if (_lookTarget == null)
        {
            _falseUpdate += RotateCameraLateUpdate;
            _falseUpdate -= UpdateTarget;
            _focusing = false;
            return;
        }

        if (Vector3.Distance(_lookTarget.position, transform.position) > _distanceLocked)
        {
            _lookTarget = null;
            _falseUpdate += RotateCameraLateUpdate;
            _falseUpdate -= UpdateTarget;
            _focusing = false;
            return;
        }

        Quaternion targetRot = Quaternion.LookRotation(_lookTarget.position - _pjModel.transform.position);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, _lockRotForce * Time.deltaTime);

        if (Quaternion.Angle(transform.rotation, targetRot) < 1f)
        {
            _focusing = false;
            _falseUpdate += RotateCameraLateUpdate;
            _falseUpdate -= UpdateTarget;

            Vector3 euler = transform.eulerAngles;
            _yaw = euler.y;
            _pitch = euler.x;
        }
    }

    private void ChangeTarget(params object[] p)
    {
        if (_lookTarget == null || _targets.Length <= 1)
            return;

        int dir = (int)p[0];
        Vector3 playerPos = _pjModel.transform.position;
        Vector3 forward = (_lookTarget.position - playerPos).normalized;

        Transform bestTarget = null;
        float bestAngle = dir == 1 ? 360f : -360f;

        foreach (var t in _targets)
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
            _lookTarget = bestTarget;
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

        _targets = Physics.OverlapSphere(_pjModel.transform.position, 100f, _lockeableLayer);
        if (_targets.Length <= 0) return;

        _falseUpdate -= RotateCameraLateUpdate;
        _falseUpdate += UpdateTarget;

        _lookTarget = null;
        _minDistanceLocked = Mathf.Infinity;

        foreach (Collider c in _targets)
        {
            if (!GameManager.Instance.LineOfSight(transform.position, c.transform.position))
                continue;

            float dist = Vector3.Distance(c.transform.position, _pjModel.transform.position);
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
        if ((Collider)obj[0] != null)
        {
            if (_targets.ToList().Contains(obj[0]))
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
    }
    /*  [Header("Target")]
      [SerializeField] private Transform _target;
      [SerializeField] private Transform _lookTarget;
      [SerializeField] private PjModel _pjModel;
      [Header("Settings")]
      [SerializeField][Range(5, 50)] private float _distanceLocked = 5f;
      [SerializeField][Range(5,10)] private float _distance = 5f;           
      [SerializeField][Range(0.1f,10)] private float _sensitivity = 3f;
      [SerializeField][Range(0, 10)] private float _lockRotForce;
      [SerializeField] private float _smoothTime = 0.15f;     
      [SerializeField] private float _zoomFov = 30f;          
      [SerializeField] private LayerMask _collisionMask;
      [SerializeField] private LayerMask _lockeableLayer;
      private float _visionAngleTarget;

      [Header("Internals")]
      [SerializeField] private float _minPitch = -30f;         
      [SerializeField] private float _maxPitch = 60f;         

      private float _yaw;                    
      private float _pitch;                 
      private Vector3 _currentVelocity;      
      private Vector3 _desiredPosition;      
      private Camera _cam;
      private float _defaultFov;
      private bool _zoomed = false;
      private bool _focusing = false;
      private Quaternion focusRotation;
      private Action _falseUpdate = delegate { };
      [SerializeField] private Collider[] _targets;
      private float _minDistanceLocked;
      void Start()
      {
          if (_cam == null)
          {
              _cam = Camera.main;
          }
          if(_pjModel==null)
          {
              _pjModel = GetComponent<PjModel>();
          }
          _defaultFov = _cam.fieldOfView;

          Vector3 euler = transform.eulerAngles;
          _yaw = euler.y;
          _pitch = euler.x;
          EventManager.Suscribe(EventManager.KindOfEvent.OnChangeTarget, ChangeTarget);
          EventManager.Suscribe(EventManager.KindOfEvent.OnLockCamera, ChangeLookMode);
          Cursor.lockState = CursorLockMode.Locked;
          Cursor.visible = false;
          _pjModel.OnAim += HandleRotation;
          _falseUpdate += RotateCameraLateUpdate;
          EventManager.Suscribe(EventManager.KindOfEvent.ChangeSensibilitieMouse, UpdateMouseSens);
      }

      void LateUpdate()
      {
          _falseUpdate();
      }
      private void RotateCameraLateUpdate()
      {
          if (_target == null) return;
          //HandleRotation();
          HandleCollision();
          HandleFollow();
          HandleZoom();
      }
      private void HandleRotation(float MouseX, float MouseY )
      {
          MouseX*=_sensitivity;
          MouseY*=_sensitivity;
          _yaw += MouseX;
          _pitch -= MouseY;
          _pitch = Mathf.Clamp(_pitch, _minPitch, _maxPitch);
      }

      private void HandleCollision()
      {
          Quaternion rotation = Quaternion.Euler(_pitch, _yaw, 0);
          _desiredPosition = _target.position - (rotation * Vector3.forward * _distance);

          RaycastHit hit;
          if (Physics.Linecast(_target.position, _desiredPosition, out hit, _collisionMask))
          {
              _desiredPosition = hit.point + hit.normal * 0.2f;
          }
      }

      private void HandleFollow()
      {
          transform.position = Vector3.SmoothDamp(transform.position, _desiredPosition, ref _currentVelocity, _smoothTime);

          transform.LookAt(_target.position);
      }

      private void HandleZoom()
      {
          if (Input.GetMouseButtonDown(1))
          {
              _zoomed = !_zoomed;
          }

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
          _targets = Physics.OverlapSphere(_pjModel.transform.position, 100f, _lockeableLayer);
          if (_targets.Length <= 0) { return; }
          else
          {
              _falseUpdate -= RotateCameraLateUpdate;
              _falseUpdate += UpdateTarget;
          }
          _lookTarget = null;
          _minDistanceLocked = Mathf.Infinity;

          foreach (Collider c in _targets)
          {
              if (!GameManager.Instance.LineOfSight(transform.position, c.transform.position))
              {
                  continue;
              }

              _visionAngleTarget = Vector3.Dot(transform.forward, (c.transform.position - transform.position).normalized);
              if (_visionAngleTarget <= 0.75f)
              {
                  continue;
              }

              if (Vector3.Distance(c.transform.position, _pjModel.transform.position) < _minDistanceLocked)
              {
                  _focusing = true;
                  _lookTarget = c.transform;
                  _minDistanceLocked = Vector3.Distance(c.transform.position, _pjModel.transform.position);
              }
          }
      }
      private void UpdateTarget()
      {
          if (_lookTarget == null)
          {
              _falseUpdate += RotateCameraLateUpdate;
              _falseUpdate -= UpdateTarget;
              _focusing = false;
              return;
          }
          if (Vector3.Distance(_lookTarget.position, transform.position) > _distanceLocked)
          {
              _lookTarget = null;
              _falseUpdate += RotateCameraLateUpdate;
              _falseUpdate -= UpdateTarget;
              _focusing = false;
              return;
          }
          transform.rotation = Quaternion.Slerp(_pjModel.transform.rotation, Quaternion.LookRotation(_lookTarget.position - _pjModel.transform.position), _lockRotForce);
      }
      private void ChangeTarget(params object[] p)
      {
          if (_lookTarget == null || _targets.Length <= 1)
              return;

          int dir = (int)p[0];

          Vector3 playerPos = _pjModel.transform.position;
          Vector3 forward = (_lookTarget.position - playerPos).normalized;

          Transform bestTarget = null;
          float bestAngle = dir == 1 ? 360f : -360f;

          foreach (var t in _targets)
          {
              if (t == null || t.transform == _lookTarget) continue;

              Vector3 toTarget = (t.transform.position - playerPos).normalized;

              float angle = Vector3.SignedAngle(forward, toTarget, Vector3.up);

              if (Vector3.Distance(playerPos, t.transform.position) <= _distanceLocked &&
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
              _lookTarget = bestTarget;
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
          _targets = Physics.OverlapSphere(_pjModel.transform.position, 100f, _lockeableLayer);
          if (_targets.Length <= 0) { return; }
          else
          {
              _falseUpdate -= RotateCameraLateUpdate;
              _falseUpdate += UpdateTarget;
          }
          _lookTarget = null;
          _minDistanceLocked = Mathf.Infinity;

          foreach (Collider c in _targets)
          {
              if (!GameManager.Instance.LineOfSight(transform.position, c.transform.position))
              {
                  continue;
              }

              if (Vector3.Distance(c.transform.position, _pjModel.transform.position) < _minDistanceLocked)
              {
                  _focusing = true;
                  _lookTarget = c.transform;
                  _minDistanceLocked = Vector3.Distance(c.transform.position, _pjModel.transform.position);
              }
          }
      }
      private void OnEnemyKilledChangeTarget(params object[] obj)
      {
          if ((Collider)obj[0] != null)
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
      }*/
}
