using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class CameraMovements : MonoBehaviour
{
    public float cameraSensitivity = 0.005f;
    public float cameraMoveZone = 0.1f;
    public Camera movingCamera;
    public GameData CurrentGameData;
    public Transform ListenerTrans;
    public float border = 0.1f;
    public bool doMove = true;

    private int screenHeight;
    private int screenWidth;
    private float maxHeight;
    private float maxWidth;

    // Start is called before the first frame update
    private void Start()
    {
        CurrentGameData.MaxCanvasX.OnValueChanged += valueChangedDelegate;
        CurrentGameData.MaxCanvasY.OnValueChanged += valueChangedDelegate;
        UpdateCameraLimits();
    }

    void valueChangedDelegate(int oldV, int newV)
    {
        UpdateCameraLimits();
    }

    void UpdateCameraLimits()
    {

#if UNITY_EDITOR
        screenHeight = (int) Handles.GetMainGameViewSize().y;
        screenWidth = (int) Handles.GetMainGameViewSize().x;
#else
        screenWidth = Screen.width;
        screenHeight = Screen.height;
#endif

        float widthToRemove = (Camera.main.ScreenToWorldPoint(new Vector3(screenWidth, 0, 0)) - Camera.main.ScreenToWorldPoint(Vector3.zero)).x / 2;
        float heightToRemove = (Camera.main.ScreenToWorldPoint(new Vector3(0, screenHeight, 0)) - Camera.main.ScreenToWorldPoint(Vector3.zero)).y / 2;


        maxHeight = CurrentGameData.MaxCanvasY.Value - heightToRemove + border;
        maxWidth = CurrentGameData.MaxCanvasX.Value - widthToRemove + border;

    }

    public bool MouseInScreenCheck()
    {
        if (Input.mousePosition.x < 0 || Input.mousePosition.y < 0 || Input.mousePosition.x > screenWidth || Input.mousePosition.y > screenHeight) {
            return false;
        }
        else
        {
            return true;
        }
    }

    // Update is called once per frame
    void Update()
    {
        //moving camera since drawings keep getting on top of each other
        if (movingCamera.transform.position.z != -CurrentGameData.LastDrawingLayerOrder)
        {
            Vector3 pos = movingCamera.transform.position;
            pos.z = -CurrentGameData.LastDrawingLayerOrder;
            movingCamera.transform.position = pos;
            ListenerTrans.position = new Vector3(ListenerTrans.position.x, ListenerTrans.position.y, -10);
        }

        Vector3 mousePt = Input.mousePosition;

        float moveY = 0;
        float moveX = 0;
        if (MouseInScreenCheck() && doMove)
        {
            if (mousePt.x < screenWidth * cameraMoveZone)
                moveX -= cameraSensitivity;
            else if (mousePt.x > screenWidth * (1 - cameraMoveZone))
                moveX += cameraSensitivity;

            if (mousePt.y < screenHeight * cameraMoveZone)
                moveY -= cameraSensitivity;
            else if (mousePt.y > screenHeight * (1 - cameraMoveZone))
                moveY += cameraSensitivity;
        }

        Vector3 camPos = movingCamera.transform.position;
        camPos.x += moveX;
        camPos.y += moveY;

        camPos.y = Mathf.Max(Mathf.Min(camPos.y, maxHeight), -maxHeight);
        camPos.x = Mathf.Max(Mathf.Min(camPos.x, maxWidth), -maxWidth);

        movingCamera.transform.position = camPos;

        if (CurrentGameData.MaxCanvasY.Value == 0)
        {
            movingCamera.transform.position = new Vector3(0, 0, -10);
        }

    }
}
