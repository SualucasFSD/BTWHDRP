
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ErrorKiller : MonoBehaviour
{
    [ContextMenu("Scan Scene for Invalid AABB")]
    public void ScanScene()
    {
        int errorCount = 0;

        foreach (GameObject go in FindObjectsByType<GameObject>(FindObjectsSortMode.None))
        {
            Transform t = go.transform;

            if (HasInvalid(t.position, "Position", go)) errorCount++;
            if (HasInvalid(t.lossyScale, "Scale", go)) errorCount++;
            if (HasInvalid(t.rotation, "Rotation", go)) errorCount++;

            // Check Renderers
            foreach (Renderer r in go.GetComponents<Renderer>())
            {
                if (HasInvalidBounds(r.bounds, "Renderer.bounds", go)) errorCount++;
            }

            // Check Colliders
            foreach (Collider c in go.GetComponents<Collider>())
            {
                if (HasInvalidBounds(c.bounds, "Collider.bounds", go)) errorCount++;
            }

            // Check UI RectTransforms
            RectTransform rect = go.GetComponent<RectTransform>();
            if (rect != null)
            {
                if (HasInvalid(rect.anchoredPosition, "RectTransform.anchoredPosition", go)) errorCount++;
                if (HasInvalid(rect.sizeDelta, "RectTransform.sizeDelta", go)) errorCount++;
                if (HasInvalid(rect.pivot, "RectTransform.pivot", go)) errorCount++;
            }

            // Check TextMeshPro
            TMP_Text tmp = go.GetComponent<TMP_Text>();
            if (tmp != null)
            {
                float w = tmp.preferredWidth;
                float h = tmp.preferredHeight;
                if (float.IsNaN(w) || float.IsInfinity(w) || w < 0 || w > 10000)
                {
                    Debug.LogError($" TMP_Text.preferredWidth inválido: {w} en {GetFullPath(go.transform)}", go);
                    errorCount++;
                }
                if (float.IsNaN(h) || float.IsInfinity(h) || h < 0 || h > 10000)
                {
                    Debug.LogError($" TMP_Text.preferredHeight inválido: {h} en {GetFullPath(go.transform)}", go);
                    errorCount++;
                }
            }

            // Check UI Layout
            LayoutElement layout = go.GetComponent<LayoutElement>();
            if (layout != null)
            {
                if (float.IsNaN(layout.minWidth) || float.IsInfinity(layout.minWidth))
                    Debug.LogError($" LayoutElement.minWidth inválido en {GetFullPath(go.transform)}", go);
                if (float.IsNaN(layout.minHeight) || float.IsInfinity(layout.minHeight))
                    Debug.LogError($" LayoutElement.minHeight inválido en {GetFullPath(go.transform)}", go);
            }
        }

        if (errorCount == 0)
        {
            Debug.Log(" No se encontraron valores inválidos en la escena.");
        }
        else
        {
            Debug.LogWarning($"Se encontraron {errorCount} elementos con datos inválidos.");
        }
    }

    private bool HasInvalid(Vector3 v, string label, GameObject go)
    {
        if (float.IsNaN(v.x) || float.IsNaN(v.y) || float.IsNaN(v.z) ||
            float.IsInfinity(v.x) || float.IsInfinity(v.y) || float.IsInfinity(v.z))
        {
            Debug.LogError($" {label} inválido: {v} en {GetFullPath(go.transform)}", go);
            return true;
        }
        return false;
    }

    private bool HasInvalid(Quaternion q, string label, GameObject go)
    {
        if (float.IsNaN(q.x) || float.IsNaN(q.y) || float.IsNaN(q.z) || float.IsNaN(q.w) ||
            float.IsInfinity(q.x) || float.IsInfinity(q.y) || float.IsInfinity(q.z) || float.IsInfinity(q.w))
        {
            Debug.LogError($"{label} inválido: {q} en {GetFullPath(go.transform)}", go);
            return true;
        }
        return false;
    }

    private bool HasInvalidBounds(Bounds b, string label, GameObject go)
    {
        if (float.IsNaN(b.min.x) || float.IsNaN(b.max.x) || float.IsInfinity(b.size.x))
        {
            Debug.LogError($" {label} inválido: {b} en {GetFullPath(go.transform)}", go);
            return true;
        }
        return false;
    }

    private string GetFullPath(Transform t)
    {
        return t.parent == null ? t.name : GetFullPath(t.parent) + "/" + t.name;
    }
}

