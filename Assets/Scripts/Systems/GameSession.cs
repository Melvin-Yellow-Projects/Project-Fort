/**
 * File Name: GameSession.cs
 * Description: Handles the current session of the game; Carries data between different levels
 * 
 * Authors: Will Lacey
 * Date Created: March 27, 2020
 * 
 * Additional Comments:
 *      TODO: move this into GameManager?
 * 
 **/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;

public class GameSession : MonoBehaviour
{
    /********** MARK: Variables **********/
    #region Variables

    [Tooltip("how fast to run the game's internal clock speed")]
    [SerializeField] [Range(0, 10)] private float gameSpeed = 1f;

    #endregion

    /********** MARK: Public Properties **********/
    # region Public Properties

    public static BinaryReader BinaryReaderBuffer { get; set; }

    #endregion

    /********** MARK: Unity Functions **********/
    #region Unity Functions

    /// <summary>
    ///     Unity Method; Awake() is called before Start() upon GameObject creation
    /// </summary>
    private void Awake()
    {
        // HACK: this logic is for keeping the game session alive across scenes
        //int gameStatusCount = FindObjectsOfType<GameSession>().Length;
        //if (gameStatusCount > 1)
        //{
        //    gameObject.SetActive(false);
        //    DestroyGameSession();

        //    GameSession gameSession = FindObjectOfType<GameSession>();
        //    gameSession.gameSpeed = gameSpeed;

        //    LoadMapFromReader();
        //}
        //else
        //{
        //    DontDestroyOnLoad(gameObject);
        //}

        LoadMapFromReader(); // HACK: this could be moved to the SaveLoadMenu even
    }

    /// <summary>
    ///     Unity Method; Update() is called once per frame
    /// </summary>
    void Update()
    {
        Time.timeScale = gameSpeed;
    }
    #endregion

    /********** MARK: Class Functions **********/
    #region Class Functions

    private void LoadMapFromReader()
    {
        if (BinaryReaderBuffer == null) return;

        SaveLoadMenu.LoadMapFromReader(BinaryReaderBuffer);

        BinaryReaderBuffer.Close();
        BinaryReaderBuffer = null;
    }

    /// <summary>
    ///     Destroys GameObject containing Game Session Class
    /// </summary>
    public void DestroyGameSession()
    {
        Destroy(gameObject);
    }

    #endregion
}
