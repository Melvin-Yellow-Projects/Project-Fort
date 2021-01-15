/**
 * File Name: SceneLoader.cs
 * Description: Loads different scenes
 * 
 * Authors: Will Lacey
 * Date Created: March 27, 2020
 * 
 * Additional Comments: 
 *      HACK: Multiple hardcoded string references exist in this class, is this okay?
 **/
 
using UnityEngine;
using UnityEngine.SceneManagement;
using Mirror;

/// <summary>
/// 
/// </summary>
public class SceneLoader : MonoBehaviour
{
    /********** MARK: Properties **********/
    #region Properties

    public static bool IsGameScene
    {
        get
        {
            return SceneManager.GetActiveScene().name.StartsWith("Game Scene");
        }
    }

    #endregion

    /********** MARK: Class Functions **********/
    #region Class Functions

    /// <summary>
    /// Loads the starting scene
    /// </summary>
    public static void LoadStartScene()
    {
        // HACK: i think this verifies that we are the host
        if (NetworkServer.active && NetworkClient.isConnected)
            GameNetworkManager.Singleton.StopHost();

        NetworkManager.singleton.autoCreatePlayer = true;
        GameSession.Singleton.IsOnline = false;
        GameSession.Singleton.IsEditorMode = false;

        SceneManager.LoadScene(0);
    }

    public static void LoadLocalGame()
    {
        //GameSession.Singleton.IsOnline = false; // HACK: brute force line of code
        NetworkManager.singleton.StartHost();

        LoadSceneByName("Game Scene");
    }

    public static void LoadMapEditorScene()
    {
        GameSession.Singleton.IsEditorMode = true;
        NetworkManager.singleton.autoCreatePlayer = false;
        NetworkManager.singleton.StartHost();

        Debug.Log("Loading Map Editor Scene");
        LoadSceneByName("Map Editor Scene");
    }

    /// <summary>
    /// Loads a scene by name
    /// </summary>
    /// <param name="sceneName">name of the scene to be loaded</param>
    public static void LoadSceneByName(string sceneName)
    {
        Debug.Log($"LoadSceneByName {sceneName}");
        GameNetworkManager.Singleton.ServerChangeScene(sceneName);
    }

    /// <summary>
    /// Quits Game
    /// </summary>
    public static void QuitGame()
    {
        Application.Quit();
    }

    #endregion
}
