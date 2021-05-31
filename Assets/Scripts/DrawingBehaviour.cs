using MLAPI;
using MLAPI.NetworkVariable;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DrawingBehaviour : NetworkBehaviour
{
    public NetworkVariableULong ID = new NetworkVariableULong();
    public NetworkVariableInt PlayerOwnerID = new NetworkVariableInt();
    public NetworkVariableColor32 MainColor = new NetworkVariableColor32(Color.red);
    public Transform Draw;
    public Transform FillImage;
    public Transform MarkerImage;
    public NetworkVariableVector2 P1 = new NetworkVariableVector2();
    public NetworkVariableVector2 P2 = new NetworkVariableVector2();
    public NetworkVariableFloat SpeedFill = new NetworkVariableFloat(6f);
    public float MarkerTransparency;
    public DrawingCollision DrawingCollider;
    public RectDrawer PlayerOwnerDrawer;
    public PlayerData PlayerOwner;
    public SoundDrawingBehaviour SoundBehaviour;
    public Animation DoneAnim;
    public SpriteRenderer BGAnim;

    public NetworkVariableFloat FillingValue = new NetworkVariableFloat(0f);
    public Slider.Direction FillingDirection;
    public NetworkVariableBool DoFilling = new NetworkVariableBool(false);
    public GameData CurrentGameData;

    public NetworkVariableBool IsDrawable = new NetworkVariableBool(false);

    public bool Done = false;
    private SpriteRenderer FillRenderer;
    private SpriteRenderer MarkerRenderer;
    private float LocalFillingValue = 0f;

    public void AddNetworkVariableListeners()
    {
        MainColor.OnValueChanged += SetColor;
        P2.OnValueChanged += SetPosition;
    }

    
    public void SetColor(Color32 prevColor, Color32 newColor)
    {
        FillRenderer.color = newColor;
        newColor.a = (byte) (255 * MarkerTransparency * (Done ? 0 : 1));
        MarkerRenderer.color = newColor;
    }

    public void SetPosition(Vector2 oldP2, Vector2 P2)
    {
        //calculate position and scale
        Vector3 scale = P1.Value - P2;
        scale.x = Mathf.Abs(scale.x);
        scale.y = Mathf.Abs(scale.y);
        scale.z = Draw.localScale.z;

        Vector3 center = (P1.Value + P2) * 0.5f;


        //calculate fill direction
        if (scale.x > scale.y) //fill horizontaly
        {
            if (P1.Value.x < P2.x) //fill right
            {
                FillingDirection = Slider.Direction.LeftToRight;
            } else //fill left
            {
                FillingDirection = Slider.Direction.RightToLeft;
            }
        } else //fill vertically
        {
            if (P1.Value.y < P2.y) //fill up
            {

                FillingDirection = Slider.Direction.BottomToTop;
            } else //fill down
            {
                FillingDirection = Slider.Direction.TopToBottom;
            }
        }

        Draw.position = center;
        Draw.localScale = scale;
    }

    public void UpdateFill(float newValue)
    {
        //updating Z pos to put fill on front 
        if (FillImage.position.z == 0)
        {
            FillImage.position = new Vector3(FillImage.position.x, FillImage.position.y, -CurrentGameData.LastDrawingLayerOrder);
            CurrentGameData.LastDrawingLayerOrder++;
        }

        Vector3 scaleFill = FillImage.localScale;
        Vector3 posFill = FillImage.localPosition;
        if (FillingDirection.Equals(Slider.Direction.BottomToTop) || FillingDirection.Equals(Slider.Direction.TopToBottom))
        {

            scaleFill.y = MarkerImage.localScale.y * newValue;
            scaleFill.x = MarkerImage.localScale.x;

            posFill.y = (FillingDirection.Equals(Slider.Direction.BottomToTop) ? -1 : 1) * (1 - newValue) / 2;
            posFill.x = 0;

        }
        else
        {
            scaleFill.x = MarkerImage.localScale.x * newValue;
            scaleFill.y = MarkerImage.localScale.y;

            posFill.x = (FillingDirection.Equals(Slider.Direction.LeftToRight) ? -1 : 1) * (1 - newValue) / 2;
            posFill.y = 0;
        }
        FillImage.localScale = scaleFill;
        FillImage.localPosition = posFill;

    }

    public void UpdateDrawing()
    {
        SetColor(MainColor.Value, MainColor.Value);
        SetPosition(P2.Value, P2.Value);
        UpdateFill(FillingValue.Value);
    }

    private void SetPlayerOwnerAndDrawer()
    {
        GameObject playerOwnerGameObject = CurrentGameData.Players[PlayerOwnerID.Value];
        PlayerOwnerDrawer = playerOwnerGameObject.GetComponent<RectDrawer>();
        PlayerOwner = playerOwnerGameObject.GetComponent<PlayerData>();
    }


    void Awake()
    {
        AddNetworkVariableListeners();
        CurrentGameData = GameObject.Find("GameManager").GetComponent<GameData>();
        GameOptions currentOptions = GameObject.Find("GameManager").GetComponent<GameOptions>();
        SoundBehaviour.SetGameOptions(currentOptions);
        SetPlayerOwnerAndDrawer();
        FillRenderer = FillImage.GetComponent<SpriteRenderer>();
        MarkerRenderer = MarkerImage.GetComponent<SpriteRenderer>();

        UpdateDrawing();
    }

    // Update is called once per frame
    void Update()
    {
        //Update color if server changed it
        if (!MainColor.Value.Equals(FillRenderer.color))
        {
            UpdateDrawing();
        }

        //update marker color if not drawable
        if (!IsDrawable.Value)
        {
            //set as red
            MarkerRenderer.color = new Color32(255, 180, 180, 125);
        } else if (MarkerRenderer.color.Equals(new Color32(255, 180, 180, 125)))
        {
            SetColor(MainColor.Value, MainColor.Value);
        }

        //check if drawing is done (only server can check)
        if (IsServer)
            DoFilling.Value = DoFilling.Value && FillingValue.Value < 1f;

        //do stuff if not already done after finished drawing
        if (!DoFilling.Value && FillingValue.Value > 0f && !Done)
        {
            //remove marker
            Color MarkerColor = MarkerRenderer.color;
            BGAnim.color = MarkerColor;
            MarkerColor.a = 0;
            MarkerRenderer.color = MarkerColor;
            //play animation
            DoneAnim.Play();


            //place it behind currently filling drawings
            MarkerRenderer.sortingLayerName = "Drawings";
            FillRenderer.sortingLayerName = "Drawings";

            //disable collision
            DrawingCollider.DoCollide = false;

            //put at last sibling (to draw on top of others)
            MarkerRenderer.sortingOrder = CurrentGameData.LastDrawingLayerOrder;
            FillRenderer.sortingOrder = CurrentGameData.LastDrawingLayerOrder;

            CurrentGameData.LastDrawingLayerOrder++;

            //set as done
            Done = true;
        }

        if (IsServer)
        {
            if (DoFilling.Value && FillingValue.Value < 1f)
            {
                float newFillingValue = FillingValue.Value + SpeedFill.Value * Time.deltaTime / (Draw.localScale.x * Draw.localScale.y);

                FillingValue.Value = Mathf.Min(newFillingValue, 1f);
                UpdateFill(FillingValue.Value);
            }
        } else
        {
            if (DoFilling.Value && LocalFillingValue < 1f)
            {
                float newLocalFillingValue = LocalFillingValue + SpeedFill.Value * Time.deltaTime / (Draw.localScale.x * Draw.localScale.y);

                LocalFillingValue = Mathf.Min(newLocalFillingValue, 1f);
                UpdateFill(newLocalFillingValue);
            }
        }

    }
}
