#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

public class CubeMapGen : ScriptableWizard
{
    [SerializeField] Camera _camera;

    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    private void OnWizardUpdate()
    {
        helpString = "Selecciona la camara para renderear un Cubemap";
        isValid = (_camera != null);
    }

    private void OnWizardCreate()
    {
        Cubemap cubemap = new Cubemap(512, TextureFormat.ARGB32, false);
        _camera.RenderToCubemap(cubemap);
        AssetDatabase.CreateAsset(cubemap, $"Assets/Shaders/FakeInterior/{_camera.name}.cubemap");
    }

    [MenuItem("Toolbox/Cubemap Wizard")]
    static void RenderCubemap()
    {
        DisplayWizard<CubeMapGen>("Render Cubemap", "Render");
    }
}
#endif //UNITY_EDITOR