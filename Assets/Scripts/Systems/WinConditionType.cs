/**
 * File Name: WinConditionType.cs
 * Description: 
 * 
 * Authors: Will Lacey
 * Date Created: January 20, 2021
 * 
 * Additional Comments: 
 * 
 *      Chess
 *          Win/Lose
 *              Checkmate, Resignation, Timeout
 *          Draw
 *              Stalemate, Insufficient material, 50 move-rule, Repetition, Agreement
 *              
 *      Fort
 *          Win/Lose
 *              (Conquest, Capture, Monopoly), (Annihilation, Routed), Capensis, Wizard, 
 *              (Resignation, Surrender), Timeout, Disconnect
 *              Disconnect
 *          Draw
 *              Draw, Insufficient material, Turn Limit, Repetition, Agreement
 **/

/// <summary>
/// Enum for the different types of Win Conditions
/// </summary>
public enum WinConditionType
{
    Draw, Conquest, Routed, Capensis, Resignation, Disconnect
}
