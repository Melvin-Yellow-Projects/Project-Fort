/**
 * File Name: GameMode.cs
 * Description: Configuration script for handling the game mode settings
 * 
 * Authors: Will Lacey
 * Date Created: December 6, 2020
 * 
 * Additional Comments: 
 * 
 *      Previously known as GameMode.cs
 **/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

[CreateAssetMenu(menuName = "Game Settings")]
public class GameSettings : ScriptableObject
{
    /************************************************************/
    #region Game Variables

    //[SerializeField] bool hasMaximumRounds = false;

    //[SerializeField] int maxRounds = 0;

    [Tooltip("turnsPerRound")]
    [SerializeField] public int turnsPerRound = 3;

    [SerializeField] public int movesPerTurn = 5;

    //[SerializeField] int minMovesPerTurn = 2;

    [SerializeField] public bool isUsingTurnTimer = false;

    [SerializeField] public int turnTimerLength = 10;

    [SerializeField] public int startingPlayerResources = 1200;

    //[SerializeField] bool isHotseat = false;

    //[SerializeField] bool canPlayerBreak = false;

    //[SerializeField] float playerBreakingTimerLength;

    #endregion
}

/// <summary>
/// 
/// </summary>
//public static class GameSettingsSerializer
//{
//    /************************************************************/
//    #region HexCellData

//    public static void WriteUnitData(this NetworkWriter writer, GameSettings settings)
//    {
//        writer.WriteByte((byte)settings.turnsPerRound);
//        writer.WriteByte((byte)settings.movesPerTurn);
//        writer.WriteBoolean(settings.isUsingTurnTimer);
//        writer.WriteInt32(settings.turnTimerLength);
//        writer.WriteInt32(settings.startingPlayerResources);
//    }

//    public static GameSettings ReadUnitData(this NetworkReader reader)
//    {
//        GameSettings settings = ScriptableObject.CreateInstance<GameSettings>();

//        settings.turnsPerRound = reader.ReadByte();
//        settings.movesPerTurn = reader.ReadByte();
//        settings.isUsingTurnTimer = reader.ReadBoolean();
//        settings.turnTimerLength = reader.ReadInt32();
//        settings.startingPlayerResources = reader.ReadInt32();

//        return settings;
//    }

//    #endregion
//}