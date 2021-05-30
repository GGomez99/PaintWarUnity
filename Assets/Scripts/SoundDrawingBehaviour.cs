using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundDrawingBehaviour : MonoBehaviour
{
    public DrawingBehaviour drawing;
    public AudioSource filling;
    public AudioSource finished;
    private bool isFilling;
    private GameOptions currentOptions;

    // Start is called before the first frame update
    void Start()
    {
        isFilling = false;
    }

    public void SetGameOptions(GameOptions newOptions)
    {
        currentOptions = newOptions;
    }

    // Update is called once per frame
    void Update()
    {
        if (currentOptions != null)
        {
            filling.volume = currentOptions.FXSound / 100f;
            finished.volume = currentOptions.FXSound / 100f;
        }

        if (!isFilling && drawing.DoFilling.Value)
        {
            filling.Play();
            isFilling = true;
        } else if (isFilling && !drawing.DoFilling.Value)
        {
            filling.Stop();
            finished.Play();
            isFilling = false;
        }
    }
}
