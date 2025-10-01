using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class GraphicsResolution : MonoBehaviour
{
    public TMP_Dropdown resolutionDropdown;
    private Resolution[] resolutions;
    private List<Resolution> filteredResolutions = new List<Resolution>();

    void Start()
    {
        resolutions = Screen.resolutions;
        resolutionDropdown.ClearOptions();

        int currentResolutionIndex = 0;
        List<string> options = new List<string>();

        for (int i = 0; i < resolutions.Length; i++)
        {
            if (resolutions[i].width < 1280 || resolutions[i].height < 720)
                continue;

            string option = resolutions[i].width + " x " + resolutions[i].height + " @" + resolutions[i].refreshRate + "Hz";
            options.Add(option);
            filteredResolutions.Add(resolutions[i]);

            if (resolutions[i].width == Screen.currentResolution.width &&
                resolutions[i].height == Screen.currentResolution.height)
            {
                currentResolutionIndex = filteredResolutions.Count - 1;
            }
        }

        resolutionDropdown.AddOptions(options);
        resolutionDropdown.value = currentResolutionIndex;
        resolutionDropdown.RefreshShownValue();

        resolutionDropdown.onValueChanged.AddListener(ApplyResolution);

        var content = resolutionDropdown.template.GetComponentInChildren<UnityEngine.UI.VerticalLayoutGroup>();
        if (content != null)
        {
            content.spacing = 5f;
        }

        foreach (var item in resolutionDropdown.GetComponentsInChildren<TMP_Text>(true))
        {
            item.enableWordWrapping = false;
            item.margin = new Vector4(-5, 5, 10, 5);
        }
    }

    public void ApplyResolution(int index)
    {
        if (index < 0 || index >= filteredResolutions.Count) return;

        Resolution res = filteredResolutions[index];
        FullScreenMode mode = Screen.fullScreenMode;
        Screen.SetResolution(res.width, res.height, mode, res.refreshRate);

        Debug.Log("Resolución aplicada: " + res.width + "x" + res.height);
    }
}
