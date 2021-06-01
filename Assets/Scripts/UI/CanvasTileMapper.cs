using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class CanvasTileMapper : MonoBehaviour
{
    public Tilemap CanvasTileMap;
    public Tile BlankTile;
    public GameData CurrentGameData;

    private void Start()
    {
        CurrentGameData.MaxCanvasX.OnValueChanged += valueChangedDelegate;
        CurrentGameData.MaxCanvasY.OnValueChanged += valueChangedDelegate;
        UpdateTileMap();
    }

    void valueChangedDelegate(int oldV, int newV)
    {
        UpdateTileMap();
    }
    void UpdateTileMap()
    {
        int maxX = CurrentGameData.MaxCanvasX.Value;
        int maxY = CurrentGameData.MaxCanvasY.Value;

        for (int x = -maxX; x < maxX; x++)
        {
            for (int y = -maxY; y < maxY; y++)
            {
                CanvasTileMap.SetTile(new Vector3Int(x, y, 0), BlankTile);
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
