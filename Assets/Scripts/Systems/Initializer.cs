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
 * 
 *      TODO: make a static initializer bool variable that checks to see if the game has init
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
    /************************************************************/
    #region Variables

    //static bool hasInitialized = false;

    [Header("SceneLoader")]
    [Tooltip("main menu scene")]
    [SerializeField, Scene] string menuScene;
    [Tooltip("core game scene")]
    [SerializeField, Scene] string gameScene;
    [Tooltip("map editor scene")]
    [SerializeField, Scene] string editorScene;

    [Header("ComputerPlayer")]
    [Tooltip("reference to the ComputerPlayer prefab")]
    [SerializeField] ComputerPlayer computerPlayerPrefab = null;

    [Header("GameOverHandler")]
    [Tooltip("reference to the GameOverHandler prefab")]
    [SerializeField] GameOverHandler gameOverHandlerPrefab = null;

    [Header("PopupMenu")]
    [Tooltip("menu for creating informative popups")]
    [SerializeField] PopupMenu popupMenuPrefab = null;

    [Header("HexMetrics")]
    [Tooltip("hex map configuration file for Hex Metrics")]
    [SerializeField] HexConfig configuration;

    [Header("Fort")]
    [Tooltip("reference to the Fort prefab")]
    [SerializeField] Fort fortPrefab;
    [Tooltip("highlight color that shows buy area for economy phase")]
    [SerializeField] Color fortHighlightColor;

    [Header("Piece")]
    [Tooltip("references to the piece prefabs")]
    [SerializeField] Piece axePrefab;
    [SerializeField] Piece bowPrefab;
    [SerializeField] Piece horsePrefab;
    [SerializeField] Piece pikePrefab;
    [SerializeField] Piece wallPrefab;

    [Header("PieceCursor")]
    [Tooltip("piece cursor prefab reference")]
    [SerializeField] PieceCursor pieceCursorPrefab = null;
    [Tooltip("piece cursor material reference")]
    [SerializeField] Material pieceCursorMaterial = null;

    #endregion
    /************************************************************/
    #region Unity Functions

    protected void Start()
    {
        Debug.Log("initializing classes with their respective prefabs");

        /** SceneLoader **/
        if (menuScene != null && SceneLoader.MenuSceneName == null)
            SceneLoader.MenuSceneName = System.IO.Path.GetFileNameWithoutExtension(menuScene);
        if (gameScene != null && SceneLoader.GameSceneName == null)
            SceneLoader.GameSceneName = System.IO.Path.GetFileNameWithoutExtension(gameScene);
        if (editorScene != null && SceneLoader.EditorSceneName == null)
            SceneLoader.EditorSceneName = System.IO.Path.GetFileNameWithoutExtension(editorScene);

        /** Computer Player **/
        if (computerPlayerPrefab && !ComputerPlayer.Prefab)
            ComputerPlayer.Prefab = computerPlayerPrefab;

        /** GameOverHandler **/
        if (gameOverHandlerPrefab && !GameOverHandler.Prefab)
            GameOverHandler.Prefab = gameOverHandlerPrefab;

        /** PopupMenu **/
        if (popupMenuPrefab && !PopupMenu.Prefab) PopupMenu.Prefab = popupMenuPrefab;

        /** HexMetrics **/
        if (configuration && !HexMetrics.Configuration) HexMetrics.Configuration = configuration;

        /** Fort **/
        if (fortPrefab && !Fort.Prefab) Fort.Prefab = fortPrefab;
        if (fortHighlightColor != null && Fort.HighlightColor != null)
            Fort.HighlightColor = fortHighlightColor;

        /** Piece **/
        if (Piece.Prefabs == null)
        {
            Piece.Prefabs = new List<Piece>
            {
                axePrefab,
                bowPrefab,
                horsePrefab,
                pikePrefab,
                wallPrefab
            };
        }

        /** PieceCursor **/
        if (pieceCursorPrefab && !PieceCursor.Prefab) PieceCursor.Prefab = pieceCursorPrefab;
        if (pieceCursorMaterial && !PieceCursor.MyMaterial)
            PieceCursor.MyMaterial = pieceCursorMaterial;
    }

    #endregion
}
