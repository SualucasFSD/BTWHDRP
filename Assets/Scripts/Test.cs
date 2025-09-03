using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Test : MonoBehaviour
{
    public GameObject esque;
    private void Awake()
    {
        StartCoroutine(active());
    }
    IEnumerator active()
    {
        yield return new WaitForSeconds(8);
        esque.SetActive(true);
    }
}
