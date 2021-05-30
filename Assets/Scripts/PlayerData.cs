using MLAPI;
using MLAPI.NetworkVariable;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerData : NetworkBehaviour
{
    public NetworkVariableInt PlayerID = new NetworkVariableInt();
    public NetworkVariableColor32 TeamColor = new NetworkVariableColor32();
    public NetworkVariableFloat Ink = new NetworkVariableFloat();
    public NetworkVariableInt MaxInk = new NetworkVariableInt();
    public NetworkVariableFloat InkRegen = new NetworkVariableFloat();

    // Start is called before the first frame update
    public void StartUpdatingInk()
    {
        InvokeRepeating("UpdateInk", 1f, 0.1f);
    }

    public void StopUpdatingInk()
    {
        CancelInvoke("UpdateInk");
    }

    // Update is called once per frame
    void UpdateInk()
    {
        Ink.Value = Mathf.Min(Ink.Value + InkRegen.Value/10, MaxInk.Value);
    }
}
