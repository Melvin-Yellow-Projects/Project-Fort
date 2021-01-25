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
using UnityEngine.SceneManagement;
using Mirror;

/// <summary>
/// Singleton MonoBehavior GameObject that initializes various static class references
/// </summary>
public class Initializer : MonoBehaviour
{
    /********** MARK: Public Variables **********/
    #region Public Variables

    [Header("SceneLoader")]
    [Tooltip("main menu scene")]
    [SerializeField, Scene] string menuScene;
    [Tooltip("core game scene")]
    [SerializeField, Scene] string gameScene;
    [Tooltip("map editor scene")]
    [SerializeField, Scene] string editorScene;

    [Header("GameMode")]
    [Tooltip("game mode settings load out")]
    [SerializeField] GameMode gameModeSettings = null;

    [Header("ComputerPlayer")]
    [Tooltip("reference to the ComputerPlayer prefab")]
    [SerializeField] ComputerPlayer computerPlayerPrefab = null;

    [Header("GameOverHandler")]
    [Tooltip("reference to the GameOverHandler prefab")]
    [SerializeField] GameOverHandler gameOverHandlerPrefab = null;

    [Header("HexMetrics")]
    [Tooltip("noise source for Hex Metrics")]
    [SerializeField] Texture2D noiseSource;

    [Header("HexGrid")]
    [Tooltip("reference to the HexGrid prefab")]
    [SerializeField] HexGrid hexGridPrefab;

    [Header("Fort")]
    [Tooltip("reference to the Fort prefab")]
    [SerializeField] Fort fortPrefab;

    [Header("Units")]
    [Tooltip("references to the Unit prefabs")]
    [SerializeField] Unit axePrefab;
    [SerializeField] Unit horsePrefab;
    [SerializeField] Unit pikePrefab;
    [SerializeField] Unit wallPrefab;

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
        Debug.Log("initializing classes with their respective prefabs");

        // SceneLoader
        if (menuScene != null && SceneLoader.MenuSceneName == null)
            SceneLoader.MenuSceneName = menuScene;
        if (gameScene != null && SceneLoader.GameSceneName == null)
            SceneLoader.GameSceneName = gameScene;
        if (editorScene != null && SceneLoader.EditorSceneName == null)
            SceneLoader.EditorSceneName = editorScene;

        // GameMode
        if (gameModeSettings && !GameMode.Singleton) GameMode.Singleton = gameModeSettings;

        // ComputerPlayer
        if (computerPlayerPrefab && !ComputerPlayer.Prefab)
            ComputerPlayer.Prefab = computerPlayerPrefab;

        // GameOverHandler
        if (gameOverHandlerPrefab && !GameOverHandler.Prefab)
            GameOverHandler.Prefab = gameOverHandlerPrefab;

        // HexMetrics
        if (noiseSource && !HexMetrics.noiseSource) HexMetrics.noiseSource = noiseSource;

        // HexGrid
        if (hexGridPrefab && !HexGrid.Prefab) HexGrid.Prefab = hexGridPrefab;

        // Fort
        if (fortPrefab && !Fort.Prefab) Fort.Prefab = fortPrefab;

        // Unit
        if (Unit.Prefabs == null)
        {
            Unit.Prefabs = new List<Unit>
            {
                axePrefab,
                horsePrefab,
                pikePrefab,
                wallPrefab
            };
        }

        // UnitCursor
        if (unitCursorPrefab && !UnitCursor.Prefab) UnitCursor.Prefab = unitCursorPrefab;
        if (unitCursorMaterial && !UnitCursor.MyMaterial)
            UnitCursor.MyMaterial = unitCursorMaterial;
    }

    #endregion
}
