using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class IaMov : MonoBehaviour
{
    public static IaMov Instance;
    [SerializeField] private LayerMask _avoidanceLayer;
    private void Awake()
    {
        Instance = this;
    }
    public Vector3 Pursuit(Entity Obj,Entity target,float Velocity, float RotForce)
    {
        Vector3 Desired = target.transform.position + target.Dir;

        return Seek(Obj,Desired,Velocity,RotForce);
    }
    public Vector3 Seek(Entity Obj,Vector3 Target,float Velocity, float RotForce)
    {
        Vector3 DesirePosition;
        DesirePosition = Target - Obj.transform.position;
        DesirePosition.Normalize();
        DesirePosition *= Velocity;
        Vector3 Steering;
        Steering = DesirePosition - Obj.Dir;
        Steering = Vector3.ClampMagnitude(Steering, RotForce);
        return Steering;
    }
    public Vector3 ObstacleAvoid(Entity Obj,float Speed,float RotForce,float Area)
    {
        Vector3 position = Obj.transform.position;
        Vector3 direcction = Obj.transform.forward;
        float distance = Obj.Dir.magnitude;
        if (Physics.SphereCast(position, Area, direcction, out RaycastHit hit, distance, _avoidanceLayer))
        {
            Transform obstacle = hit.transform;
            Vector3 dirToObj = obstacle.position - position;
            float angleBtw = Vector3.SignedAngle(Obj.transform.forward, dirToObj, Vector3.up);
            Vector3 desired = angleBtw >= 0 ? -Obj.transform.right : Obj.transform.right;
            desired.Normalize();
            desired *= Speed;
            Vector3 steering = Vector3.ClampMagnitude(desired - Obj.Dir, RotForce);
            return steering;
        }
        return Vector3.zero;
    }
    public Vector3 Arrive(Entity Obj,Transform Target,float Speed,float RotForce)
    {
        Vector3 DesirePosition;
        if (Vector3.Distance(Target.position, Obj.transform.position) > 4f)
        {
            return Seek(Obj,Target.position,Speed,RotForce);
        }
        else
        {
            DesirePosition = Target.position - Obj.transform.position;
            DesirePosition.Normalize();
            DesirePosition *= Speed * 0.5f;
            Vector3 Steering;
            Steering = DesirePosition - Obj.Dir;
            Steering = Vector3.ClampMagnitude(Steering, RotForce);
            return Steering;
        }
    }
    private Vector3 Separation(List<Transform> Entity, float Radius,Entity Obj,float Speed,float RotForce)
    {
        Vector3 dir;
        Vector3 desired = Vector3.zero;
        Vector3 steering;
        foreach (Transform T in Entity)
        {
            if (T.gameObject == Obj.gameObject)
            {
                continue;
            }
            dir = T.position - Obj.transform.position;
            if (dir.magnitude > Radius)
            {
                continue;
            }
            desired -= dir;
        }
        if (desired == Vector3.zero)
        {
            return desired;
        }
        desired.Normalize();
        desired *= Speed;
        steering = desired - Obj.Dir;
        steering = Vector3.ClampMagnitude(steering, RotForce);
        return steering;
    }
    public Vector3 GetRotationLerp(Transform Transform ,Vector3 Target,float RotForce)
    {
        Vector3 DesirePosition;
        DesirePosition = Target - Transform.position;
        DesirePosition.Normalize();
        DesirePosition *=  RotForce;
        Vector3 Steering;
        Steering = DesirePosition -transform.forward;
        Steering = Vector3.ClampMagnitude(Steering, RotForce);
        return Steering;
    }
    /*private void AddForce(Vector3 target)
    {
        if (target.magnitude == 0)
        { _dir = Vector3.zero; return; }
        _dir = Vector3.ClampMagnitude(_dir + target, _speed);
    }*/
}
