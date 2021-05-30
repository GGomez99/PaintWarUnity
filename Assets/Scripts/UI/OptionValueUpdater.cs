using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class OptionValueUpdater : MonoBehaviour
{

    public Slider slider;
    public Text textOption;

    // Update is called once per frame
    void Update()
    {
        textOption.text = ((int) slider.value).ToString();
    }
}
