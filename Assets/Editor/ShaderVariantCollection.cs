#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System;

public static class CollectShaderVariants
{
    [MenuItem("Tools/Collect Shader Variants")]
    public static void Collect()
    {
        string path = "Assets/AllVariants.shadervariants";

        // Cargar si ya existe
        UnityEngine.ShaderVariantCollection svc = AssetDatabase.LoadAssetAtPath<UnityEngine.ShaderVariantCollection>(path);
        if (svc == null)
        {
            // Crear la colección (NO CreateInstance porque no es un ScriptableObject)
            svc = new UnityEngine.ShaderVariantCollection();
            AssetDatabase.CreateAsset(svc, path);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        svc.Clear();

        string[] guids = AssetDatabase.FindAssets("t:Material");
        int total = guids.Length;
        for (int i = 0; i < total; i++)
        {
            EditorUtility.DisplayProgressBar("Collecting shader variants", $"Scanning materials ({i + 1}/{total})", (float)i / (float)Math.Max(1, total));
            string matPath = AssetDatabase.GUIDToAssetPath(guids[i]);
            Material mat = AssetDatabase.LoadAssetAtPath<Material>(matPath);
            if (mat == null || mat.shader == null) continue;

            Shader shader = mat.shader;
            string[] keywords = mat.shaderKeywords; // array de strings

            // Recorremos todos los PassType del engine (usa el tipo totalmente calificado para evitar colisiones de namesapce)
            Array passValues = Enum.GetValues(typeof(UnityEngine.Rendering.PassType));
            foreach (var pv in passValues)
            {
                var pass = (UnityEngine.Rendering.PassType)pv;
                try
                {
                    UnityEngine.ShaderVariantCollection.ShaderVariant variant;
                    if (keywords != null && keywords.Length > 0)
                        variant = new UnityEngine.ShaderVariantCollection.ShaderVariant(shader, pass, keywords);
                    else
                        variant = new UnityEngine.ShaderVariantCollection.ShaderVariant(shader, pass);

                    if (!svc.Contains(variant))
                        svc.Add(variant);
                }
                catch (ArgumentException)
                {
                    // combinación no válida para este shader => ignorar
                }
            }
        }

        EditorUtility.ClearProgressBar();
        EditorUtility.SetDirty(svc);
        AssetDatabase.SaveAssets();
        Debug.Log($"ShaderVariantCollection creado/actualizado en: {path} (variants: {svc.variantCount})");
    }
}
#endif