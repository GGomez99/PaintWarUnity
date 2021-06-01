using MLAPI;
using MLAPI.Messaging;
using MLAPI.NetworkVariable;
using MLAPI.NetworkVariable.Collections;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GameData : NetworkBehaviour
{
    public ScoreCalculator Calculator;
    public InkbarDisplay InkDisplay;
    public OptionsFileManager OptionsManager;
    public List<Color32> PossibleColors;
    public NetworkList<Color32> Teams = new NetworkList<Color32>();
    public NetworkDictionary<string, int> Scores = new NetworkDictionary<string, int>();
    public NetworkList<GameObject> Players = new NetworkList<GameObject>();
    public GameObject LocalPlayer;
    public int LastDrawingLayerOrder = 1;
    public NetworkVariableBool GameStarted = new NetworkVariableBool(false);
    public NetworkVariableBool GameEnded = new NetworkVariableBool(false);
    public NetworkVariableInt GameTime = new NetworkVariableInt(0);
    public NetworkVariableColor32 Winner = new NetworkVariableColor32();


    public NetworkVariableInt MaxCanvasX = new NetworkVariableInt();
    public NetworkVariableInt MaxCanvasY = new NetworkVariableInt();
    public NetworkVariableFloat MinimalDrawingLength = new NetworkVariableFloat();
    public NetworkVariableFloat BaseSpeedFillingPerSecond = new NetworkVariableFloat();
    public NetworkVariableFloat BaseInk = new NetworkVariableFloat();
    public NetworkVariableFloat InkPerTeamPixel = new NetworkVariableFloat();
    public NetworkVariableFloat BaseInkRegenPerSecond = new NetworkVariableFloat();
    public NetworkVariableFloat InkRegenPerTeamPixel = new NetworkVariableFloat();
    public NetworkVariableFloat InkToAreaPaintRatio = new NetworkVariableFloat();
    public NetworkVariableInt RoundTime = new NetworkVariableInt();
    public int MaxTeamNumber = 0;

    //used by server only
    private int lastPlayerID = 0;
    private List<Color32> TeamColorsLeft;
    private List<int> TeamCount;
    private bool teamsAreInit;


    void InitialiseTeamList()
    {
        //load round options
        OptionsManager.LoadGameRoundOptions();

        TeamColorsLeft = new List<Color32>();
        TeamColorsLeft = PossibleColors.OrderBy(a => Random.value).ToList();
        if (MaxTeamNumber > 0)
        {
            TeamCount = new List<int>(new int[MaxTeamNumber]);

            for (int i = 0; i < MaxTeamNumber; i++)
            {
                Color32 newTeam = AddTeam();
                TeamCount[i] = 0;
            }
        }
    }

    //server only but can't rpc since gameobject doesn't support serialization
    public void AddPlayer(GameObject newPlayer)
    {
        if (IsServer)
        {
            if (!teamsAreInit)
            {
                InitialiseTeamList();
                teamsAreInit = true;
            }

            Players.Add(newPlayer);

            PlayerData data = newPlayer.GetComponent<PlayerData>();

            //if no max team number, create team for each player
            if (MaxTeamNumber == 0)
            {
                Color32 playerColor = AddTeam();
                data.TeamColor.Value = playerColor;

            //else assign it to the team which has the least amount of players
            } else
            {
                int minTeamIndex = TeamCount.IndexOf(TeamCount.Min());
                data.TeamColor.Value = Teams[minTeamIndex];
                TeamCount[minTeamIndex]++;
            }

            data.PlayerID.Value = lastPlayerID;
            data.InkRegen.Value = BaseInkRegenPerSecond.Value;
            data.MaxInk.Value = (int) BaseInk.Value;
            data.Ink.Value = BaseInk.Value;

            lastPlayerID++;
        }
    }

    public void SetLocalPlayer(GameObject player)
    {
        LocalPlayer = player;
        InkDisplay.LocalPlayer = player.GetComponent<PlayerData>();
        InkDisplay.LocalDrawer = player.GetComponent<RectDrawer>();
        InkDisplay.UpdateColor();
    }


    //server only but can't use rpc since returning value
    public Color32 AddTeam()
    {
        if (IsServer)
        {
            Color32 newTeam;
            if (TeamColorsLeft.Count > 0)
            {
                int teamIndex = Random.Range(0, TeamColorsLeft.Count);
                newTeam = TeamColorsLeft[teamIndex];
                TeamColorsLeft.RemoveAt(teamIndex);
            } else
            {
                newTeam = Random.ColorHSV();
                newTeam = Color.Lerp(newTeam, Color.white, 0.5f);
                newTeam.a = 255;
            }

            Teams.Add(newTeam);
            Calculator.UpdateTeamListServerRpc();

            return newTeam;
        } else
        {
            print("client trying to generate team");
            return new Color32();
        }

    }

    [ServerRpc]
    public void StartGameServerRpc()
    {
        //load round options
        OptionsManager.LoadGameRoundOptions();

        //reset team scores
        Scores.Clear();

        //reset player stats
        foreach (GameObject player in Players)
        {
            PlayerData data = player.GetComponent<PlayerData>();

            data.PlayerID.Value = lastPlayerID;
            data.InkRegen.Value = BaseInkRegenPerSecond.Value;
            data.MaxInk.Value = (int)BaseInk.Value;
            data.Ink.Value = BaseInk.Value;
        }

        //destroying all drawings
        GameObject[] drawings = GameObject.FindGameObjectsWithTag("Drawing");
        foreach (GameObject drawing in drawings)
        {
            Destroy(drawing);
        }

        GameStarted.Value = true;
        GameEnded.Value = false;

        Invoke("StartGameForRealServerRpc", 3.3f);
     
    }

    [ServerRpc]
    private void StartGameForRealServerRpc()
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
        GameTime.Value = RoundTime.Value;

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
            float newMaxInk = Scores[ColorUtility.ToHtmlStringRGB(data.TeamColor.Value)] * InkPerTeamPixel.Value + BaseInk.Value;

            data.MaxInk.Value = (int) newMaxInk;
            data.InkRegen.Value = Scores[ColorUtility.ToHtmlStringRGB(data.TeamColor.Value)] * InkRegenPerTeamPixel.Value + BaseInkRegenPerSecond.Value;

        }
    }
}
