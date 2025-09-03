using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class FallowObject : MonoBehaviour
{
    public Transform Target;
    Rigidbody _rb;
    [SerializeField][Range(1f, 10)] private float _velocity=5;
    private void Start()
    {
        if(_rb == null)
        {
            _rb = GetComponent<Rigidbody>();
        }
        _rb.useGravity = false;
        _rb.constraints=RigidbodyConstraints.FreezeRotation;
        GameManager.Instance.GenericFixedUpdate += FalseFixedUpdate;
    }
    public void FalseFixedUpdate()
    {
        if (Target != null)
        {
            if (Vector3.Distance(transform.position, Target.position) >= 15) { transform.position = Target.transform.position + Vector3.up * 3f; }
            _rb.MovePosition(transform.position+(Target.position-transform.position)*_velocity*Time.fixedDeltaTime);
            transform.forward = Target.forward;
        }
        else { GameManager.Instance.GenericFixedUpdate -= FalseFixedUpdate; }
    }
}
