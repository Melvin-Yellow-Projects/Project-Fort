/**
 * File Name: ReadOnlyAttribute.cs
 * Description: Script to mark variables as "ReadOnly" within the Unity Editor if they are
 *                  serialized
 * 
 * Authors: Will Lacey
 * Date Created: September 11, 2020
 * 
 * Additional Comments:
 *      TODO: force serialization with the ReadOnly PropertyAttribute
 *      TODO: add the ability to comment the variable
 *      TODO: figure out how to put this script in the Editor folder
 **/

using UnityEngine;

/// <summary>
///     Makes a serialized value non-editable within the Unity Editor; variable MUST be serialized
/// </summary>
public class ReadOnlyAttribute : PropertyAttribute
{

}