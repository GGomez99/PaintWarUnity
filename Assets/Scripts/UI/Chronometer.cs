using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Chronometer : MonoBehaviour
{
    public GameData CurrentGameData;
    public Text ChronoLabel;

    private Color32 red = new Color32(250, 64, 74, 255);
    private bool doAlert;

    void TimeAlert()
    {
        if (ChronoLabel.color.Equals(red))
            ChronoLabel.color = Color.black;
        else
            ChronoLabel.color = red;
    }

    // Update is called once per frame
    void Update()
    {
        int roundTime = CurrentGameData.GameTime.Value;

        if (roundTime == 0 || CurrentGameData.GameEnded.Value || !CurrentGameData.GameStarted.Value)
        {
            ChronoLabel.text = "";
        }
        else
        {

            if (roundTime > 30)
            {
                if (doAlert)
                {
                    doAlert = false;
                    CancelInvoke("TimeAlert");
                    ChronoLabel.color = Color.black;
                }
            }
            else if (roundTime < 30 && !doAlert)
            {
                InvokeRepeating("TimeAlert", 0f, 0.75f);
                doAlert = true;
            }

            TimeSpan chronoTime = new TimeSpan(TimeSpan.TicksPerSecond * CurrentGameData.GameTime.Value);
            ChronoLabel.text = chronoTime.ToString(@"m\:ss");

        }


        
    }
}
