using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DrawingCollision : MonoBehaviour
{

    public DrawingBehaviour CurrentDrawing;
    public bool DoCollide;

    void Start()
    {
        DoCollide = true;
    }

    private void OnMouseDown()
    {
        GameObject localPlayer = CurrentDrawing.CurrentGameData.LocalPlayer;

        if (CurrentDrawing.MainColor.Value.Equals(localPlayer.GetComponent<PlayerData>().TeamColor.Value))
        {
            localPlayer.GetComponent<RectDrawer>().GenerateDrawing();
        }
    }

    void OnCollisionEnter2D(Collision2D coll)
    {
        DrawingCollision otherDrawingCollision = coll.collider.gameObject.GetComponent<DrawingCollision>();

        if (DoCollide && otherDrawingCollision.DoCollide)
        {
            DrawingBehaviour OtherDrawing = otherDrawingCollision.CurrentDrawing;

            //checking if from same team or not
            if (!OtherDrawing.MainColor.Value.Equals(CurrentDrawing.MainColor.Value))
            {
                Vector2 v = coll.GetContact(0).normal;

                if (Mathf.Abs(Vector2.Angle(v, -transform.up)) <= 45)
                {
                    //Debug.Log("Collision on top");
                    if (CurrentDrawing.FillingDirection.Equals(Slider.Direction.BottomToTop))
                    {
                        CurrentDrawing.DoFilling.Value = false;
                    }
                }
                else if (Mathf.Abs(Vector2.Angle(v, -transform.right)) <= 45)
                {
                    //Debug.Log("Collision on right");
                    if (CurrentDrawing.FillingDirection.Equals(Slider.Direction.LeftToRight))
                    {
                        CurrentDrawing.DoFilling.Value = false;
                    }
                }
                else if (Mathf.Abs(Vector2.Angle(v, transform.right)) <= 45)
                {
                    //Debug.Log("Collision on Left");
                    if (CurrentDrawing.FillingDirection.Equals(Slider.Direction.RightToLeft))
                    {
                        CurrentDrawing.DoFilling.Value = false;
                    }
                }
                else
                {
                    //Debug.Log("Collision on Bottom");
                    if (CurrentDrawing.FillingDirection.Equals(Slider.Direction.TopToBottom))
                    {
                        CurrentDrawing.DoFilling.Value = false;
                    }
                }
            }
        }
    }
}
