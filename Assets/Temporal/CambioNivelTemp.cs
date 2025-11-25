using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CambioNivelTemp : MonoBehaviour
{
    [SerializeField] private string SceneName;
    private void OnTriggerEnter(Collider other)
    {
        if (SaveSystemManager.instance != null)
        {
            SaveSystemManager.instance.SaveData(4);
            SaveSystemManager.instance.SaveData(0);
        }
        if (SceneName!=null)
        SceneManager.LoadScene(SceneName);
    }
}
