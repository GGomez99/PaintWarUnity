using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EndRound : MonoBehaviour
{
    public GameObject TextWin;
    public GameData CurrentGame;
    public Text WinnerText;
    public Animation EndRoundAnim;
    private bool roundEnded;

    private void Start()
    {
        roundEnded = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (!roundEnded && CurrentGame.GameEnded.Value)
        {
            TextWin.SetActive(true);
            roundEnded = true;
            WinnerText.color = Color.Lerp(CurrentGame.Winner.Value, Color.black, 0.15f);
            int teamNumber = CurrentGame.Teams.IndexOf(CurrentGame.Winner.Value) + 1;
            WinnerText.text = "Team " + teamNumber.ToString();
            EndRoundAnim.Play();
        }
        else if (CurrentGame.GameStarted.Value && roundEnded)
        {
            TextWin.SetActive(false);
            roundEnded = false;
        }
    }
}
