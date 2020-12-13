/**
 * File Name: Team.cs
 * Description: 
 * 
 * Authors: Will Lacey
 * Date Created: December 13, 2020
 * 
 * Additional Comments: 
 **/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Team : MonoBehaviour
{
    /********** MARK: Variables **********/
    #region Variables

    int teamIndex = 0;

    #endregion

    /********** MARK: Properties **********/
    #region Properties

    public int TeamIndex
    {
        get
        {
            return teamIndex;
        }
        set
        {
            teamIndex = value;
        }
    }

    public Color MyColor
    {
        get
        {
            return (teamIndex == 0) ? Color.blue : Color.red;
        }
    }

    #endregion

    /********** MARK: Class Functions **********/
    #region Class Functions

    #endregion

    /********** MARK: Comparison Functions **********/
    #region Comparison Functions

    public static bool operator == (Team team1, Team team2)
    {
        if (ReferenceEquals(team1, team2)) return true;
        if (ReferenceEquals(team1, null)) return false;
        if (ReferenceEquals(team2, null)) return false;

        return team1.Equals(team2);
    }

    public static bool operator != (Team team1, Team team2)
    {
        return !(team1 == team2);
    }

    public bool Equals(Team other)
    {
        if (ReferenceEquals(this, other)) return true;
        if (ReferenceEquals(other, null)) return false;

        return this.teamIndex == other.teamIndex;
    }

    public override bool Equals(object obj)
    {
        return Equals(obj as Team);
    }

    public override int GetHashCode()
    {
        unchecked
        {
            int hashCode = teamIndex.GetHashCode();
            //hashCode = (hashCode * 397) ^ length.GetHashCode();
            //hashCode = (hashCode * 397) ^ breadth.GetHashCode();
            return hashCode;
        }
    }

    #endregion
}
