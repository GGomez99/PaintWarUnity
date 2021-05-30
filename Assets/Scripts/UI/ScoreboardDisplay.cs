using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ScoreboardDisplay : MonoBehaviour
{
    public GameData CurrentGameData;
    public RectTransform ScoreboardContainer;
    public GameObject TeamBar;
    private Dictionary<string, RectTransform> teamsInScoreboard = new Dictionary<string, RectTransform>();

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        int totalScore = 0;
        Dictionary<string, RectTransform> newTeamsInScoreboard = new Dictionary<string, RectTransform>();

        //get total score and generate team color bar if needed
        foreach (string team in CurrentGameData.Scores.Keys)
        {
            int teamScore = CurrentGameData.Scores[team];

            //print(team + " with score " + teamScore);
            if (teamScore > 0) {
                totalScore += teamScore;
                if (!teamsInScoreboard.ContainsKey(team))
                {
                    GameObject newTeamBar = Instantiate(TeamBar, ScoreboardContainer);

                    Image TeamBarImage = newTeamBar.GetComponent<Image>();
                    Color parsedTeamColor = new Color();
                    ColorUtility.TryParseHtmlString("#"+team, out parsedTeamColor);
                    TeamBarImage.color = parsedTeamColor;

                    newTeamBar.transform.SetAsLastSibling();
                    RectTransform rectTrans = newTeamBar.GetComponent<RectTransform>();

                    newTeamsInScoreboard.Add(team, rectTrans);
                }
                else
                {
                    //adding to new list and removing from old
                    newTeamsInScoreboard.Add(team, teamsInScoreboard[team]);
                    teamsInScoreboard.Remove(team);
                }
            }
        }

        //removing old teams that are not in scores anymore
        foreach (string team in teamsInScoreboard.Keys)
        {
            Destroy(teamsInScoreboard[team].gameObject);
        }
        //Update team list
        teamsInScoreboard = newTeamsInScoreboard;

        float lastPercent = 0;
        foreach (string team in teamsInScoreboard.Keys)
        {
            RectTransform rectTrans = newTeamsInScoreboard[team];
            float teamPercent = CurrentGameData.Scores[team] / (float) totalScore;

            rectTrans.anchorMin = new Vector2(lastPercent, 0);
            lastPercent += teamPercent;
            rectTrans.anchorMax = new Vector2(lastPercent, 1);
        }
    }
}
