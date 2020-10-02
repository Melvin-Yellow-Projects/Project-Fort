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