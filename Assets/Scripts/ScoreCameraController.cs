using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScoreCameraController : MonoBehaviour
{
    public GameData CurrentGameData;
    public Camera ScoreCamera;
    public RenderTexture ScoreCamText;
    public int MaxTextResolution = 1080;

    // Start is called before the first frame update
    void Start()
    {

        if (CurrentGameData.MaxCanvasY >= CurrentGameData.MaxCanvasX)
        {
            ScoreCamText.width = MaxTextResolution * CurrentGameData.MaxCanvasX / CurrentGameData.MaxCanvasY;
            ScoreCamText.height = MaxTextResolution;
        }
        else
        {
            ScoreCamText.height = MaxTextResolution * CurrentGameData.MaxCanvasY / CurrentGameData.MaxCanvasX;
            ScoreCamText.width = MaxTextResolution;
        }

        ScoreCamera.orthographicSize = CurrentGameData.MaxCanvasY;
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
