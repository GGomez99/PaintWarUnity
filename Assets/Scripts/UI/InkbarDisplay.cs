using MLAPI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InkbarDisplay : NetworkBehaviour
{

    public Slider InkSlider;
    public Slider CostSlider;
    public Text InkText;
    public PlayerData LocalPlayer;
    public RectDrawer LocalDrawer;
    public Image SliderFill;
    public Image IconFill;
    public Image SliderBG;
    public Image SliderCost;

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
            InkSlider.maxValue = LocalPlayer.MaxInk.Value;
            InkSlider.value = LocalPlayer.Ink.Value;
            InkText.text = InkSlider.value + "/" + InkSlider.maxValue;
        }

        CostSlider.value = 0;
        if (LocalDrawer != null && LocalDrawer.CurrentDrawingID != 0)
        {
            NetworkObject drawingNetObj = GetNetworkObject(LocalDrawer.CurrentDrawingID);
            if (drawingNetObj != null)
            {

                DrawingBehaviour drawing = drawingNetObj.gameObject.GetComponent<DrawingBehaviour>();

                if (drawing != null)
                {
                    CostSlider.maxValue = InkSlider.value;
                    Vector3 drawScale = drawing.Draw.localScale;
                    float area = drawScale.x * drawScale.y;
                    int areaCost = (int)(area / LocalDrawer.CurrentGameData.InkToAreaPaintRatio);
                    CostSlider.value = areaCost;
                    if (CostSlider.value < areaCost)
                    {
                        SliderCost.color = new Color32(255, 180, 180, 255);
                    }
                    else
                    {
                        SliderCost.color = Color.white;
                    }
                }
            }
            
        }
    }
}
