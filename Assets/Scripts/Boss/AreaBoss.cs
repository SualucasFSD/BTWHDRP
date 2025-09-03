using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AreaBoss : MonoBehaviour
{
    [SerializeField] private float Height;
    public IEnumerator Initialize()
    {
        while (transform.position.y < Height) { transform.position += Vector3.up * Time.deltaTime; yield return null; }
    }
}
