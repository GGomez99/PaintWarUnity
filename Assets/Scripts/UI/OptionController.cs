using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class OptionController : MonoBehaviour
{
    public GameObject OptionsWindow;
    public OptionsFileManager fileManager;
    public LocalOptions options;
    public Slider[] optionSliders;

    public void ToggleDisplayWindow()
    {
        if (OptionsWindow.activeSelf)
        {
            fileManager.SavePlayerOptions();
        } else
        {
            float[] values = { options.FXSound, options.MusicSound, options.CameraSensitivity };
            //update values
            for (int i = 0; i < values.Length; i++)
            {
                optionSliders[i].value = values[i];
            }
        }
        OptionsWindow.SetActive(!OptionsWindow.activeSelf);

    }
}
