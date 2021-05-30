using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Countdown : MonoBehaviour
{
    public Animation CountdownAnim;
    public GameData CurrentGameData;
    public GameOptions CurrentOptions;
    public AudioSource CountdownFX;

    private bool CountStarted;
    private float basedCountdownFXVolume;

    private void Start()
    {
        CountStarted = false;
        basedCountdownFXVolume = CountdownFX.volume;
    }

    // Update is called once per frame
    void Update()
    {
        CountdownFX.volume = basedCountdownFXVolume * CurrentOptions.FXSound / 100f;

        if (CurrentGameData.GameStarted.Value && !CountStarted)
        {
            CountStarted = true;
            CountdownAnim.Play();
        } else if (CurrentGameData.GameEnded.Value)
        {
            CountStarted = false;
        }
    }
}
