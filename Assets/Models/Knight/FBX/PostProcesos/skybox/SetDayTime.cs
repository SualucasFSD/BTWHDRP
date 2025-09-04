using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[ExecuteInEditMode]
public class SetDayTime : MonoBehaviour
{
    
    [SerializeField] private Material skyboxMat;
    /*[SerializeField] private Material cloudsMat;
    [SerializeField] private Material cloudsUpMat;
    [SerializeField] private string timeSlider = "_TimeClouds";
    [Range(0, 1)] public float dayCloudValue = 0.0f;
    [Range(0, 1)] public float nigthCloudValue = 1.0f;
    [SerializeField] private float transitionSmoothness = 2f;*/



    void Update()
    {
        skyboxMat.SetVector("_MainLightDirection", transform.forward);
        skyboxMat.SetVector("_MainLightUp", transform.forward);
        skyboxMat.SetVector("_MainLightRight", transform.forward);
        /*if (skyboxMat != null)
        {
            skyboxMat.SetVector("_MainLightDirection", transform.forward);
        }
        UpdateClouds();
    }
    private void UpdateClouds()
    {
        if (cloudsMat == null) return;
        float ligthAngle= transform.eulerAngles.y;
        float normalizedTime = Mathf.Clamp01(Mathf.Sin(ligthAngle * Mathf.Deg2Rad) * 0.5f + 0.5f);
        float targetValue = Mathf.Lerp(dayCloudValue, nigthCloudValue, normalizedTime);
        float currentValue = cloudsMat.GetFloat(timeSlider);
        float newValue = Mathf.Lerp(currentValue, targetValue, Time.deltaTime * transitionSmoothness);
        cloudsMat.SetFloat(timeSlider, newValue);*/

    }
}
