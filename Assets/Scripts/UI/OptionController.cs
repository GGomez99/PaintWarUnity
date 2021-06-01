using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OptionController : MonoBehaviour
{
    public GameObject OptionsWindow;
    public OptionsFileManager fileManager;

    public void ToggleDisplayWindow()
    {
        OptionsWindow.SetActive(!OptionsWindow.activeSelf);
        if (!OptionsWindow.activeSelf)
        {
            fileManager.SavePlayerOptions();
        }
    }
}
