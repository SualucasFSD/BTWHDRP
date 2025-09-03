using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MusicManager : MonoBehaviour
{
    [SerializeField] AudioSource _firstHalf, _secondHalf, _endSong;
    [SerializeField] float _transitionSpeed;
    public bool isOndSecond = false;
    public bool canEnd = false;
    

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(canEnd)
            StartCoroutine(EndSong());
    }

    public void StartMusic()
    {
        _firstHalf.Play();
    }

    public IEnumerator TransitionToSecondHalf()
    {

        float timeElapsed = 0;

        _secondHalf.Play();

        isOndSecond = true;

        while(timeElapsed < _transitionSpeed)
        {
            print(timeElapsed);
            _firstHalf.volume = Mathf.Lerp(1, 0, timeElapsed / _transitionSpeed);
            _secondHalf.volume = Mathf.Lerp(0, 1, timeElapsed / _transitionSpeed);
            timeElapsed += Time.deltaTime;
            yield return null;
        }
    }

    public IEnumerator EndSong()
    {
        float timeElapsed = 0;

        canEnd = false;

        _endSong.Play();

        while (timeElapsed < _transitionSpeed)
        {
            _secondHalf.volume = Mathf.Lerp(1, 0, timeElapsed / _transitionSpeed);
            _endSong.volume = Mathf.Lerp(0, 1, timeElapsed / _transitionSpeed);
            timeElapsed += Time.deltaTime;
            yield return null;
        }
    }
}
