using UnityEngine;

public class ColumScript : MonoBehaviour
{
    private void Awake()
    {
        Collider[] c = Physics.OverlapSphere(transform.position, 12);
        foreach (Collider c2 in c)
        {
            if(c2.gameObject==gameObject)
            { continue; }
            if(c2.CompareTag("Colum"))
            {
                Destroy(gameObject);
                return;
            }
        }
    }
    /*private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, 12);
    }*/
}
