/**
 * File Name: Unit.cs
 * Description: Script for managing a hex unit
 * 
 * Authors: Catlike Coding, Will Lacey
 * Date Created: October 6, 2020
 * 
 * Additional Comments: 
 *      The original version of this file can be found here:
 *      https://catlikecoding.com/unity/tutorials/hex-map/ within Catlike Coding's tutorial series:
 *      Hex Map; this file has been updated it to better fit this project
 **/

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System;
using Mirror;

/// <summary>
/// a unit that is able to interact with a hex map 
/// </summary>
[RequireComponent(typeof(Team))]
[RequireComponent(typeof(ColorSetter))]
[RequireComponent(typeof(UnitDisplay))]
public class Unit : NetworkBehaviour
{
    /************************************************************/
    #region Variables

    bool isSelected = false;

    #endregion
    /************************************************************/
    #region Class Events

    /// <summary>
    /// Event for when a unit is spawned, called in the Start Method
    /// </summary>
    /// <subscriber class="Player">adds unit to player's list of owned units</subscriber>
    /// <subscriber class="Grid">adds unit to list of units</subscriber>
    public static event Action<Unit> OnUnitSpawned;

    /// <summary>
    /// Event for when a unit is despawned, called in the OnDestroy Method
    /// </summary>
    /// <subscriber class="Player">removes unit from player's list of owned units</subscriber>
    /// <subscriber class="Grid">removes unit from list of units</subscriber>
    public static event Action<Unit> OnUnitDepawned;

    #endregion
    /************************************************************/
    #region Properties

    public static Unit Prefab { get; set; }

    public Team MyTeam { get; private set; }

    public ColorSetter MyColorSetter { get; private set; }

    public UnitMovement Movement { get; private set; }

    public UnitCollisionHandler CollisionHandler { get; private set; }

    public bool IsSelected
    {
        get
        {
            return isSelected;
        }
        set
        {
            isSelected = value;
            Movement.RefreshPath(); 
        }
    }

    public HexCell MyCell
    {
        get
        {
            return Movement.MyCell;
        }
        set
        {
            Movement.MyCell = value;
        }
    }

    #endregion
    /************************************************************/
    #region Unity Functions

    private void Awake()
    {
        MyTeam = GetComponent<Team>();
        MyColorSetter = GetComponent<ColorSetter>();
        Movement = GetComponent<UnitMovement>();
        CollisionHandler = GetComponentInChildren<UnitCollisionHandler>();
    }

    private void Start()
    {
        OnUnitSpawned?.Invoke(this);
    }

    private void OnDestroy()
    {
        OnUnitDepawned?.Invoke(this);
    }

    #endregion
    /************************************************************/
    #region Class Functions

    [Server]
    public void ServerValidateLocation()
    {
        UnitPathfinding.IncreaseVisibility(MyCell, Movement.VisionRange);
        transform.localPosition = MyCell.Position;
    }

    public void Die(bool isPlayingAnimation = true)
    {
        //StopAllCoroutines(); // HACK: i think this line can be safely removed

        GetComponent<UnitDeath>().Die(isPlayingAnimation);
    }

    #endregion
    /************************************************************/
    #region Save/Load Functions

    public void Save(BinaryWriter writer)
    {
        MyCell.coordinates.Save(writer);
        writer.Write(Movement.Orientation);
        writer.Write((byte)MyTeam.TeamIndex);
    }

    public static void Load(BinaryReader reader, int header)
    {
        HexCoordinates coordinates = HexCoordinates.Load(reader);
        float orientation = reader.ReadSingle();

        Unit unit = Instantiate(Prefab);
        if (header >= 4) unit.MyTeam.TeamIndex = reader.ReadByte();

        unit.Movement.MyCell = HexGrid.Singleton.GetCell(coordinates);
        unit.Movement.Orientation = orientation;

        //HexGrid.Singleton.units.Add(unit); // FIXME: sub in hexgrid happens after spawn

        // HACK: figure out to do with ParentTransformToGrid line (Unit.cs)
        //HexGrid.Singleton.ParentTransformToGrid(unit.transform);

        //NetworkServer.Spawn(unit.gameObject);
    }

    #endregion
}


