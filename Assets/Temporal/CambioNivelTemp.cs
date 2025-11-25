using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CambioNivelTemp : MonoBehaviour
{
    [SerializeField] FadeinOut _fade;
    [SerializeField] private string SceneName;

    private void OnTriggerEnter(Collider other)
    {
        _fade = other.GetComponent<FadeinOut>();
        StartCoroutine(Ending());
        
    }

    IEnumerator Ending()
    {
        _fade.StartFadeIn();
        yield return new WaitForSeconds(2);
        if (SaveSystemManager.instance != null)
        {
            SaveSystemManager.instance.SaveData(4);
            SaveSystemManager.instance.SaveData(0);
        }
        if (SceneName != null)
            SceneManager.LoadScene(SceneName);
    }
}
