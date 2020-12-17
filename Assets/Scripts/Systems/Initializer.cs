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

    [Header("GameMode")]
    [Tooltip("game mode settings load out")]
    [SerializeField] GameMode gameModeSettings = null;

    [Header("HexMetrics")]
    [Tooltip("noise source for Hex Metrics")]
    public Texture2D noiseSource;

    [Header("Fort")]
    [Tooltip("reference to the Fort prefab")]
    public Fort fortPrefab;

    [Header("Unit")]
    [Tooltip("reference to the Unit prefab")]
    public Unit unitPrefab;

    [Header("UnitCursor")]
    [Tooltip("unit cursor prefab reference")]
    [SerializeField] UnitCursor unitCursorPrefab = null;
    [Tooltip("unit cursor material reference")]
    [SerializeField] Material unitCursorMaterial = null;

    #endregion

    /********** MARK: Unity Functions **********/
    #region Unity Functions

    /// <summary>
    /// Unity Method; Awake() is called before Start() upon GameObject creation
    /// </summary>
    protected void Awake()
    {
        // GameMode
        if (gameModeSettings) GameMode.Singleton = gameModeSettings;

        // HexMetrics
        if (noiseSource) HexMetrics.noiseSource = noiseSource;

        // Fort
        if (fortPrefab) Fort.Prefab = fortPrefab;

        // Unit
        if (unitPrefab) Unit.Prefab = unitPrefab;

        // UnitCursor
        if (unitCursorPrefab) UnitCursor.Prefab = unitCursorPrefab;
        if (unitCursorMaterial) UnitCursor.MyMaterial = unitCursorMaterial;
    }

    /// <summary>
    /// Unity Method; This function is called when the object becomes enabled and active
    /// </summary>
    protected void OnEnable()
    {
        // GameMode
        if (gameModeSettings && !GameMode.Singleton) GameMode.Singleton = gameModeSettings;

        // HexMetrics
        if (noiseSource && !HexMetrics.noiseSource) HexMetrics.noiseSource = noiseSource;

        // Fort
        if (fortPrefab && !Fort.Prefab) Fort.Prefab = fortPrefab;

        // Unit
        if (unitPrefab && !Unit.Prefab) Unit.Prefab = unitPrefab;

        // UnitCursor
        if (unitCursorPrefab && !UnitCursor.Prefab) UnitCursor.Prefab = unitCursorPrefab;
        if (unitCursorMaterial && !UnitCursor.MyMaterial) UnitCursor.MyMaterial = unitCursorMaterial;
    }

    #endregion
}
