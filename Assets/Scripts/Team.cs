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
using Mirror;
using System;

[RequireComponent(typeof(ColorSetter))]
public class Team : NetworkBehaviour
{
    /************************************************************/
    #region Variables

    [SyncVar(hook = nameof(HookOnTeamIndex))]
    int teamIndex = 0;

    #endregion
    /************************************************************/
    #region Properties

    public int TeamIndex
    {
        get
        {
            return teamIndex; 
        }

        [Server]
        set
        {
            if (teamIndex == value) return;

            teamIndex = value;
            ServerRefreshAuthoritativeConnection();
        }
    }

    public Color MyColor
    {
        get
        {
            return (teamIndex == 1) ? Color.blue : Color.red;
        }
    }

    // HACK: this might not be present on a GameObject that doesn't need to have it's color set
    public ColorSetter MyColorSetter 
    {
        get
        {
            return GetComponent<ColorSetter>();
        }
    }

    public NetworkConnection AuthoritiveConnection
    {
        get
        {
            for (int i = 0; i < GameNetworkManager.HumanPlayers.Count; i++)
            {
                if (teamIndex == GameNetworkManager.HumanPlayers[i].MyTeam.teamIndex)
                {
                    Debug.Log($"Grabbing Authoritative Connection for {name}");
                    return GameNetworkManager.HumanPlayers[i].connectionToClient;
                }
            }

            Debug.LogWarning($"Authoritative Connection is null for {name}");

            return null;
        }
    }

    #endregion
    /************************************************************/
    #region Server Functions

    [Server]
    public void ServerRefreshAuthoritativeConnection()
    {
        Debug.LogWarning($"Attempting to refresh AuthoritativeConnection for {name}");
        if (!isServer || GetComponent<HumanPlayer>()) return;

        Debug.Log($"Refreshing AuthoritativeConnection for {name}");

        netIdentity.RemoveClientAuthority();
        netIdentity.AssignClientAuthority(AuthoritiveConnection);
    }

    #endregion
    /************************************************************/
    #region Event Handler Functions

    private void HookOnTeamIndex(int oldValue, int newValue)
    {
        if (MyColorSetter) MyColorSetter.SetColor(MyColor);

    }

    #endregion
    /************************************************************/
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
