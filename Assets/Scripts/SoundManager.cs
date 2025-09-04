using AYellowpaper.SerializedCollections;
using UnityEngine;

public class SoundManager : MonoBehaviour
{

    public static SoundManager Instance;

    [SerializedDictionary("Prueba", "prueba")]
    public SerializedDictionary<entityType, SerializedDictionary<soundType, AudioClip[]>> audioLibrary = new SerializedDictionary<entityType, SerializedDictionary<soundType, AudioClip[]>>();


    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

    }
    public void PlayOneShot(entityType entityType, soundType soundType, AudioSource source, int specificSound = -1)
    {
        var array = audioLibrary[entityType][soundType];

        if(specificSound < 0)
            source.PlayOneShot(array[Random.Range(0, array.Length)]);
        else
            source.PlayOneShot(array[specificSound]);
    }

}
    public enum entityType
    {
        basic,
        mage,
        player,
    }

    public enum soundType
    {
        walk,
        run,
        attack,
        swordSounds,
        playerTalk,
        rocksSounds,
    }
