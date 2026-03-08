using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public MapRenderer mapRenderer;
    public MapGenerator mapGenerator;

    // The "Brain" - Keys are Cube Coords, Values are Tile Data (null = empty)
    public Dictionary<Vector3Int, TileData> mapData = new();

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        var mapVectors = mapGenerator.GenerateVectors();
        mapData = mapGenerator.GenerateWeightedMap(mapVectors);
        mapGenerator.CleanSea(mapData);
        mapRenderer.RenderMap(mapData);
    }
}
