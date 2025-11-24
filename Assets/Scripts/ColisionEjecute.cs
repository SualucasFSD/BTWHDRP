using UnityEngine;
[RequireComponent(typeof(Rigidbody))]
public class ColisionEjecute : MonoBehaviour
{
    [SerializeField] private Animator _animator;
    private bool _boolValue = false;
    [SerializeField] private Rigidbody _rb;
    [SerializeField] private float _vel = 3.2f;
    private float _timer = 0;
    private void Start()
    {
        _animator.speed = 0;
        if (_animator == null)
        {
            _animator = GetComponent<Animator>();
        }
        if (GameManager.Instance != null)
        {
            transform.parent = GameManager.Instance.Gameplay;
        }
    }
    private void OnTriggerEnter(Collider other)
    {
        PjModel model = other.GetComponent<PjModel>();
        if (model != null && _animator != null)
        {
            _boolValue = true;
            _animator.speed = 1;
        }
    }
    private void FixedUpdate()
    {
        if(GameManager.Instance.IsPaused)
        {
            return;
        }
        if (_boolValue)
        {
            _timer += Time.deltaTime;
            if (_timer >= 2)
            {
                _rb.MovePosition(transform.position + Vector3.up * _vel * Time.fixedDeltaTime);
            }
        }
    }
}

