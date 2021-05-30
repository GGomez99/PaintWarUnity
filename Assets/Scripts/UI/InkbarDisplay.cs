using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InkbarDisplay : MonoBehaviour
{

    public Slider inkSlider;
    public PlayerData LocalPlayer;
    public Image SliderFill;
    public Image IconFill;
    public Image SliderBG;

    // Start is called before the first frame update
    public void UpdateColor()
    {
        SliderFill.color = LocalPlayer.TeamColor.Value;
        IconFill.color = LocalPlayer.TeamColor.Value;
        Color32 teamColor = LocalPlayer.TeamColor.Value;
        Color32 newColor = Color.black;
        Color32 BGColor = Color.Lerp(teamColor, newColor, 0.5f);
        SliderBG.color = BGColor;

    }

    // Update is called once per frame
    void Update()
    {
        if (LocalPlayer != null)
        {
            inkSlider.maxValue = LocalPlayer.MaxInk.Value;
            inkSlider.value = LocalPlayer.Ink.Value;
        }
    }
}
