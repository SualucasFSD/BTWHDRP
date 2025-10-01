using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;
using TMPro;

public class ConfiguratonDropDown : MonoBehaviour
{
    [Header("HDRP Assets")]
    public HDRenderPipelineAsset lowAsset;
    public HDRenderPipelineAsset mediumAsset;
    public HDRenderPipelineAsset highAsset;

    public void ChangeGraphics(int index)
    {
        QualitySettings.SetQualityLevel(index, true);
        switch (index)
        {
            case 0:
                GraphicsSettings.renderPipelineAsset = lowAsset;
                break;
            case 1:
                GraphicsSettings.renderPipelineAsset = mediumAsset;
                break;
            case 2:
                GraphicsSettings.renderPipelineAsset = highAsset;
                break;
            default:
                Debug.LogWarning("Index de calidad inválido");
                break;
        }

        Debug.Log("Calidad cambiada a: " + QualitySettings.names[index]);
    }
}

