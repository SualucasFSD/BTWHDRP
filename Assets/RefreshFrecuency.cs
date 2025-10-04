using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class RefreshFrecuency : MonoBehaviour
{
    [SerializeField] private TMP_Dropdown refreshDropdown;

    private Resolution[] allResolutions;
    private List<RefreshRate> availableRates = new List<RefreshRate>();
    private int currentRefreshIndex = 0;
    private int currentWidth;
    private int currentHeight;

    private void OnEnable()
    {
        EventManager.Suscribe(EventManager.KindOfEvent.OnChangeResolution, OnResolutionChanged);
    }

    private void OnDisable()
    {
        EventManager.Unscribe(EventManager.KindOfEvent.OnChangeResolution, OnResolutionChanged);
    }
    private void OnDestroy()
    {
        EventManager.Unscribe(EventManager.KindOfEvent.OnChangeResolution, OnResolutionChanged);
    }
    private void Start()
    {
        if (refreshDropdown == null)
        {
            Debug.LogError("[RefreshRateDropdown] Falta asignar el dropdown de frecuencias.");
            return;
        }

        allResolutions = Screen.resolutions;
        refreshDropdown.onValueChanged.AddListener(OnRefreshChanged);

        currentWidth = Screen.currentResolution.width;
        currentHeight = Screen.currentResolution.height;
        PopulateDropdown(currentWidth, currentHeight);
    }

    private void PopulateDropdown(int width, int height)
    {
        refreshDropdown.ClearOptions();
        availableRates.Clear();

        List<string> options = new List<string>();
        HashSet<int> uniqueRates = new HashSet<int>();

        foreach (var res in allResolutions)
        {
            if (res.width == width && res.height == height)
            {
                int rate = Mathf.RoundToInt((float)res.refreshRateRatio.value);
                if (!uniqueRates.Contains(rate))
                {
                    uniqueRates.Add(rate);
                    availableRates.Add(res.refreshRateRatio);
                    options.Add($"{rate} Hz");

                    if (Mathf.Approximately((float)res.refreshRateRatio.value, (float)Screen.currentResolution.refreshRateRatio.value))
                        currentRefreshIndex = availableRates.Count - 1;
                }
            }
        }

        if (availableRates.Count == 0)
        {
            availableRates.Add(Screen.currentResolution.refreshRateRatio);
            options.Add($"{Screen.currentResolution.refreshRateRatio.value:0} Hz");
            currentRefreshIndex = 0;
        }

        refreshDropdown.AddOptions(options);
        refreshDropdown.value = currentRefreshIndex;
        refreshDropdown.RefreshShownValue();
    }

    private void OnResolutionChanged(params object[] p)
    {
        print("Actualizado");
        if (p.Length < 2) return;

        currentWidth = (int)p[0];
        currentHeight = (int)p[1];

        PopulateDropdown(currentWidth, currentHeight);
    }

    private void OnRefreshChanged(int index)
    {
        if (index < 0 || index >= availableRates.Count)
            return;

        currentRefreshIndex = index;

        RefreshRate rate = availableRates[index];

        Screen.fullScreenMode = FullScreenMode.ExclusiveFullScreen;
        Screen.SetResolution(currentWidth, currentHeight, Screen.fullScreenMode, rate);

        Debug.Log($"[RefreshRateDropdown] Aplicada: {currentWidth}x{currentHeight} @{rate.value:0}Hz");
    }
}
