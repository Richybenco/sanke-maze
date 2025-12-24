using UnityEngine;

public class SnakeAudioManager : MonoBehaviour
{
    [Header("Audio Source")]
    public AudioSource audioSource;

    [Header("Clips")]
    public AudioClip moveClip;
    public AudioClip eatClip;
    public AudioClip crashClip;

    [Header("Sound Toggles")]
    public bool enableMove = true;
    public bool enableEat = true;
    public bool enableCrash = true;

    void OnEnable()
    {
        SnakeController.SnakeMoved += PlayMove;
        AppleSpawner.AppleEaten += PlayEat;
        UIManager.SnakeCrashed += PlayCrash;
    }

    void OnDisable()
    {
        SnakeController.SnakeMoved -= PlayMove;
        AppleSpawner.AppleEaten -= PlayEat;
        UIManager.SnakeCrashed -= PlayCrash;
    }

    void PlayMove()
    {
        if (enableMove && moveClip != null && audioSource != null)
            audioSource.PlayOneShot(moveClip);
    }

    void PlayEat()
    {
        if (enableEat && eatClip != null && audioSource != null)
            audioSource.PlayOneShot(eatClip);
    }

    void PlayCrash()
    {
        if (enableCrash && crashClip != null && audioSource != null)
            audioSource.PlayOneShot(crashClip);
    }
}