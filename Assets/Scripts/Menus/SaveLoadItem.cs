/**
 * File Name: SaveLoadItem.cs
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

public class SaveLoadItem : MonoBehaviour
{
	/********** MARK: Variables **********/
	#region Variables

    /// <summary>
    /// TODO: comment var, must be initialized when instantiated
    /// </summary>
	public SaveLoadMenu menu;

	string mapName;

	#endregion

	/********** MARK: Properties **********/
	#region Properties

	public string MapName
	{
		get
		{
			return mapName;
		}
		set
		{
			mapName = value;
			transform.GetChild(0).GetComponent<Text>().text = value;
		}
	}

	#endregion

    /********** MARK: Class Functions **********/
    #region Class Functions

    public void Select()
	{
		menu.SelectItem(mapName);
	}

    #endregion
}