using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Key : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        PjModel pj = other.GetComponent<PjModel>();
        if (pj != null )
        {
            Destroy(gameObject);
        }
    }
}
