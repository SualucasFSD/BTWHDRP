
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class GraphicsResolution : MonoBehaviour
{
    [SerializeField] private TMP_Dropdown resolutionDropdown;

    private Resolution[] allResolutions;
    private List<Resolution> filteredResolutions = new List<Resolution>();
    private int currentIndex = 0;

    private void Start()
    {
        if (resolutionDropdown == null)
        {
            Debug.LogError("[ResolutionDropdown] Falta asignar el dropdown de resoluciones.");
            return;
        }

        allResolutions = Screen.resolutions;
        PopulateDropdown();

        resolutionDropdown.onValueChanged.AddListener(OnResolutionChanged);
    }

    private void PopulateDropdown()
    {
        resolutionDropdown.ClearOptions();
        filteredResolutions.Clear();

        List<string> options = new List<string>();

        foreach (var res in allResolutions)
        {
            if (res.width < 1280 || res.height < 720)
                continue;

            string label = $"{res.width} x {res.height}";
            if (!options.Contains(label))
            {
                options.Add(label);
                filteredResolutions.Add(res);

                if (res.width == Screen.currentResolution.width && res.height == Screen.currentResolution.height)
                    currentIndex = filteredResolutions.Count - 1;
            }
        }

        resolutionDropdown.AddOptions(options);
        resolutionDropdown.value = currentIndex;
        resolutionDropdown.RefreshShownValue();
    }

    private void OnResolutionChanged(int index)
    {
        if (index < 0 || index >= filteredResolutions.Count)
            return;

        Resolution selected = filteredResolutions[index];
        currentIndex = index;

        EventManager.Ejecute(EventManager.KindOfEvent.OnChangeResolution, selected.width, selected.height);
    }
}
/*using System.Collections.Generic;
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

            int refreshRate = Mathf.RoundToInt((float)resolutions[i].refreshRateRatio.value);
            string option = $"{resolutions[i].width} x {resolutions[i].height} @{refreshRate}Hz";
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
            content.spacing = 5f;

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

        Screen.SetResolution(res.width, res.height, mode, res.refreshRateRatio);

        Debug.Log($"Resolución aplicada: {res.width}x{res.height} @{res.refreshRateRatio.value:0.##}Hz");
    }
}
*/
/*using System.Collections.Generic;
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
}*/
