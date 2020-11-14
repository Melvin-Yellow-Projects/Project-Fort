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

public class GameSession : MonoBehaviour
{
    /********** MARK: Variables **********/
    #region Variables

    [Tooltip("how fast to run the game's internal clock speed")]
    [SerializeField] [Range(0, 10)] private float gameSpeed = 1f;

    #endregion

    /********** MARK: Unity Functions **********/
    #region Unity Functions

    /// <summary>
    ///     Unity Method; Awake() is called before Start() upon GameObject creation
    /// </summary>
    private void Awake()
    {
        int gameStatusCount = FindObjectsOfType<GameSession>().Length;
        if (gameStatusCount > 1)
        {
            gameObject.SetActive(false);
            DestroyGameSession();
            GameSession gameSession = FindObjectOfType<GameSession>();
            gameSession.gameSpeed = gameSpeed;
        }
        else
        {
            DontDestroyOnLoad(gameObject);
        }
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

    /// <summary>
    ///     Destroys GameObject containing Game Session Class
    /// </summary>
    public void DestroyGameSession()
    {
        Destroy(gameObject);
    }
    #endregion
}
