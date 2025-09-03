using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spawner : MonoBehaviour
{
    [SerializeField] private GameObject[] EnemysPrefab=new GameObject[2];
    public void SpawnReady()
    {
        int iti = Random.Range(3, 6);
        int j=0;
        for(int i=0;i<iti;i++)
        {
            j= Random.Range(0,100);
            if(j>15)
            {
                Instantiate(EnemysPrefab[0]);
            }
            else
            {
                Instantiate(EnemysPrefab[1]);
            }
        }
    }
}
