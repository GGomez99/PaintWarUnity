using MLAPI;
using MLAPI.NetworkVariable;
using MLAPI.NetworkVariable.Collections;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameData : NetworkBehaviour
{
    public ScoreCalculator calculator;
    public NetworkList<Color32> Teams = new NetworkList<Color32>();
    public NetworkDictionary<string, int> Scores = new NetworkDictionary<string, int>();
    public NetworkList<GameObject> Players = new NetworkList<GameObject>();
    public GameObject LocalPlayer;
    public int LastDrawingLayerOrder = 0;
    public NetworkVariableBool GameStarted = new NetworkVariableBool(false);
    public NetworkVariableBool GameEnded = new NetworkVariableBool(false);
    public NetworkVariableInt GameTime = new NetworkVariableInt(0);
    public NetworkVariableColor32 Winner = new NetworkVariableColor32();

    public InkbarDisplay InkDisplay;

    public int MaxCanvasX;
    public int MaxCanvasY;
    public float MinimalDrawingLength = 0.5f;
    public float BaseSpeedFillingPerSecond = 6f;
    public float BaseInk = 100f;
    public float InkPerTeamPixel = 0.001f;
    public float BaseInkRegenPerSecond = 17f;
    public float InkRegenPerTeamPixel = 0.000001f;
    public float InkToAreaPaintRatio = 0.06f;
    public int RoundTime = 300;

    //used by server only
    private int lastPlayerID = 0;

    public void AddPlayer(GameObject newPlayer)
    {
        if (IsServer)
        {
            print("gamedata server adding player");
            Players.Add(newPlayer);

            PlayerData data = newPlayer.GetComponent<PlayerData>();

            data.PlayerID.Value = lastPlayerID;
            data.InkRegen.Value = BaseInkRegenPerSecond;
            data.MaxInk.Value = (int) BaseInk;
            data.Ink.Value = BaseInk;

            lastPlayerID++;
        }
    }

    public void SetLocalPlayer(GameObject player)
    {
        LocalPlayer = player;
        InkDisplay.LocalPlayer = player.GetComponent<PlayerData>();
        InkDisplay.UpdateColor();
    }

    public Color32 AddTeam()
    {
        if (IsServer)
        {
            Color32 newTeam = Random.ColorHSV();
            newTeam = Color.Lerp(newTeam, Color.white, 0.5f);
            newTeam.a = 255;

            print("gamedata server adding team");
            Teams.Add(newTeam);
            print("gamedata server updating score list");
            calculator.UpdateTeamList();

            return newTeam;
        } else
        {
            print("client trying to generate team");
            return new Color32();
        }

    }

    public void StartGame()
    {
        if (IsServer)
        {
            GameStarted.Value = true;
            GameEnded.Value = false;

            //reset data
            Scores.Clear();

            //reset player stats
            foreach (GameObject player in Players)
            {
                PlayerData data = player.GetComponent<PlayerData>();

                data.PlayerID.Value = lastPlayerID;
                data.InkRegen.Value = BaseInkRegenPerSecond;
                data.MaxInk.Value = (int)BaseInk;
                data.Ink.Value = BaseInk;
            }

            //destroying all drawings
            GameObject[] drawings = GameObject.FindGameObjectsWithTag("Drawing");
            foreach (GameObject drawing in drawings)
            {
                Destroy(drawing);
            }


            Invoke("StartGameForReal", 3.3f);
        }
    }

    private void StartGameForReal()
    {

        //generate zones
        foreach (GameObject player in Players)
        {
            RectDrawer playerDrawer = player.GetComponent<RectDrawer>();
            PlayerData playerData = player.GetComponent<PlayerData>();
            playerDrawer.GenerateTeamZone();
            playerData.StartUpdatingInk();
        }

        //setup timer
        GameTime.Value = RoundTime;

        //start timer
        InvokeRepeating("TickCountdown", 0f, 1f);

    }

    private void TickCountdown()
    {
        GameTime.Value -= 1;
        if (GameTime.Value == 0)
        {
            EndGame();
        }
    }

    private void EndGame()
    {
        CancelInvoke("TickCountdown");
        GameTime.Value = 0;

        float maxScore = 0f;
        Color32 teamWinner = new Color32();
        foreach(string team in Scores.Keys)
        {
            if (Scores[team] > maxScore)
            {
                maxScore = Scores[team];
                Color teamWinnerColor = new Color();
                ColorUtility.TryParseHtmlString("#"+team, out teamWinnerColor);
                teamWinner = teamWinnerColor;
            }
        }

        Winner.Value = teamWinner;
        GameEnded.Value = true;
        GameStarted.Value = false;
    }

    public void UpdatePlayersInk()
    {
        foreach (GameObject Player in Players)
        {
            PlayerData data = Player.GetComponent<PlayerData>();
            float newMaxInk = Scores[ColorUtility.ToHtmlStringRGB(data.TeamColor.Value)] * InkPerTeamPixel + BaseInk;

            data.MaxInk.Value = (int) newMaxInk;
            data.InkRegen.Value = Scores[ColorUtility.ToHtmlStringRGB(data.TeamColor.Value)] * InkRegenPerTeamPixel + BaseInkRegenPerSecond;

        }
    }
}
