using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BeginBoss : MonoBehaviour
{
    [SerializeField]private FirstBoss Boss;

    private void OnTriggerEnter(Collider other)
    {
        if (Boss != null)
        {
            Boss.Begin();
        }
    }
}
