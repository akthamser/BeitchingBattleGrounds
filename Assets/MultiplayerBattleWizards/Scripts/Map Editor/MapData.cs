using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class MapData
{
    public string mapName;          // Name of the map.
    public MapTileData[] tiles;     // Data for all tiles in the map.
    public Vector3[] spawnPoints;   // Array of all spawn positions.

    public MapData (string mapName)
    {
        this.mapName = mapName;
    }
}

[System.Serializable]
public class MapTileData
{
    public string tileName;     // Name of the tile / sprite.
    public Vector3 position;    // World position of the tile.

    public MapTileData (string tileName, Vector3 position)
    {
        this.tileName = tileName;
        this.position = position;
    }
}