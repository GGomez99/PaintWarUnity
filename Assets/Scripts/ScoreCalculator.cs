using MLAPI;
using MLAPI.Messaging;
using MLAPI.NetworkVariable.Collections;
using System.Collections;
using System.Collections.Generic;
using Unity.Jobs;
using UnityEngine;

public class ScoreCalculator : NetworkBehaviour
{
    public RenderTexture DrawZone;
    public GameData CurrentGameData;
    public bool DoDisplay;
    public int SkipPixels = 10;
    private Texture2D CurrentZone;

    // Start is called before the first frame update
    override public void NetworkStart()
    {
        if (IsServer)
        {
            
            CurrentZone = new Texture2D(DrawZone.width, DrawZone.height, TextureFormat.RGB24, false);

            InvokeRepeating("getScore", 1f, 5f);
        } else if (IsClient)
        {
            //CurrentGameData.Scores.OnDictionaryChanged += DebugScore;
            //InvokeRepeating("DisplayScore", 1f, 2f);
        }
    }

    [ServerRpc]
    public void UpdateTeamListServerRpc()
    {
        if (IsServer)
        {
            foreach (Color32 teamColor in CurrentGameData.Teams)
            {
                bool notInScores = true;
                
                foreach (string colorInScores in CurrentGameData.Scores.Keys)
                {
                    if (colorInScores.Equals(ColorUtility.ToHtmlStringRGB(teamColor)))
                        notInScores = false;    
                }

                if (notInScores)
                {
                    CurrentGameData.Scores.Add(ColorUtility.ToHtmlStringRGB(teamColor), 0);
                }
            }
        } else
        {
            print("client trying to add a team to scores");
        }
    }

    void DebugScore(NetworkDictionaryEvent<string, int> changeEvent)
    {
        print("key changed : " + changeEvent.Key);
        print("value changed : " + changeEvent.Value);

    }

    void DisplayScore()
    {
        if (DoDisplay) 
        {
            foreach (string teamColor in CurrentGameData.Scores.Keys)
            {
                print(teamColor + " : " + CurrentGameData.Scores[teamColor]);
                int scoreRecov;
                print(CurrentGameData.Scores.TryGetValue(teamColor, out scoreRecov));
                print("recovered " + scoreRecov);
            }
        }
    }

    void getScore()
    {
        if (IsServer)
        {
            RenderTexture.active = DrawZone;

            //print(Time.realtimeSinceStartup + " Start Reading pixels");
            CurrentZone.ReadPixels(new Rect(0, 0, DrawZone.width, DrawZone.height), 0, 0);
            CurrentZone.Apply();
            //print(Time.realtimeSinceStartup + " pixel read");

            RenderTexture.active = null;

            //generate dict for new score
            Dictionary<string, int> newScores = new Dictionary<string, int>();
            foreach (Color32 teamColor in CurrentGameData.Teams)
            {
                newScores.Add(ColorUtility.ToHtmlStringRGB(teamColor), 0);
            }

            //count pixels for each team
            //print(Time.realtimeSinceStartup + " start counting pixels");
            for (int x = 0; x < DrawZone.width; x = x + SkipPixels)
            {
                for (int y = 0; y < DrawZone.height; y = y + SkipPixels)
                {
                    Color32 pixelColor = CurrentZone.GetPixel(x, y);
                    if (CurrentGameData.Teams.Contains(pixelColor))
                    {
                        newScores[ColorUtility.ToHtmlStringRGB(pixelColor)] += SkipPixels;
                    }
                }
            }

            //update networkDict
            foreach (Color32 teamInScore in CurrentGameData.Teams)
            {
                string teamKey = ColorUtility.ToHtmlStringRGB(teamInScore);
                CurrentGameData.Scores[teamKey] = newScores[teamKey];
            }

            DisplayScore();
            CurrentGameData.UpdatePlayersInk();
        }
        //print(Time.realtimeSinceStartup + " done counting");

        
    }

}

/*public struct ScoreJob : IJob
{
    public Texture2D CurrentZone;
    public GameData CurrentGameData;
    public RenderTexture DrawZone;
    public Dictionary<Color32, int> Scores;

    public void Execute()
    {
        RenderTexture.active = DrawZone;

        CurrentZone.ReadPixels(new Rect(0, 0, DrawZone.width, DrawZone.height), 0, 0);
        CurrentZone.Apply();

        RenderTexture.active = null;

        for (int x = 0; x < DrawZone.width; x++)
        {
            for (int y = 0; y < DrawZone.height; y++)
            {
                Color32 pixelColor = CurrentZone.GetPixel(x, y);

                if (CurrentGameData.Teams.Contains(pixelColor))
                {
                    Scores[pixelColor] += 1;
                }
            }
        }

        foreach (Color32 teamColor in CurrentGameData.Teams)
        {
            Debug.Log(teamColor + " : " + Scores[teamColor]);
        }
    }
}
*/