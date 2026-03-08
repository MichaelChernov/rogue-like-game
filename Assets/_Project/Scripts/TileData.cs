using UnityEngine;

[CreateAssetMenu(fileName = "TileData", menuName = "Scriptable Objects/TileData")]
public class TileData : ScriptableObject
{
    public string tileName = "Placeholder";
    public Sprite sprite;

    [Header("Generation Stats")]
    [Tooltip("Higher = Spawns constantly. Lower = Very rare.")]
    public float baseSpawnWeight = 1.0f;
}
