using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationEvents : MonoBehaviour
{

    [SerializeField] ParticleSystem[] _myParticles;
    [SerializeField] AudioSource _mySource;
    public void PlayParticle(int desiredParticle)
    {
        _myParticles[desiredParticle].Play();
    }

    public void StopParticle(int desiredParticle)
    {
        _myParticles[desiredParticle].Stop();
    }

    public void PlayerOneShot(soundType _type)
    {
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlayOneShot(entityType.player, _type, _mySource);
        }
    }
    public void PlayerOneShotSpecificSound(AudioClip myClip)
    {
        _mySource.PlayOneShot(myClip);
    }
}
