using UnityEngine;
using UnityEngine.VFX;

public class EffectEvent : MonoBehaviour
{
    [SerializeField] private ParticleSystem[] _particleSystems;
    [SerializeField] private VisualEffect[] _vfxSystems;

    public void EjecuteEffect(int P)
    {
        if (P >= 0 && P < _particleSystems.Length && _particleSystems[P] != null)
        {
            _particleSystems[P].Play();
        }
        else
        {
            Debug.LogWarning("Invalid particle system index or null reference");
        }
    }

    public void FinishEffect(int P)
    {
        if (P >= 0 && P < _particleSystems.Length && _particleSystems[P] != null)
        {
            _particleSystems[P].Stop();
        }
        else
        {
            Debug.LogWarning("Invalid particle system index or null reference");
        }
    }

    public void EjecuteVfx(int P)
    {
        if (P >= 0 && P < _vfxSystems.Length && _vfxSystems[P] != null)
        {
            _vfxSystems[P].Play();
        }
        else
        {
            Debug.LogWarning("Invalid VFX index or null reference");
        }
    }

    public void FinishVfx(int P)
    {
        if (P >= 0 && P < _vfxSystems.Length && _vfxSystems[P] != null)
        {
            _vfxSystems[P].Stop();
        }
        else
        {
            Debug.LogWarning("Invalid VFX index or null reference");
        }
    }
}
