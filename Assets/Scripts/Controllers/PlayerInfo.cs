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

    [SyncVar(hook = nameof(HookOnIsPartyOwner))]
    bool isPartyOwner = false;

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
    public static event Action ClientOnPlayerInfoUpdate;

    #endregion
    /************************************************************/
    #region Class Events

    public bool IsPartyOwner
    {
        get
        {
            return isPartyOwner;
        }

        [Server]
        set
        {
            isPartyOwner = value;
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

    private void Subscribe()
    {

    }

    private void Unsubscribe()
    {

    }

    private void HookOnIsPartyOwner(bool oldValue, bool newValue)
    {
        if (!hasAuthority) return;

        ClientOnPlayerInfoUpdate?.Invoke();
    }

    private void HookOnPlayerName(string oldValue, string newValue)
    {
        ClientOnPlayerInfoUpdate?.Invoke();
        name = playerName;
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
