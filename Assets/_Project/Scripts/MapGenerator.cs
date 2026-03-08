using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[System.Serializable]
public struct TileAffinity 
{
    public TileData TileA;
    public TileData TileB;
    public float Score; // +5 for "Loves", -10 for "Hates"
}

//Shit code: innefficient, slow, stupid. fix later
public class MapGenerator : MonoBehaviour
{

    [Header("Generation Settings")]
    public List<TileAffinity> affinities;

    [Header("Hexagonal Bounds")]
    [Tooltip("Distance from center (0,0,0) to the edge")]
    public int mapRange = 50; 

    [Header("Tile Palette")]
    public List<TileData> availableTiles;

    //Change later, seems innefficient
    [Header("Tile SO's")]
    public TileData grassTileData;
    public TileData seaTileData;

    public Dictionary<Vector3Int, TileData> GenerateVectors()
    {
        Dictionary<Vector3Int, TileData> mapData = new();

        // Standard Cube Coordinate Triple-Loop
        // We iterate through X and Y, then calculate Z
        for (int x = -mapRange; x <= mapRange; x++)
        {
            // The magic: Y's range is constrained by X to keep the sum x+y+z = 0
            int yMin = Mathf.Max(-mapRange, -x - mapRange);
            int yMax = Mathf.Min(mapRange, -x + mapRange);

            for (int y = yMin; y <= yMax; y++)
            {
                int z = -x - y;

                Vector3Int coord = new Vector3Int(x, y, z);
                
                mapData.Add(coord, null);
            }
        }

        Debug.Log($"Generated a Hexagonal map with {mapData.Count} tiles using a Range of {mapRange}.");

        return mapData;
    }

    public Dictionary<Vector3Int, TileData> GenerateWeightedMap(Dictionary<Vector3Int, TileData> mapVectors)
    {
        // 1. Get all coordinates and sort them center-out
        List<Vector3Int> allCoords = GetAllCoordinatesSortedByDistance(mapVectors);

        Dictionary<Vector3Int, TileData> mapData = mapVectors;
        mapData.Clear();

        // 2. Loop through every coordinate
        foreach (Vector3Int coord in allCoords)
        {
            // Get the neighbors that have ALREADY been placed in the dictionary
            List<TileData> placedNeighbors = GetPlacedNeighbors(mapData, coord);

            if (placedNeighbors.Count == 0)
            {
                // First tile at 0,0,0! Pick completely at random.
                mapData.Add(coord, grassTileData);
                continue;
            }

            // 3. Calculate scores for all possible tiles
            Dictionary<TileData, float> tileScores = new Dictionary<TileData, float>();

            foreach (TileData possibleTile in availableTiles)
            {
                // START with this specific tile's base rarity, not a global number
                float score = possibleTile.baseSpawnWeight; 

                foreach (TileData neighbor in placedNeighbors)
                {
                    // Add the affinities
                    score += GetAffinityBetween(possibleTile, neighbor); 
                }

                // Don't allow negative weights in a roulette wheel
                tileScores[possibleTile] = Mathf.Max(0f, score); 
            }

            // 4. Spin the wheel
            TileData winningTile = GetTileFromRouletteWheel(tileScores);
            mapData.Add(coord, winningTile);
        }

        return mapData;
    }

    // --- 1. GET ALL COORDINATES SORTED BY DISTANCE ---
    // This creates the "Expanding Ripple" generation effect
    public List<Vector3Int> GetAllCoordinatesSortedByDistance(Dictionary<Vector3Int, TileData> mapVectors)
    {
        return mapVectors.Keys.OrderBy(coord => 
            Mathf.Max(Mathf.Abs(coord.x), Mathf.Abs(coord.y), Mathf.Abs(coord.z))
        ).ToList();
    }

    // --- 2. GET PLACED NEIGHBORS ---
    // Checks the 6 directions and returns only the tiles that aren't null
    public List<TileData> GetPlacedNeighbors(Dictionary<Vector3Int, TileData> mapData, Vector3Int currentCoord)
    {
        List<TileData> neighbors = new List<TileData>();

        foreach (Vector3Int dir in Hex.Directions)
        {
            Vector3Int neighborCoord = currentCoord + dir;
            
            // Check if the coord exists in our map and has a tile assigned
            if (mapData.TryGetValue(neighborCoord, out TileData tile))
            {
                if (tile != null) 
                {
                    neighbors.Add(tile);
                }
            }
        }
        return neighbors;
    }

    // --- 3. GET AFFINITY BETWEEN ---
    // Searches your list for a rule matching these two tiles (A-B or B-A)
    public float GetAffinityBetween(TileData tile, TileData neighbor)
    {
        foreach (var rule in affinities)
        {
            // Symmetry check: Does Rule match (Tile-Neighbor) OR (Neighbor-Tile)?
            if ((rule.TileA == tile && rule.TileB == neighbor) || 
                (rule.TileA == neighbor && rule.TileB == tile))
            {
                return rule.Score;
            }
        }
        return 0f; // No specific rule found
    }

    // --- 4. GET TILE FROM ROULETTE WHEEL ---
    // Takes the calculated scores and picks one based on probability
    public TileData GetTileFromRouletteWheel(Dictionary<TileData, float> tileScores)
    {
        float totalWeight = 0;
        foreach (float weight in tileScores.Values)
        {
            totalWeight += weight;
        }

        // Pick a random point on the "wheel"
        float randomPoint = Random.Range(0, totalWeight);
        float currentSum = 0;

        foreach (var entry in tileScores)
        {
            currentSum += entry.Value;
            if (randomPoint <= currentSum)
            {
                return entry.Key;
            }
        }

        return null; // Should only happen if all weights are 0
    }

    //Clean the single hex land tiles in a generated sea.
    public Dictionary<Vector3Int, TileData> CleanSea(Dictionary<Vector3Int, TileData> mapData)
    {
        // Store coordinates that need to be changed
        List<Vector3Int> tilesToChange = new List<Vector3Int>();

        foreach (var entry in mapData)
        {
            Vector3Int currentPos = entry.Key;
            TileData currentTile = entry.Value;

            // We only care if the current tile ISN'T sea
            if (currentTile != seaTileData)
            {
                int seaNeighborCount = 0;

                foreach (Vector3Int direction in Hex.Directions)
                {
                    Vector3Int neighborPos = currentPos + direction;

                    if (mapData.TryGetValue(neighborPos, out TileData neighborData))
                    {
                        if (neighborData == seaTileData)
                        {
                            seaNeighborCount++;
                        }
                    }
                    else
                    {
                        // Treat the edge of the map as sea to clean coastal artifacts
                        seaNeighborCount++;
                    }
                }

                // If all 6 neighbors are sea, this is a 1-hex island
                if (seaNeighborCount == 6)
                {
                    tilesToChange.Add(currentPos);
                }
            }
        }

        foreach (Vector3Int pos in tilesToChange)
        {
            mapData[pos] = seaTileData;
        }

        return mapData;
    }
}
