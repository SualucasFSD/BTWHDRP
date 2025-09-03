using UnityEngine;

public class RagdollOnOff : MonoBehaviour
{
    [SerializeField] CapsuleCollider mainCollider;
    [SerializeField] GameObject BasicEnemyRig;
    [SerializeField] Animator BasicEnemyAnimator;
    void Start()
    {
        GetRagdollBits();
        RagdollModeOff();
    }
    Collider[] ragdollColliders;
    Rigidbody[] limbsRigibodies;
    void GetRagdollBits()
    {
        ragdollColliders = BasicEnemyRig.GetComponentsInChildren<Collider>();
        limbsRigibodies = BasicEnemyRig.GetComponentsInChildren<Rigidbody>();
    }
    public void RagdollModeOn(Vector3 dir,float force)
    {
        BasicEnemyAnimator.enabled = false;
        mainCollider.enabled = false;

        foreach (Collider col in ragdollColliders)
            col.enabled = true;

        foreach (Rigidbody rigid in limbsRigibodies)
            rigid.isKinematic = false;

        Rigidbody mainRb = GetComponentInParent<Rigidbody>();
        if (mainRb != null)
            mainRb.isKinematic = true;
        foreach (Rigidbody limb in limbsRigibodies)
        {
            limb.AddForce(dir * force, ForceMode.Impulse);
        }
    }
    public void RagdollModeOff()
    {
        foreach (Collider col in ragdollColliders)
        {
            col.enabled = false;
        }
        foreach (Rigidbody rigid in limbsRigibodies)
        {
            rigid.isKinematic = true;
        }

        BasicEnemyAnimator.enabled = true;
        mainCollider.enabled = true;
        GetComponentInParent<Rigidbody>().isKinematic = false;   
    }

}
