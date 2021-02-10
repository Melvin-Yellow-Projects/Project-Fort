/**
 * File Name: PlayerInfo.cs
 * Description: 
 * 
 * Authors: Will Lacey
 * Date Created: December 22, 2020
 * 
 * Additional Comments: 
 * 
 *      HACK: Properties don't need to be server protected since SyncVar is from server to client
 **/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using System;
using Steamworks;

public class PlayerInfo : NetworkBehaviour
{
    /************************************************************/
    #region Variables

    [SyncVar(hook = nameof(HookOnIsPartyLeader))]
    bool isPartyLeader = false;

    [SyncVar(hook = nameof(HookOnPlayerName))]
    string playerName;

    [SyncVar(hook = nameof(HandleSteamIdUpdated))]
    ulong steamId;

    //[SyncVar]
    //Color playerColor = new Color();

    Texture2D displayTexture;

    protected Callback<AvatarImageLoaded_t> avatarImageLoaded;

    #endregion
    /************************************************************/
    #region Class Events

    /// <summary>
    /// Event for when a client disconnects from the server
    /// </summary>
    /// <subscriber class="LobbyMenu">refreshes lobby informtion</subscriber>
    public static event Action ClientOnPlayerInfoUpdate;

    /// <summary>
    /// Event for when a client disconnects from the server
    /// </summary>
    /// <subscriber class="LobbyMenu">refreshes lobby informtion</subscriber>
    public static event Action ClientOnPartyLeaderChanged;

    #endregion
    /************************************************************/
    #region Class Events

    public bool IsPartyLeader
    {
        get
        {
            return isPartyLeader;
        }

        [Server]
        set
        {
            isPartyLeader = value;
        }
    }

    public string PlayerName
    {
        get
        {
            return playerName;
        }

        [Server]
        set
        {
            playerName = value;
        }
    }

    public ulong SteamId
    {
        get
        {
            return steamId;
        }

        [Server]
        set
        {
            steamId = value;
        }
    }

    public Texture2D DisplayTexture
    {
        get
        {
            return displayTexture;
        }
    }

    #endregion
    /************************************************************/
    #region Server Functions

    [Command]
    public void CmdChangePartyLeaderToNewPlayer(NetworkIdentity playerNetId)
    {
        if (GameNetworkManager.HasLaunchedGame) return;
        if (!playerNetId.TryGetComponent(out PlayerInfo playerInfo)) return;

        IsPartyLeader = false;
        playerInfo.IsPartyLeader = true;

        Debug.LogWarning($"{name} has changed the party leader!");
    }

    #endregion
    /************************************************************/
    #region Client Functions

    public override void OnStartClient()
    {
        if (GameNetworkManager.IsUsingSteam)
            avatarImageLoaded = Callback<AvatarImageLoaded_t>.Create(OnAvatarImageLoaded);
    }

    public override void OnStopClient()
    {
        ClientOnPlayerInfoUpdate?.Invoke();
    }

    #endregion
    /************************************************************/
    #region Class Functions

    private void OnAvatarImageLoaded(AvatarImageLoaded_t callback)
    {
        if (callback.m_steamID.m_SteamID != steamId) return;

        displayTexture = GetSteamImageAsTexture(callback.m_iImage);
        ClientOnPlayerInfoUpdate?.Invoke();
    }

    public static Texture2D GetSteamImageAsTexture(int iImage)
    {
        Texture2D texture = null;

        bool isValid = SteamUtils.GetImageSize(iImage, out uint width, out uint height);

        if (isValid)
        {
            byte[] image = new byte[width * height * 4];

            isValid = SteamUtils.GetImageRGBA(iImage, image, (int)(width * height * 4));

            if (isValid)
            {
                texture = new Texture2D((int)width, (int)height, TextureFormat.RGBA32, false, true);

                texture.LoadRawTextureData(image);
                texture.Apply();
            }
        }

        return texture;
    }

    #endregion
    /************************************************************/
    #region Event Handler Functions

    private void HookOnIsPartyLeader(bool oldValue, bool newValue)
    {
        Debug.Log("Setting Party Leader");
        ClientOnPartyLeaderChanged?.Invoke();
    }

    private void HookOnPlayerName(string oldValue, string newValue)
    {
        name = playerName;

        ClientOnPlayerInfoUpdate?.Invoke();
    }

    private void HandleSteamIdUpdated(ulong oldSteamId, ulong newSteamId)
    {
        CSteamID steamId = new CSteamID(newSteamId);

        playerName = SteamFriends.GetFriendPersonaName(steamId);

        int imageId = SteamFriends.GetLargeFriendAvatar(steamId);

        if (imageId == -1) return;

        displayTexture = GetSteamImageAsTexture(imageId);
        ClientOnPlayerInfoUpdate?.Invoke();
    }

    #endregion
}
