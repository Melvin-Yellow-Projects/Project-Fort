/**
 * File Name: SaveLoadMenu.cs
 * Description: TODO: comment script
 * 
 * Authors: Catlike Coding, Will Lacey
 * Date Created: October 2, 2020
 * 
 * Additional Comments: 
 *      The original version of this file can be found here:
 *      https://catlikecoding.com/unity/tutorials/hex-map/ within Catlike Coding's tutorial series:
 *      Hex Map; this file has been updated it to better fit this project
 **/

using UnityEngine;
using UnityEngine.UI;
using System;
using System.IO;
using UnityEngine.InputSystem;

/// <summary>
/// 
/// </summary>
public class SaveLoadMenu : MonoBehaviour
{
    /********** MARK: Public Variables **********/
    #region Public Variables

    public Text menuLabel;

    public Text actionButtonLabel;

    public InputField nameInput;

    public RectTransform listContent;

    public SaveLoadItem itemPrefab;

    #endregion

    /********** MARK: Private Variables **********/
    #region Private Variables

    /// <summary>
    /// current map save/load version
    /// </summary>
    const int mapFileVersion = 4;

    /// <summary>
    /// determines if the user is either saving or loading
    /// </summary>
    int menuMode;

    Controls controls;

    #endregion

    /********** MARK: Unity Functions **********/
    #region Unity Functions

    private void Awake()
    {
        controls = new Controls();
        controls.General.Affirmation.performed += Action;
        controls.Enable();
    }

    private void OnDestroy()
    {
        controls.General.Affirmation.performed -= Action;
    }

    #endregion

    /********** MARK: Class Functions **********/
    #region Class Functions

    public void Open(int menuMode)
	{
        this.menuMode = menuMode;

        if (menuMode == 0)
        {
            menuLabel.text = "Save Map";
            actionButtonLabel.text = "Save";
        }
        else
        {
            menuLabel.text = "Load Map";
            actionButtonLabel.text = "Load";
        }

        FillList(); // TODO: check what happens if this happens at the end

        gameObject.SetActive(true);
		HexMapCamera.Locked = true;
	}

	public void Close()
	{
		gameObject.SetActive(false);
		HexMapCamera.Locked = false;
	}

    private void Action(InputAction.CallbackContext ctx)
    {
        Action();
    }

    public void Action()
    {
        string path = GetSelectedPath();

        // if the path is empty, exit
        if (path == null) return;

        // action depends on saveMode
        if (menuMode == 0)
        {
            Save(path);
        }
        else if (menuMode == 1)
        {
            Load(path);
        }
        else
        {
            PrepareReaderForNextScene("Game Scene");
        }

        // exit menu
        Close();
    }

    private void PrepareReaderForNextScene(string nextSceneName)
    {
        string path = GetSelectedPath();

        // if the path is empty, exit
        if (path == null) return;

        // if the path is invalid, exit
        if (!IsPathValid(path)) return;

        BinaryReader reader = new BinaryReader(File.OpenRead(path));
        GameSession.BinaryReaderBuffer = reader;

        SceneLoader.LoadSceneByName(nextSceneName, false);
    }

    public void SelectItem(string name)
    {
        nameInput.text = name;
    }

    void FillList()
    {
        // destroy previous items in list
        for (int i = 0; i < listContent.childCount; i++)
        {
            Destroy(listContent.GetChild(i).gameObject);
        }

        string[] paths = Directory.GetFiles(Application.persistentDataPath, "*.map");

        // sorts paths in alphabetical order
        Array.Sort(paths);

        for (int i = 0; i < paths.Length; i++)
        {
            SaveLoadItem item = Instantiate(itemPrefab);
            item.menu = this;
            item.MapName = Path.GetFileNameWithoutExtension(paths[i]); // removes path from name
            item.transform.SetParent(listContent, false);
        }
    }

    /// <summary>
    /// TODO: "You can use the Content Type of input fields to control what kind of input is
    /// allowed"
    /// </summary>
    /// <returns></returns>
    string GetSelectedPath()
    {
        string mapName = nameInput.text;
        if (mapName.Length == 0)
        {
            return null;
        }

        // this shows where the file is going to be saved
        //Debug.Log(Application.persistentDataPath);

        // creates path specific to the system's file storage
        return Path.Combine(Application.persistentDataPath, mapName + ".map");
    }

    /// <summary>
    /// TODO write save func
    /// </summary>
	public void Save(string path)
    {
        // creates a file stream object encapsulated within the BinaryWriter; the using block then
        // defines where this object will exist; "these objects have a Dispose method, which is
        // implicitly invoked when exiting the using scope"; c#/visual studio also struggle to make
        // sense of this... so ignore warnings or any editor suggestions outside of Unity Editor
        using (BinaryWriter writer = new BinaryWriter(File.Open(path, FileMode.Create)))
        {
            writer.Write(mapFileVersion);
            FindObjectOfType<HexGrid>().Save(writer);
        }
    }

    /// <summary>
    /// TODO write Load func
    /// </summary>
	public void Load(string path)
    {
        // check to see if the path exists
        if (!IsPathValid(path)) return;

        // creates a file stream object encapsulated within the BinaryReader; the using block then
        // defines where this object will exist
        using (BinaryReader reader = new BinaryReader(File.OpenRead(path)))
        {
            LoadMapFromReader(reader);
        }
    }

    public static void LoadMapFromReader(BinaryReader reader)
    {
        int header = reader.ReadInt32();
        if (header <= mapFileVersion)
        {
            FindObjectOfType<HexGrid>().Load(reader, header);

            HexMapCamera.ValidatePosition();
        }
        else
        {
            Debug.LogWarning("Unknown map format " + header);
        }
    }

    private bool IsPathValid(string path)
    {
        if (!File.Exists(path))
        {
            Debug.LogError("Path/File does not exist " + path);
            return false;
        }

        return true;
    }

    public void Delete()
    {
        string path = GetSelectedPath();
        if (path == null)
        {
            return;
        }

        // check if the file exists first
        if (File.Exists(path)) File.Delete(path);

        nameInput.text = "";
        FillList();
    }

    #endregion
}