using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class MinimapCameraController : MonoBehaviour, IPointerDownHandler, IDragHandler//, IPointerEnterHandler, IPointerExitHandler
{

    public RectTransform CameraMarker;
    public RectTransform MinimapContainer;
    public Camera MainCamera;
    public GameData CurrentGameData;
    public CameraMovements CameraMovementsController;

    private float worldToMinimapPoint;

    private void Start()
    {
        CurrentGameData.MaxCanvasX.OnValueChanged += valueChangedDelegate;
        CurrentGameData.MaxCanvasY.OnValueChanged += valueChangedDelegate;
        UpdateMinimapBorders();
    }

    void valueChangedDelegate(int oldV, int newV)
    {
        UpdateMinimapBorders();
    }

    public void OnDrag(PointerEventData eventData)
    {
        Vector2 localPoint = new Vector2();
        RectTransformUtility.ScreenPointToLocalPointInRectangle(MinimapContainer, eventData.position, null, out localPoint);
        
        localPoint = (localPoint + 150 * Vector2.one) / worldToMinimapPoint;
        MainCamera.transform.position = new Vector3(localPoint.x, localPoint.y, MainCamera.transform.position.z);
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        OnDrag(eventData);
    }

    /*public void OnPointerEnter(PointerEventData eventData)
    {
        CameraMovementsController.doMove = false;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        CameraMovementsController.doMove = true;
    }*/

    // Start is called before the first frame update
    void UpdateMinimapBorders()
    {
        if (CurrentGameData.MaxCanvasY.Value == 0)
        {
            worldToMinimapPoint = MinimapContainer.rect.height / (15 * 2);
            MinimapContainer.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, MinimapContainer.rect.height);

        }
        else
        {
            worldToMinimapPoint = MinimapContainer.rect.height / (CurrentGameData.MaxCanvasY.Value * 2);
            MinimapContainer.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, MinimapContainer.rect.height * (CurrentGameData.MaxCanvasX.Value / CurrentGameData.MaxCanvasY.Value));
        }
        float cameraWidthToWorld = (Camera.main.ScreenToWorldPoint(new Vector3(Screen.width, 0, 0)) - Camera.main.ScreenToWorldPoint(Vector3.zero)).x;
        float cameraHeightToWorld = (Camera.main.ScreenToWorldPoint(new Vector3(0, Screen.height, 0)) - Camera.main.ScreenToWorldPoint(Vector3.zero)).y;

        CameraMarker.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, cameraWidthToWorld * worldToMinimapPoint);
        CameraMarker.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, cameraHeightToWorld * worldToMinimapPoint);

    }

    // Update is called once per frame
    void Update()
    {
        float minimapNewX = MainCamera.transform.position.x * worldToMinimapPoint;
        float minimapNewY = MainCamera.transform.position.y * worldToMinimapPoint;
        CameraMarker.anchoredPosition = new Vector2(minimapNewX, minimapNewY);
    }
}
