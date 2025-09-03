using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnerEnemy : MonoBehaviour
{
    private int i = 0;

    private void Start()
    {
       //GameManager.Instance.GenericUpdate+= FalseUpdate;
       StartCoroutine(Spawn());
    }
    private IEnumerator Spawn()
    {
        int j = Random.Range(0, 11);
        yield return new WaitForSeconds(Random.Range(3, 5));
        for (int r = 0; r < j; r++)
        {
            yield return new WaitUntil(()=>!GameManager.Instance.IsPaused);
            i = Random.Range(0, 101);
            if (i <= 75)
            {
                var p = EsqueletonFctory.instance.GetObj();
                p.transform.position = transform.position;
                p.transform.rotation = transform.rotation;
            }
            else
            {
                var p = MagueFactory.instance.GetObj();
                p.transform.position = transform.position;
                p.transform.rotation = transform.rotation;
            }
            yield return new WaitForSeconds(Random.Range(3,5));
            //GameManager.Instance.GenericUpdate -= FalseUpdate;
        }
    }
   /* public void FalseUpdate()
    {
        int j = Random.Range(0, 11);

        for (int r = 0; r < j; r++)
        {
            i=Random.Range(0, 101);
            if(i<=75)
            {
                var p= EsqueletonFctory.instance.GetObj();
                p.transform.position = p.transform.position;
                p.transform.rotation = p.transform.rotation;
            }
            else
            {
                var p = MagueFactory.instance.GetObj();
                p.transform.position = p.transform.position;
                p.transform.rotation = p.transform.rotation;
            }
            GameManager.Instance.GenericUpdate -= FalseUpdate;
        }
    }*/
}
