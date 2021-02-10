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

    public static string MenuSceneName { get; set; }

    public static string GameSceneName { get; set; }

    public static string EditorSceneName { get; set; }

    public static bool IsGameScene
    {
        get
        {
            return SceneManager.GetActiveScene().name.Equals(GameSceneName);
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
        // FIXME: This needs to work for host/server/client; i can't figure it out yeesh
        if (NetworkServer.active && NetworkClient.isConnected)
        {
            NetworkManager.singleton.StopHost();
            //Application.Quit();
        }
        else
        {
            NetworkManager.singleton.StopClient();
            //Application.Quit(); // FIXME yea this line-of-code needs to line-of-go
        }

        SceneManager.LoadScene(MenuSceneName); 
    }

    public static void LoadLocalGame()
    {
        //GameSession.Singleton.IsOnline = false; // HACK: brute force line of code
        NetworkManager.singleton.StartHost();

        LoadSceneByName(GameSceneName); 
    }

    public static void LoadMapEditorScene()
    {
        GameSession.IsEditorMode = true;
        NetworkManager.singleton.autoCreatePlayer = false;
        NetworkManager.singleton.StartHost();

        Debug.Log("Loading Editor Scene");
        LoadSceneByName(EditorSceneName);
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
