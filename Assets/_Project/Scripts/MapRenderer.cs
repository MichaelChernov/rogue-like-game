using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class MapRenderer : MonoBehaviour
{
    public GameObject tilePrefab;

    // Based on 48x16 pixels @ 16 PPU
    private const float X_STEP = 2.00f; // (48 * 0.6666666) / 16
    private const float Y_STEP = 1.00f; // 16 / 16

    public void RenderMap(Dictionary<Vector3Int, TileData> mapData)
    {
        // Clear existing GameObjects
        foreach (Transform child in transform) {
            if(Application.isPlaying) Destroy(child.gameObject);
            else DestroyImmediate(child.gameObject);
        }

        foreach (var entry in mapData)
        {
            Vector3Int coord = entry.Key;
            TileData data = entry.Value;

            Vector3 worldPos = CubeToWorld(coord);

            GameObject go = Instantiate(tilePrefab, worldPos, Quaternion.identity, transform);
            go.name = $"Hex_{coord.x}_{coord.y}_{coord.z}";

            var sr = go.GetComponent<SpriteRenderer>();
            if (sr != null)
            {
                sr.sprite = data.sprite;
                // Since the tiles are super flat (16px), sorting is vital.
                // We use a large multiplier (100) to ensure distinct layers.
                sr.sortingOrder = Mathf.RoundToInt(-worldPos.y * 100);
            }
        }
    }

    public Vector3 CubeToWorld(Vector3Int cube)
    {
        float x = cube.x * X_STEP;
        // The y calculation for Cube coordinates on a flat-top grid:
        float y = (cube.z + (cube.x / 2f)) * Y_STEP;

        return new Vector3(x, y, 0);
    }
}
