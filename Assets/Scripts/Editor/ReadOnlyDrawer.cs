/**
 * File Name: ReadOnlyDrawer.cs
 * Description: Editor-only script; Draws a non-editable, "ReadOnly" variable
 * 
 * Authors: Will Lacey
 * Date Created: September 11, 2020
 * 
 * Additional Comments:
 *      TODO: add a parameter that can change the text
 **/

using UnityEditor;
using UnityEngine;

/// <summary>
/// Draws a displayable, non-editable value within the Unity Editor
/// </summary>
[CustomPropertyDrawer(typeof(ReadOnlyAttribute))]
public class ReadOnlyDrawer : PropertyDrawer
{
    /********** MARK: Unity Functions **********/
    #region Unity Functions

    /// <summary>
    /// Unity Function; OnGUI is called for rendering and handling GUI events
    /// </summary>
    /// <param name="position">where to draw the data in the Editor</param>
    /// <param name="property">Serialized Property data to draw</param>
    /// <param name="label">Editor GUI label to append to data</param>
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        string text;

        switch (property.propertyType)
        {
            /* Boolean Property Type */
            case SerializedPropertyType.Boolean:
                text = property.boolValue.ToString();
                break;

            /* Integer Property Type */
            case SerializedPropertyType.Integer:
                text = property.intValue.ToString();
                break;

            /* Float Property Type */
            case SerializedPropertyType.Float:
                text = property.floatValue.ToString();
                break;

            /* String Property Type */
            case SerializedPropertyType.String:
                text = property.stringValue;
                break;

            /* Enum Property Type */
            case SerializedPropertyType.Enum:
                text = property.enumDisplayNames[property.enumValueIndex];
                break;

            /* Color Property Type */
            case SerializedPropertyType.Color:
                text = property.colorValue.ToString();
                break;

            /* Unknown Property Type */
            default:
                text = "unknown property";
                break;
        }

        // assign text label to Unity Editor GUI
        EditorGUI.LabelField(position, label, new GUIContent(text));
    }

    #endregion

    /********** MARK: Unused Functions **********/
    #region Unused Functions

    //public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    //{
    //    return EditorGUI.GetPropertyHeight(property, label, true);
    //}

    //public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    //{
    //    GUI.enabled = false;
    //    EditorGUI.PropertyField(position, property, label, true);
    //    GUI.enabled = true;
    //}

    #endregion
}