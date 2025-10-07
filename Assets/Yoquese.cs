using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Yoquese : MonoBehaviour
{
    private void Start()
    {
        GetComponent<NextRoom>().enabled = true;
    }
}
