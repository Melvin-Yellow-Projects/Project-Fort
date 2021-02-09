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
    #region Class Events
    
    /// <summary>
    /// Event for when a client changes team
    /// </summary>
    /// <subscriber class="LobbyMenu">updates a player's team color</subscriber>
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
            Color color;
            switch (id)
            {
                case 0:
                    return Color.gray;
                case 1:
                    ColorUtility.TryParseHtmlString("#67A383FF", out color); // sea green
                    //return Color.blue;
                    return color;
                case 2:
                    ColorUtility.TryParseHtmlString("#732B1CFF", out color); // adobe red
                    //return Color.red;
                    return color;
                case 3:
                    ColorUtility.TryParseHtmlString("#8ABFD1FF", out color); // beige blue
                    //return Color.green;
                    return color;
                case 4:
                    ColorUtility.TryParseHtmlString("#B097C9FF", out color); // lavender
                    //return Color.yellow;
                    return color;
                case 5:
                    ColorUtility.TryParseHtmlString("#A37476FF", out color); // rose gold
                    //return Color.magenta;
                    return color;
                case 6:
                    ColorUtility.TryParseHtmlString("#033882FF", out color); // saphire
                    //return Color.cyan;
                    return color;
                case 7:
                    ColorUtility.TryParseHtmlString("#C9A253FF", out color); // gold
                    //return Color.red + Color.yellow;
                    return color;
                case 8:
                    ColorUtility.TryParseHtmlString("#053D1BFF", out color); // rust/emerald green
                    //return new Color(143f / 255f, 0.4f, 1f, 1); // lavender
                    return color;
                case 9:
                    return Color.black; // this team is for when a unit dies
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

        Debug.LogWarning($"{name} is Changing Teams!");

        //id += 1;

        id = (id == maxTeams) ? 1 : id + 1;

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
