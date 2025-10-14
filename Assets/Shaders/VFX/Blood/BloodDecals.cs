using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.HighDefinition;
public class BloodDecals : MonoBehaviour
{
    [SerializeField] GameObject _myDecalPref;
    [SerializeField] ParticleSystem _bloodSpill;
    [SerializeField] ParticleSystem _myPS;
    [SerializeField] float _offset = .01f;
    [SerializeField] float _lerpDuration = 1f;
    [SerializeField] float _decalDuration = 5f;
    List<ParticleCollisionEvent> collisionEvents = new List<ParticleCollisionEvent>();
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnParticleCollision(GameObject other)
    {
        int _eventCount = _myPS.GetCollisionEvents(other, collisionEvents);

        foreach(var pos in collisionEvents)
        {
            Vector3 _hitPos = pos.intersection;
            Vector3 hitNomral = pos.normal;

            Vector3 spawnPos = _hitPos + hitNomral * _offset;

            Quaternion rotation = Quaternion.LookRotation(hitNomral);

            GameObject x = Instantiate(_myDecalPref, spawnPos, rotation, other.transform);
            Instantiate(_bloodSpill, pos.intersection, rotation, x.transform);
        }    
    }

    IEnumerator DespawnDecal(DecalProjector _myDecal)
    {
        yield return new WaitForSeconds(_decalDuration);
        float elapsed = 0f;
        while (elapsed <= _lerpDuration)
        {
            _myDecal.fadeFactor = Mathf.Lerp(1, 0, elapsed / _lerpDuration);
            elapsed += Time.deltaTime;
            yield return null;
        }
        Destroy(_myDecal.gameObject);
    }
}
