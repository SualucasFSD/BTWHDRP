using UnityEngine;

public class SyncTerrainWithRoom : MonoBehaviour
{
    [SerializeField] private Terrain terrain;
    [SerializeField] private Vector3 _offset = Vector3.zero;

    private void Start()
    {
        if (terrain == null)
            terrain = GetComponentInChildren<Terrain>();

        if (terrain == null)
        {
            Debug.LogWarning("No se encontró un Terrain dentro de la habitación.");
            return;
        }

        terrain.transform.position = transform.position + _offset;

        terrain.transform.rotation = transform.rotation;

        terrain.Flush();
    }
}
