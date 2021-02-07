/**
 * File Name: HumanPlayer.cs
 * Description: 
 * 
 * Authors: Will Lacey
 * Date Created: December 20, 2020
 * 
 * Additional Comments: 
 **/

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using System;
using Mirror;

public class HumanPlayer : Player
{
    /************************************************************/
    #region Variables

    [Header("Cached References")]
    [SerializeField] MapCamera mapCamera = null;

    public HexCell currentCell; // HACK this shouldnt be public

    Unit selectedUnit;

    bool hasCurrentCellUpdated = false;

    Controls controls;

    #endregion
    /************************************************************/
    #region Properties

    public bool HasCreatedMap { get; [Server] set; } = false;

    #endregion
    /************************************************************/
    #region Unity Functions

    protected void OnEnable()
    {
        mapCamera.enabled = true;
        PlayerMenu.MyPlayer = this;
        PlayerMenu.RefreshResourcesText();
    }

    protected void LateUpdate() 
    {
        // this is in late update so the player cursor always shows on top of hexes

        // other human players should be disabled 
        //if (!hasAuthority) return; 

        // HACK: is this still needed?
        // verify pointer is not on top of GUI
        if (EventSystem.current.IsPointerOverGameObject()) return;

        UpdateCurrentCell();
        if (selectedUnit) DoPathfinding();
    }

    protected override void OnDestroy()
    {
        if (currentCell) currentCell.DisableHighlight();
        base.OnDestroy();
    }

    #endregion
    /************************************************************/
    #region Server Functions

    [Command] // HACK: i dont like this function here
    public void CmdStartGame() 
    {
        if (!GetComponent<PlayerInfo>().IsPartyLeader) return;

        if (GameNetworkManager.IsGameInProgress) return;

        GameNetworkManager.Singleton.ServerStartGame();
    }

    #endregion
    /************************************************************/
    #region Client Input Functions

    [Client]
    private void Command(InputAction.CallbackContext context)
    {
        if (!currentCell) return;

        if (GameManager.IsEconomyPhase)
        {
            CmdTryBuyUnit(PlayerMenu.UnitId, currentCell);
        }
        else
        {
            if (selectedUnit)
            {
                CmdSetAction(UnitData.Instantiate(selectedUnit));
                PlayerMenu.RefreshMoveCountText();
                DeselectUnit();
            }
            else
            {
                //DeselectUnitAndClearItsPath();
                SelectUnit(currentCell.MyUnit);
            }
        }
    }

    [Client]
    private void Cancel(InputAction.CallbackContext context)
    {
        if (GameManager.IsEconomyPhase && currentCell)
        {
            CmdTrySellUnit(currentCell);
        }
        else
        {
            DeselectUnitAndClearItsPath();
        }
    }

    #endregion
    /************************************************************/
    #region Client Functions

    public override void OnStartLocalPlayer()
    {
        AuthoritySubscribe();
    }

    [Client]
    private void UpdateCurrentCell()
    {
        HexCell cell = HexGrid.Singleton.GetCellUnderMouse();

        // HACK: this color is hardcoded, it should probably reflect the team color?
        if (cell && cell.IsExplored) cell.EnableHighlight(new Color(1f, 0f, 0f, 0.6f));

        if (cell != currentCell)
        {
            if (currentCell) currentCell.DisableHighlight();

            currentCell = cell;
            hasCurrentCellUpdated = true; // whether or not current cell has updated
        }
        else
        {
            hasCurrentCellUpdated = false;
        }
    }

    [Client]
    private void DoPathfinding()
    {
        if (!hasCurrentCellUpdated || currentCell == null) return;

        if (Input.GetKey("left shift")) // HACK: this is a hardcoded input
        {
            selectedUnit.Movement.Path.AddCellToPath(currentCell, canBackTrack: true);
        }
        else
        {
            selectedUnit.Movement.Path.AddCellToPath(currentCell, canBackTrack: false);
        }

        selectedUnit.Movement.Path.Show();
    }

    [Client]
    private void SelectUnit(Unit unit)
    {
        if (!MyUnits.Contains(unit)) return;
        //if (!unit) return; // THIS LINE IS FOR DEBUG PURPOSES (allows you to control enemies)

        if (!unit.Movement.CanMove) return;

        if (unit) CmdClearAction(UnitData.Instantiate(unit));

        if (!CanMove()) return;

        selectedUnit = unit;
        selectedUnit.IsSelected = true;
    }

    [Client]
    private void DeselectUnit()
    {
        if (!selectedUnit) return;

        selectedUnit.IsSelected = false;
        selectedUnit = null;
    }

    [Client]
    private void DeselectUnitAndClearItsPath()
    {
        if (!selectedUnit) return;

        selectedUnit.Movement.Path.Clear();

        DeselectUnit();

        //Debug.Log("There is a Unit to DeselectUnitAndClearItsPath");
    }

    [Client] 
    public void EndTurnButtonPressed()
    {
        if (HasEndedTurn) CmdCancelEndTurn();
        else CmdEndTurn();
    }

    #endregion
    /************************************************************/
    #region Event Handler Functions

    protected override void AuthoritySubscribe()
    {
        GameManager.ClientOnStartRound += HandleClientOnStartRound;
        GameManager.ClientOnStopEconomyPhase += HandleClientOnStopEconomyPhase;
        GameManager.ClientOnPlayTurn += HandleClientOnPlayTurn;
        GameManager.ClientOnStopTurn += HandleClientOnStopTurn;

        controls = new Controls();
        controls.Player.Command.performed += Command;
        controls.Player.Cancel.performed += Cancel;
        controls.Enable();

        base.AuthoritySubscribe();
    }

    protected override void AuthorityUnsubscribe()
    {
        GameManager.ClientOnStartRound -= HandleClientOnStartRound;
        GameManager.ClientOnStopEconomyPhase -= HandleClientOnStopEconomyPhase;
        GameManager.ClientOnPlayTurn -= HandleClientOnPlayTurn;
        GameManager.ClientOnStopTurn -= HandleClientOnStopTurn;

        controls.Dispose();

        base.AuthorityUnsubscribe();
    }

    [Client]
    private void HandleClientOnStartRound()
    {
        foreach (Fort fort in MyForts) fort.ShowBuyCells();
        foreach (Unit unit in MyUnits) unit.Movement.Display.HideDisplay();
    }

    [Client]
    private void HandleClientOnStopEconomyPhase()
    {
        foreach (Fort fort in MyForts) fort.HideBuyCells();
        foreach (Unit unit in MyUnits) unit.Movement.Display.ShowDisplay();
    }

    [Client]
    private void HandleClientOnPlayTurn()
    {
        // HACK: i dont think you need to clear it's path, the path shouldn't be set
        DeselectUnitAndClearItsPath(); 
        controls.Disable();
    }

    [Client]
    private void HandleClientOnStopTurn()
    {
        controls.Enable();
    }

    [Client]
    protected override void HookOnResources(int oldValue, int newValue)
    {
        if (PlayerMenu.MyPlayer) PlayerMenu.RefreshResourcesText();
    }

    [Client]
    protected override void HookOnMoveCount(int oldValue, int newValue)
    {
        PlayerMenu.RefreshMoveCountText();
    }

    #endregion
}