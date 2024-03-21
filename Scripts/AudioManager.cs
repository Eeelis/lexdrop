using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [SerializeField] private AudioSource audioSource;
    [Space(10)]
    [Header("Audio Clips")]
    [SerializeField] private List<AudioClip> keyPressSounds = new List<AudioClip>();
    [SerializeField] private AudioClip startGame;
    [SerializeField] private AudioClip gameOver;
    [SerializeField] private AudioClip scoreUp;

    public enum AudioClips
    {
        StartGame,
        GameOver,
        ScoreUp
    }

    private void Awake()
    {
        if (Instance == null) { Instance = this; }
    }

    public void PlayAudioClip(AudioClips audioClip, float volume = 1f)
    {   
        audioSource.Stop();
        audioSource.volume = volume;

        switch(audioClip)
        {
            case AudioClips.StartGame:
                audioSource.PlayOneShot(startGame);
                break;
            case AudioClips.GameOver:
                audioSource.PlayOneShot(gameOver);
                break;
            case AudioClips.ScoreUp:
                audioSource.PlayOneShot(scoreUp);
                break;
        }
    }

    public void PlayRandomKeyPressSound(float volume = 1f)
    {
        audioSource.volume = volume;
        audioSource.PlayOneShot(keyPressSounds[UnityEngine.Random.Range(0, keyPressSounds.Count - 1)]);
    }
}
