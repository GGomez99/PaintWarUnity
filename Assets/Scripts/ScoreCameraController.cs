using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScoreCameraController : MonoBehaviour
{
    public GameData CurrentGameData;
    public Camera ScoreCamera;
    public RenderTexture ScoreCamText;
    public int MaxTextResolution = 1080;

    private void Start()
    {
        CurrentGameData.MaxCanvasX.OnValueChanged += valueChangedDelegate;
        CurrentGameData.MaxCanvasY.OnValueChanged += valueChangedDelegate;
        UpdateCameraBorders();
    }

    void valueChangedDelegate(int oldV, int newV)
    {
        UpdateCameraBorders();
    }

    void UpdateCameraBorders()
    {

        if (CurrentGameData.MaxCanvasY.Value >= CurrentGameData.MaxCanvasX.Value)
        {
            ScoreCamText.width = MaxTextResolution * CurrentGameData.MaxCanvasX.Value / CurrentGameData.MaxCanvasY.Value;
            ScoreCamText.height = MaxTextResolution;
        }
        else
        {
            ScoreCamText.height = MaxTextResolution * CurrentGameData.MaxCanvasY.Value / CurrentGameData.MaxCanvasX.Value;
            ScoreCamText.width = MaxTextResolution;
        }

        ScoreCamera.orthographicSize = CurrentGameData.MaxCanvasY.Value;
    }

    private void Update()
    {
        if (ScoreCamera.transform.position.z != -CurrentGameData.LastDrawingLayerOrder)
        {
            Vector3 pos = ScoreCamera.transform.position;
            pos.z = -CurrentGameData.LastDrawingLayerOrder;
            ScoreCamera.transform.position = pos;
        }
    }
}
