using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class CanvasTileMapper : MonoBehaviour
{
    public Tilemap CanvasTileMap;
    public Tile BlankTile;
    public GameData CurrentGameData;


    void Start()
    {
        int maxX = CurrentGameData.MaxCanvasX;
        int maxY = CurrentGameData.MaxCanvasY;

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
