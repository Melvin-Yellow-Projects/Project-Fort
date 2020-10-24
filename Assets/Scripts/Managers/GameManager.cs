﻿/**
 * File Name: GameManager.cs
 * Description: Manages scene loading and persistent data
 * 
 * Authors: Will Lacey
 * Date Created: October 22, 2020
 * 
 * Additional Comments: 
 **/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// 
/// </summary>
public class GameManager : MonoBehaviour
{
    /********** MARK: Scene Functions **********/
    #region Scene Functions

    public void ExecuteMoves()
    {
        HexGrid grid = FindObjectOfType<HexGrid>();
        for(int i = 0; i < grid.units.Count; i++)
        {
            HexUnit unit = grid.units[i];
            unit.Travel();
        }
    }

    #endregion

    /********** MARK: Class Functions **********/
    #region Class Functions

    /// <summary>
    ///     Loads a scene by name
    /// </summary>
    /// <param name="sceneName">name of the scene to be loaded</param>
    public void LoadSceneByName(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }

    /// <summary>
    ///     Loads the next scene in the Build Settings Index
    /// </summary>
    public void LoadNextScene()
    {
        int CurrentSceneIndex = SceneManager.GetActiveScene().buildIndex;
        SceneManager.LoadScene(CurrentSceneIndex + 1);
    }

    /// <summary>
    ///     Quits the application and closes game
    /// </summary>
    public void QuitGame()
    {
        Application.Quit();
    }

    #endregion
}