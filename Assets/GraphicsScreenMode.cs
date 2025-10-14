using TMPro;
using UnityEngine;

public class GraphicsScreenMode : MonoBehaviour
{
    public TMP_Dropdown displayModeDropdown;

    void Start()
    {
        displayModeDropdown.ClearOptions();
        displayModeDropdown.AddOptions(new System.Collections.Generic.List<string>()
        {
            "Exclusive Fullscreen",
            "Borderless Fullscreen",
            "Maximized Window",
            "Windowed"
        });

        displayModeDropdown.value = (int)Screen.fullScreenMode;
        displayModeDropdown.RefreshShownValue();

        displayModeDropdown.onValueChanged.AddListener(ChangeDisplayMode);

        var content = displayModeDropdown.template.GetComponentInChildren<UnityEngine.UI.VerticalLayoutGroup>();
        if (content != null)
        {
            content.spacing = 5f;
        }

        foreach (var item in displayModeDropdown.GetComponentsInChildren<TMP_Text>(true))
        {
            item.textWrappingMode = TMPro.TextWrappingModes.NoWrap;
            //item.enableWordWrapping = false;
            item.margin = new Vector4(-5, 5, 10, 5);
        }
    }
    public void ChangeDisplayMode(int modeIndex)
    {
        Resolution res = Screen.currentResolution;
        FullScreenMode mode = (FullScreenMode)modeIndex;
        Screen.SetResolution(res.width, res.height, mode, res.refreshRateRatio);
        Debug.Log("Modo de pantalla cambiado a: " + mode);
    }
}
