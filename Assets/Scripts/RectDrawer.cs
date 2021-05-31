using MLAPI;
using MLAPI.Connection;
using MLAPI.Messaging;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class RectDrawer : NetworkBehaviour
{
    public GameObject DrawingTemplate;
    public GameObject CanvasDrawings;
    public PlayerData DrawerPlayer;
    public GameData CurrentGameData;
    public DrawingBehaviour CurrentDisplayDrawing;
    private int CurrentDrawingID = -1;

    //used by server
    private int drawingNextID = 0;
    private Dictionary<int, DrawingBehaviour> Drawings = new Dictionary<int, DrawingBehaviour>();
    
    int CreateDrawing(Vector2 pt1, Vector2 pt2, Color32 drawColor, bool startFilling = false)
    {
        //create drawing
        GameObject newDrawing = Instantiate(DrawingTemplate, Vector3.zero, Quaternion.identity, CanvasDrawings.transform);
        DrawingBehaviour newDrawingBehav = newDrawing.GetComponent<DrawingBehaviour>();
        newDrawing.GetComponent<NetworkObject>().Spawn();
        newDrawingBehav.MainColor.Value = drawColor;
        newDrawingBehav.P1.Value = pt1;
        newDrawingBehav.P2.Value = pt2;
        newDrawingBehav.ID.Value = ++drawingNextID;
        newDrawingBehav.PlayerOwnerID.Value = DrawerPlayer.PlayerID.Value;
        newDrawingBehav.SpeedFill.Value = CurrentGameData.BaseSpeedFillingPerSecond;
        if (!pt2.Equals(pt1))
        {
            newDrawingBehav.IsDrawable.Value = true;
            newDrawingBehav.UpdateDrawing();
        }
        newDrawingBehav.DoFilling.Value = startFilling;

        Drawings.Add(newDrawingBehav.ID.Value, newDrawingBehav);

        return newDrawingBehav.ID.Value;
    }

    void UpdateDrawingP2(int ID, Vector2 pt)
    {
        if (Drawings.ContainsKey(ID))
        {
            Vector3 drawScale = Drawings[ID].Draw.localScale;
            float area = drawScale.x * drawScale.y;

            if (drawScale.x < CurrentGameData.MinimalDrawingLength || drawScale.y < CurrentGameData.MinimalDrawingLength || area > DrawerPlayer.Ink.Value * CurrentGameData.InkToAreaPaintRatio)
            {
                Drawings[ID].IsDrawable.Value = false;
            } else
            {
                Drawings[ID].IsDrawable.Value = true;
            }

            Drawings[ID].P2.Value = pt;
        }
        else
            print("Drawing not found");
    }

    void StartFillingDrawing(int ID)
    {
        if (Drawings.ContainsKey(ID))
        {
            if (Drawings[ID].IsDrawable.Value)
            {
                Vector3 drawScale = Drawings[ID].Draw.localScale;
                float area = drawScale.x * drawScale.y;

                Drawings[ID].DoFilling.Value = true;
                DrawerPlayer.Ink.Value -= area / CurrentGameData.InkToAreaPaintRatio;
            }
            else
            {
                Drawings[ID].gameObject.GetComponent<NetworkObject>().Despawn(true);
                Drawings.Remove(ID);
            }
        }
        else
            print("Drawing not found");
    }

    [ServerRpc]
    void CreateDrawingServerRpc(Vector2 pt, Color drawColor, ulong clientID, ServerRpcParams rpcParams = default)
    {
        int newID = CreateDrawing(pt, pt, drawColor);
        ClientRpcParams clientRpcParams = new ClientRpcParams
        {
            Send = new ClientRpcSendParams
            {
                TargetClientIds = new ulong[] { clientID }
            }
        };

        UpdateCurrentDrawingIDClientRpc(newID, clientRpcParams);
    }

    [ServerRpc]
    void CreateDrawingServerRpc(Vector2 pt1, Vector2 pt2, Color drawColor, ulong clientID, ServerRpcParams rpcParams = default)
    {
        int newID = CreateDrawing(pt1, pt2, drawColor, true);
    }

    [ServerRpc]
    void UpdateDrawingP2ServerRpc(int ID, Vector2 pt, ServerRpcParams rpcParams = default)
    {
        UpdateDrawingP2(ID, pt);
    }

    [ServerRpc]
    void StartFillingDrawingServerRpc(int ID, ServerRpcParams rpcParams = default)
    {
        StartFillingDrawing(ID);
    }

    [ClientRpc]
    void UpdateCurrentDrawingIDClientRpc(int newID, ClientRpcParams clientRpcParams = default)
    {
        CurrentDrawingID = newID;
    }

    public override void NetworkStart()
    {
        //set where to draw stuff
        CanvasDrawings = GameObject.Find("Grid");
        CurrentGameData = GameObject.Find("GameManager").GetComponent<GameData>();

        //generate team for player
        if (IsServer) {
            //add player to list (generates player ID too)
            CurrentGameData.AddPlayer(gameObject);

            //generate team color
            Color32 newTeam = CurrentGameData.AddTeam();
            DrawerPlayer.TeamColor.Value = newTeam;
        }

        //mark player as local player
        if (IsLocalPlayer)
        {
            CurrentGameData.SetLocalPlayer(gameObject);
        }
    }

    public void GenerateTeamZone()
    {
        if (IsServer)
        {
            //generate player square
            float randomX = Random.value * 18 - 9;
            float randomY = Random.value * 18 - 9;
            CreateDrawing(new Vector2(randomX, randomY), new Vector2(randomX + 1, randomY + 1), DrawerPlayer.TeamColor.Value, true);
        } else
        {
            print("trying to generate team zone from client");
        }
    }

    public void GenerateDrawing()
    {
        Vector3 pz = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        pz.z = 0;

        if (IsServer)
        {
            CurrentDrawingID = CreateDrawing(pz, pz, DrawerPlayer.TeamColor.Value);
        }
        else if (IsClient)
        {
            CurrentDrawingID = -2;
            CreateDrawingServerRpc(pz, DrawerPlayer.TeamColor.Value, OwnerClientId);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (IsLocalPlayer && !CurrentGameData.GameEnded.Value)
        {
            if (Input.GetMouseButtonDown(0) && CurrentDrawingID == -1)
            {
                int layerMask = 1 << LayerMask.NameToLayer("DrawingHitboxes");
                Collider2D colliderHit = Physics2D.OverlapPoint(Camera.main.ScreenToWorldPoint(Input.mousePosition), layerMask);
                if (colliderHit != null)
                {
                    //check if clicking on UI
                    if (!EventSystem.current.IsPointerOverGameObject())
                    {

                        DrawingBehaviour drawingHit = colliderHit.gameObject.GetComponent<DrawingCollision>().CurrentDrawing;

                        if (drawingHit.MainColor.Value.Equals(DrawerPlayer.TeamColor.Value))
                        {
                            GenerateDrawing();
                        }
                    }

                }
            } 
            
            else if (Input.GetMouseButton(0) && CurrentDrawingID > -1)
            {
                //update draw marker
                Vector3 pz = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                pz.z = 0;
                pz.x = Mathf.Max(Mathf.Min(pz.x, CurrentGameData.MaxCanvasX), -CurrentGameData.MaxCanvasX);
                pz.y = Mathf.Max(Mathf.Min(pz.y, CurrentGameData.MaxCanvasY), -CurrentGameData.MaxCanvasY);


                if (IsServer)
                    UpdateDrawingP2(CurrentDrawingID, pz);
                else if (IsClient)
                    UpdateDrawingP2ServerRpc(CurrentDrawingID, pz);
            }

            else if (Input.GetMouseButtonUp(0) && CurrentDrawingID > -1)
            {
                //start filling
                if (IsServer)
                    StartFillingDrawing(CurrentDrawingID);
                else if (IsClient)
                    StartFillingDrawingServerRpc(CurrentDrawingID);
                CurrentDrawingID = -1;
            }
        }
    }
}
