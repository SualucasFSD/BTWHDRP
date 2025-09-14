using System.Collections;
using TMPro;
using UnityEngine;

public class StartGuide : MonoBehaviour
{
    [SerializeField] AudioSource _mySource;
    [SerializeField] TMP_Text text;
    [SerializeField] ParticleSystem _particles;

    [SerializeField][TextArea] string[] _desiredText;
    [SerializeField] float _textSpeed;

    int currentdisplayedChar = 0;
    void Start()
    {
        if(text==null||_particles==null||_mySource==null)
        {
            return;
        }
        StartCoroutine(StartingGuide());
    }

    IEnumerator StartingGuide()
    {
        yield return new WaitForSeconds(5);
        SoundManager.Instance.PlayOneShot(entityType.player, soundType.playerTalk, _mySource, 0);

        for (int i = 0; i < _desiredText[currentdisplayedChar].Length+1; i++)
        {
            text.text = _desiredText[currentdisplayedChar].Substring(0, i);
            yield return new WaitUntil(() => !GameManager.Instance.IsPaused);
            yield return new WaitForSeconds(_textSpeed);
        }

        yield return new WaitForSeconds(2);


        for (int i = 0; i < _desiredText[currentdisplayedChar].Length + 1; i++)
        {
            if (i + 1 < text.text.Length+1)
            {
                string visibleText = text.text.Substring(i + 1);
                string espacio = new string(' ', i + 1);
                text.text = espacio + visibleText;
                yield return new WaitUntil(() => !GameManager.Instance.IsPaused);
            }
            yield return new WaitForSeconds(_textSpeed);
        }
    }

}
