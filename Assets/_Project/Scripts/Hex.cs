using System.Collections.Generic;
using UnityEngine;

public static class Hex
{
    // The 6 directions for Flat-Top Cube Coordinates
    public static readonly Vector3Int East       = new Vector3Int( 1, -1,  0);
    public static readonly Vector3Int West       = new Vector3Int(-1,  1,  0);
    public static readonly Vector3Int NorthEast  = new Vector3Int( 1,  0, -1);
    public static readonly Vector3Int NorthWest  = new Vector3Int( 0,  1, -1);
    public static readonly Vector3Int SouthEast  = new Vector3Int( 0, -1,  1);
    public static readonly Vector3Int SouthWest  = new Vector3Int(-1,  0,  1);

    // Useful for loops: foreach(Vector3Int dir in Hex.Directions)
    public static readonly Vector3Int[] Directions = {
        East, NorthEast, NorthWest, West, SouthWest, SouthEast
    };

    // Helper to get a direction by name if you really want to
    public static Vector3Int GetDirection(string name)
    {
        return name.ToLower() switch
        {
            "east" => East,
            "west" => West,
            "northeast" or "ne" => NorthEast,
            "northwest" or "nw" => NorthWest,
            "southeast" or "se" => SouthEast,
            "southwest" or "sw" => SouthWest,
            _ => Vector3Int.zero
        };
    }
}
