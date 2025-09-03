using UnityEngine;
[RequireComponent(typeof(Rigidbody))]
public class ParabelShoot : MonoBehaviour
{
    public Transform Target;
    public float LaunchAngle = 45f;
    [SerializeField]private Rigidbody _rb;

    /*void Start()
    {
        if (_rb == null) { _rb = GetComponent<Rigidbody>(); }
        LaunchProjectile();
    }*/

    void LaunchProjectile(Vector3 Position)
    {
        if (Target == null)
        {
            return;
        }
        Vector3 toTarget = Target.position - Position;

        // Separar componentes horizontal y vertical
        Vector3 toTargetXZ = new Vector3(toTarget.x, 0, toTarget.z);
        float distanceXZ = toTargetXZ.magnitude;
        float heightDifference = toTarget.y;

        float angleRad = LaunchAngle * Mathf.Deg2Rad;
        float gravity = Mathf.Abs(Physics.gravity.y);

        // Fórmula completa
        float cosAngle = Mathf.Cos(angleRad);
        float sinAngle = Mathf.Sin(angleRad);

        float numerator = gravity * distanceXZ * distanceXZ;
        float denominator = 2 * cosAngle * cosAngle * (distanceXZ * Mathf.Tan(angleRad) - heightDifference);

        if (denominator <= 0)
        {
            Debug.LogWarning("No hay solución real: angulo muy bajo o objetivo demasiado alto");
            return;
        }

        float velocity = Mathf.Sqrt(numerator / denominator);

        // Dirección final del disparo
        Vector3 velocityXZ = toTargetXZ.normalized * velocity * cosAngle;
        float velocityY = velocity * sinAngle;

        Vector3 finalVelocity = velocityXZ + Vector3.up * velocityY;

        _rb.velocity = finalVelocity;
    }
}

