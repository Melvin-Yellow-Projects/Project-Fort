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
        // FIXME: This needs to work for host/server/client
        if (NetworkServer.active && NetworkClient.isConnected)
        {
            GameNetworkManager.IsGameInProgress = false;

            for (int i = GameManager.Players.Count - 1; i >= 0; i--)
            {
                Player p = GameManager.Players[i];
                if (p as ComputerPlayer) NetworkServer.Destroy(p.gameObject);
            }

            // HACK you shouldn't manually have to destroy these
            NetworkServer.Destroy(HexGrid.Singleton.gameObject);
            NetworkServer.Destroy(GameOverHandler.Singleton.gameObject);

            NetworkManager.singleton.StopHost();
        }
        else
        {
            NetworkManager.singleton.StopClient();
        }

        NetworkManager.singleton.autoCreatePlayer = true;
        GameSession.Singleton.IsOnline = false;
        GameSession.Singleton.IsEditorMode = false;

        SceneManager.LoadScene(MenuSceneName); // FIXME
    }

    public static void LoadLocalGame()
    {
        //GameSession.Singleton.IsOnline = false; // HACK: brute force line of code
        NetworkManager.singleton.StartHost();

        LoadSceneByName(GameSceneName); // FIXME
    }

    public static void LoadMapEditorScene()
    {
        GameSession.Singleton.IsEditorMode = true;
        NetworkManager.singleton.autoCreatePlayer = false;
        NetworkManager.singleton.StartHost();

        Debug.Log("Loading Editor Scene");
        LoadSceneByName(EditorSceneName); // FIXME
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
