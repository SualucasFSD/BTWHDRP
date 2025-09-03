using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[RequireComponent(typeof(Rigidbody))]
public class MovPj : MonoBehaviour
{
    [SerializeField] private Rigidbody _rb;
    private PlayerController _playerController;
    private Vector3 DirControls= Vector3.zero;
   /* private void Awake()
    {
        _rb = GetComponent<Rigidbody>();
        _playerController = GetComponent<PlayerController>();
    }
    private void FixedUpdate()
    {
        ManualMovement();
        //_rb.position += Dir * Time.fixedDeltaTime;
    }
    private void ManualMovement(Vector3 Direction)
    {
       /* DirControls.z = Input.GetAxis("Vertical");
        DirControls.x = Input.GetAxis("Horizontal");*/
     /*   if (DirControls.sqrMagnitude > 1)
        {
            DirControls.Normalize();
        }
        Dir = DirControls;
        _rb.position += Dir * Time.fixedDeltaTime;
    }*/
}
