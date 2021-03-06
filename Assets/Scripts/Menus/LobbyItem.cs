﻿/**
 * File Name: LobbyItem.cs
 * Description: 
 * 
 * Authors: Will Lacey
 * Date Created: February 1, 2021
 * 
 * Additional Comments: 
 **/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Mirror;

public class LobbyItem : MonoBehaviour
{
    /************************************************************/
    #region Variables

    [SerializeField] TMP_Text nameText = null;

    [SerializeField] Image teamColorImage = null;
    [SerializeField] Color defaultColor = new Color();

    [SerializeField] Image partyLeaderImage = null;

    [SerializeField] RawImage steamImage = null;

    Player player = null;

    #endregion
    /************************************************************/
    #region Class Functions

    public void SetPlayer(Player player)
    {
        this.player = player;

        nameText.text = player.Info.PlayerName;
        nameText.GetComponent<EllipsisSetter>().enabled = false;

        teamColorImage.color = player.MyTeam.TeamColor;

        steamImage.texture = player.Info.DisplayTexture;

        partyLeaderImage.gameObject.SetActive(player.Info.IsPartyLeader);
    }

    public void ClearPlayer()
    {
        player = null;

        nameText.text = "Waiting For Player";
        nameText.GetComponent<EllipsisSetter>().enabled = true;

        teamColorImage.color = defaultColor;

        steamImage.texture = null;

        partyLeaderImage.gameObject.SetActive(false);
    }

    public void Action()
    {
        if (!player) return;

        PlayerInfo clientPlayerInfo = NetworkClient.connection.identity.GetComponent<PlayerInfo>();

        if (player.hasAuthority)
        {
            Debug.Log($"{clientPlayerInfo.name} is attempting to change teams");
            // if this is my button, ask server to change my color
            player.MyTeam.CmdChangeTeam();
        }
        else if (clientPlayerInfo.IsPartyLeader)
        {
            Debug.Log($"{clientPlayerInfo.name} is attempting to change the party leader");

            // else am i the party leader? if so, give the leader status to another player
            clientPlayerInfo.CmdChangePartyLeaderToNewPlayer(player.netIdentity);
        }
    }

    #endregion
}
