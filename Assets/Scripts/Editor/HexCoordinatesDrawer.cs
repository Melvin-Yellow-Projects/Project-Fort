﻿/**
 * File Name: HexCoordinatesDrawer.cs
 * Description: Editor-only script; Draws a HexCell's coordinates within the Editor
 * 
 * Authors: Catlike Coding, Will Lacey
 * Date Created: September 10, 2020
 * 
 * Additional Comments: 
 *      The original version of this file can be found here:
 *      https://catlikecoding.com/unity/tutorials/hex-map/ within Catlike Coding's tutorial series:
 *      Hex Map; this file has been updated it to better fit this project
 **/

using UnityEngine;
using UnityEditor;

[CustomPropertyDrawer(typeof(HexCoordinates))]
public class HexCoordinatesDrawer : PropertyDrawer
{
    /********** MARK: Unity Functions **********/
    #region Unity Functions

    /// <summary>
    ///     Unity Function; OnGUI is called for rendering and handling GUI events
    /// </summary>
    /// <param name="position">where to draw in the Editor</param>
    /// <param name="property">what to draw</param>
    /// <param name="label"></param>
	public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
	{
        // this code draws the coordinate in the editor
		HexCoordinates coordinates = new HexCoordinates(
			property.FindPropertyRelative("x").intValue, // gets HexCoordinate serialized value x
            property.FindPropertyRelative("z").intValue  // gets HexCoordinate serialized value z
        );

        // 
		position = EditorGUI.PrefixLabel(position, label);

        // sets the text in the Editor
		GUI.Label(position, coordinates.ToString());
	}

    #endregion
}