/**
 * File Name: SceneLoader.cs
 * Description: Loads different scenes
 * 
 * Authors: Will Lacey
 * Date Created: March 27, 2020
 * 
 * Additional Comments: 
 * 
 **/
 
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// 
/// </summary>
public class SceneLoader : MonoBehaviour
{
    /// <summary>
    /// Loads the starting scene
    /// </summary>
    public void LoadStartScene()
    {
        // TODO: does the start scene need a game session?
        //GameSession gameSession = FindObjectOfType<GameSession>();
        //if (gameSession) gameSession.DestroyGameSession();

        SceneManager.LoadScene(0);
    }

    /// <summary>
    /// Loads a scene by name
    /// </summary>
    /// <param name="sceneName">name of the scene to be loaded</param>
    public void LoadSceneByName(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }

    /// <summary>
    /// Loads a scene by name
    /// </summary>
    /// <param name="sceneName">name of the scene to be loaded</param>
    public static void LoadSceneByName(string sceneName, bool dummy)
    {
        SceneManager.LoadScene(sceneName);
    }

    /// <summary>
    /// Loads the next scene in the Build Settings Index
    /// </summary>
    public void LoadNextScene()
    {
        int CurrentSceneIndex = SceneManager.GetActiveScene().buildIndex;
        SceneManager.LoadScene(CurrentSceneIndex + 1);
    }
    
    /// <summary>
    /// Quits Game
    /// </summary>
    public void QuitGame()
    {
        Application.Quit();
    }
}
