/**
 * File Name: HexConfig.cs
 * Description: Configuration file for the hex map
 * 
 * Authors: Will Lacey
 * Date Created: March 1, 2021
 * 
 * Additional Comments: 
 **/


using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 
/// </summary>
[CreateAssetMenu(menuName = "Hex Configuration")]
public class HexConfig : ScriptableObject
{
    /************************************************************/
    #region Variables

    [Header("Editor")]
    [Tooltip("editor description for this configuration file; purely for the editor")]
    [SerializeField, TextArea] string configurationDescription;

    [Header("Hex Cell Settings")]
    [Tooltip("number of hex map chunks in the X direction")]
    [SerializeField] int chunkSizeX = 4;

    [Tooltip("number of hex map chunks in the Z direction")]
    [SerializeField] int chunkSizeZ = 3;

    [Tooltip("a hex's outer radius")]
    [SerializeField] float outerRadius = 12f;

    [Tooltip("percent of a HexCell that is solid and unaltered by its neighbors")]
    [SerializeField] float solidFactor = 0.8f;

    [Header("Elevation Settings")]
    [Tooltip("height of each successive elevation change")]
    [SerializeField] float elevationStep = 4f;

    [Tooltip("number of terraces per slope (small elevation connection between hex cells)")]
    [SerializeField] int terracesPerSlope = 3;

    [Tooltip("how high the elevation delta must be to be considered a cliff (set to 0 for only " +
        "cliffs)")]
    [SerializeField] int cliffDelta = 2;

    [Header("Noise Settings")]
    [Tooltip("source of map noise; HexGrid serves as an intermediate to assign the noise source " +
        "to HexMetrics because this is a static class")]
    [SerializeField] Texture2D noiseSource;

    [Tooltip("strength of hex grid vertex noise; max displacement will equal " +
        "[2 * (value ** 2)] ** 0.5")]
    [SerializeField] float cellPerturbStrength = 6f;

    [Tooltip("strength of hex grid elevation noise; this should be relatively related to a " +
        "vertical terrace step and an elevation step")]
    [SerializeField] float elevationPerturbStrength = 2f;

    [Tooltip("how often the noise repeats itself; repeats every " +
        "[1 / (2 * noiseScale * innerRadius)]")]
    float noiseScale = 0.003f;

    #endregion

    /************************************************************/
    #region Properties

    public int ChunkSizeX => chunkSizeX;

    public int ChunkSizeZ => chunkSizeZ;

    public float OuterRadius => outerRadius;

    public float SolidFactor => solidFactor;

    public float ElevationStep => elevationStep;

    public int TerracesPerSlope => terracesPerSlope;

    public int CliffDelta => cliffDelta;

    public float CellPerturbStrength => cellPerturbStrength;

    public float ElevationPerturbStrength => elevationPerturbStrength;

    public Texture2D NoiseSource => noiseSource;

    public float NoiseScale => noiseScale;

    #endregion
}
