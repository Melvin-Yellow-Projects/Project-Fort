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

    [SyncVar(hook = nameof(HookOnId))]
    int id = 0;

    const int maxTeams = 8;

    #endregion
    /************************************************************/
    #region Properties

    //public static event Action ServerOnChangePlayerTeam;

    public static event Action ClientOnChangeTeam;

    #endregion
    /************************************************************/
    #region Properties

    public int Id
    {
        get
        {
            return id; 
        }
    }

    //public Color TeamColor { get; set; }
    public Color TeamColor
    {
        get
        {
            // TODO: add settable color
            switch (id)
            {
                case 0:
                    return Color.gray;
                case 1:
                    return Color.blue;
                case 2:
                    return Color.red;
                case 3:
                    return Color.green;
                case 4:
                    return Color.yellow;
                case 5:
                    return Color.magenta;
                case 6:
                    return Color.cyan;
                case 7:
                    return Color.red + Color.yellow;
                case 8:
                    return new Color(143f / 255f, 0.4f, 1f, 1); // lavender
            }
            Debug.LogError("team color not found");
            return Color.black;
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

    //HACK: This needs to be updated
    public NetworkConnection AuthoritiveConnection 
    {
        [Server] 
        get
        {
            for (int i = 0; i < GameManager.Players.Count; i++)
            {
                if (id == GameManager.Players[i].MyTeam.id)
                {
                    //Debug.Log($"Grabbing Authoritative Connection for {name}");
                    MyPlayer = GameManager.Players[i];
                    return GameManager.Players[i].connectionToClient;
                }
            }

            //Debug.LogWarning($"Authoritative Connection is null for {name}");

            return null;
        }
    }

    public Player MyPlayer { get; set; }

    #endregion
    /************************************************************/
    #region Server Functions

    [Server]
    public void ServerRefreshAuthoritativeConnection()
    {
        //Debug.LogWarning($"Attempting to refresh AuthoritativeConnection for {name}");
        if (!isServer || GetComponent<HumanPlayer>()) return;

        //Debug.Log($"Refreshing AuthoritativeConnection for {name}");

        netIdentity.RemoveClientAuthority();

        NetworkConnection conn = AuthoritiveConnection;
        if (conn != null) netIdentity.AssignClientAuthority(conn);
    }

    [Command]
    public void CmdChangeTeam()
    {
        if (GameNetworkManager.IsGameInProgress) return;

        id += 1;

        if (id > maxTeams) id = 1;

        //RpcChangeTeam();
    }

    #endregion
    /************************************************************/
    #region Client Functions

    //[ClientRpc]
    //private void RpcChangeTeam()
    //{
    //    ClientOnChangeTeam?.Invoke();
    //}

    #endregion
    /************************************************************/
    #region Class Functions

    public void SetTeam(int id)
    {
        if (this.id == id) return;

        this.id = id;
        ServerRefreshAuthoritativeConnection();
        //if (MyColorSetter) MyColorSetter.SetColor(TeamColor); // TODO: validate if line needed
    }

    public void SetTeam(Team team)
    {
        SetTeam(team.id);
    }

    #endregion
    /************************************************************/
    #region Event Handler Functions

    private void HookOnId(int oldValue, int newValue)
    {
        ClientOnChangeTeam?.Invoke();
        if (MyColorSetter) MyColorSetter.SetColor(TeamColor);
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

        return this.id == other.id;
    }

    public override bool Equals(object obj)
    {
        return Equals(obj as Team);
    }

    public override int GetHashCode()
    {
        unchecked
        {
            int hashCode = id.GetHashCode();
            //hashCode = (hashCode * 397) ^ length.GetHashCode();
            //hashCode = (hashCode * 397) ^ breadth.GetHashCode();
            return hashCode;
        }
    }

    #endregion
}
