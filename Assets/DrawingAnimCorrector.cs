using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DrawingAnimCorrector : MonoBehaviour
{
    public Transform drawing;
    public Transform BGAnim;
    public Transform BG;

    // Update is called once per frame
    void Update()
    {
        if (drawing.localScale.x > 0 && drawing.localScale.y > 0)
        {
            float increase = BG.transform.localScale.z - 1;
            BGAnim.localScale = new Vector3(1 + increase / drawing.localScale.x, 1 + increase / drawing.localScale.y, 1);
        }
    }
}
