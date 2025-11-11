using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class AcquireAbility : MonoBehaviour
{
    [SerializeField] AudioSource _mySource;
    [SerializeField] TMP_Text text;
    [SerializeField] ParticleSystem _particles;

    [SerializeField][TextArea] string _desiredText;
    [SerializeField] float _textSpeed;

    public bool isPlaying = false;


    public IEnumerator OnAbilityAcquired()
    {
        //var tempText = _desiredText;
        //tempText.
        var tempText = _desiredText.ToCharArray();
        isPlaying = true;
        text.enabled = true;
        yield return new WaitForSeconds(.5f);
        for (int i = 0; i < tempText.Length; i++)
        {
            text.text += _desiredText[i];
            yield return new WaitForSeconds(_textSpeed);
        }
        var textMesh = text.mesh;
        var shape = _particles.shape;
        shape.mesh = textMesh;
        yield return new WaitForSeconds(1.5f);
        text.text = null;
        text.enabled = false;
        _particles.Play();
        isPlaying = false;
        //this.gameObject.SetActive(false);
    }
}
