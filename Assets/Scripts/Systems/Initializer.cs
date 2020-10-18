/**
 * File Name: Initializer.cs
 * Description: This configuration script serves to initialize static objects and references from a 
 *                  singleton MonoBehavior GameObject that may seem otherwise out-of-place 
 *                  elsewhere
 * 
 * Authors: Will Lacey
 * Date Created: October 17, 2020
 * 
 * Additional Comments: 
 **/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Singleton MonoBehavior GameObject that initializes various static class references
/// </summary>
public class Initializer : MonoBehaviour
{
    /********** MARK: Public Variables **********/
    #region Public Variables

    [Header("HexCurser")]
    [Tooltip("hex curser prefab reference")]
    [SerializeField] HexCurser hexCurserPrefab = null;
    [Tooltip("hex curser material reference")]
    [SerializeField] Material hexCurserMaterial = null;

    [Header("HexMetrics")]
    [Tooltip("noise source for Hex Metrics")]
    public Texture2D noiseSource;

    [Header("HexUnit")]
    [Tooltip("reference to the hex unit prefab")]
    public HexUnit hexUnitPrefab;

    #endregion

    /********** MARK: Unity Functions **********/
    #region Unity Functions

    /// <summary>
    /// Unity Method; Awake() is called before Start() upon GameObject creation
    /// </summary>
    protected void Awake()
    {
        // HexCurser
        if (hexCurserPrefab) HexCurser.prefab = hexCurserPrefab;
        if (hexCurserMaterial) HexCurser.material = hexCurserMaterial;

        // HexMetrics
        if (noiseSource) HexMetrics.noiseSource = noiseSource;

        // HexUnit
        if (hexUnitPrefab) HexUnit.prefab = hexUnitPrefab;
    }

    /// <summary>
    /// Unity Method; This function is called when the object becomes enabled and active
    /// </summary>
    protected void OnEnable()
    {
        // HexCurser
        if (hexCurserPrefab && !HexCurser.prefab) HexCurser.prefab = hexCurserPrefab;
        if (hexCurserMaterial && !HexCurser.material) HexCurser.material = hexCurserMaterial;

        // HexMetrics
        if (noiseSource && !HexMetrics.noiseSource) HexMetrics.noiseSource = noiseSource;

        // HexUnit
        if (hexUnitPrefab && !HexUnit.prefab) HexUnit.prefab = hexUnitPrefab;
    }

    #endregion
}
