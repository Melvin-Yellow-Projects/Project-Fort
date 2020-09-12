﻿/**
 * File Name: ReadOnlyAttribute.cs
 * Description: Script to mark variables as "ReadOnly" within the Unity Editor if they are
 *                  serialized
 * 
 * Authors: Will Lacey
 * Date Created: September 11, 2020
 * 
 * Additional Comments:
 *      UNDONE: force serialization with the ReadOnly PropertyAttribute
 *      UNDONE: add the ability to comment the variable
 **/

using UnityEngine;

/// <summary>
///     Makes a serialized value non-editable within the Unity Editor; variable MUST be serialized
/// </summary>
public class ReadOnlyAttribute : PropertyAttribute
{

}