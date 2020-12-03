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

    [Header("HexCursor")]
    [Tooltip("hex cursor prefab reference")]
    [SerializeField] UnitCursor hexCursorPrefab = null;
    [Tooltip("hex curser material reference")]
    [SerializeField] Material hexCurserMaterial = null;

    [Header("HexMetrics")]
    [Tooltip("noise source for Hex Metrics")]
    public Texture2D noiseSource;

    [Header("HexUnit")]
    [Tooltip("reference to the hex unit prefab")]
    public Unit unitPrefab;

    #endregion

    /********** MARK: Unity Functions **********/
    #region Unity Functions

    /// <summary>
    /// Unity Method; Awake() is called before Start() upon GameObject creation
    /// </summary>
    protected void Awake()
    {
        // HexCurser
        if (hexCursorPrefab) UnitCursor.prefab = hexCursorPrefab;
        if (hexCurserMaterial) UnitCursor.material = hexCurserMaterial;

        // HexMetrics
        if (noiseSource) HexMetrics.noiseSource = noiseSource;

        // Unit
        if (unitPrefab) Unit.prefab = unitPrefab;
    }

    /// <summary>
    /// Unity Method; This function is called when the object becomes enabled and active
    /// </summary>
    protected void OnEnable()
    {
        // HexCursor
        if (hexCursorPrefab && !UnitCursor.prefab) UnitCursor.prefab = hexCursorPrefab;
        if (hexCurserMaterial && !UnitCursor.material) UnitCursor.material = hexCurserMaterial;

        // HexMetrics
        if (noiseSource && !HexMetrics.noiseSource) HexMetrics.noiseSource = noiseSource;

        // Unit
        if (unitPrefab && !Unit.prefab) Unit.prefab = unitPrefab;
    }

    #endregion
}
