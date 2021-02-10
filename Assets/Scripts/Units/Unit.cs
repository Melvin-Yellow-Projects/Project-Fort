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

    [Header("Settings")]
    [Tooltip("ID for this unit")]
    [SerializeField] int id = 0;

    [Tooltip("class title name for this unit")]
    [SerializeField] string classTitle = null;

    [Tooltip("piece title name for this unit")]
    [SerializeField] string pieceTitle = null;

    [Tooltip("how much this unit costs")]
    [SerializeField] int resources = 0;

    //[Tooltip("sprite asset for the unit")]
    //[SerializeField] Sprite artwork = null;

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

    public static List<Unit> Prefabs { get; set; }

    public int Id
    {
        get
        {
            return id;
        }
    }

    public string ClassTitle
    {
        get
        {
            return classTitle;
        }
    }

    public string PieceTitle
    {
        get
        {
            return pieceTitle;
        }
    }

    public int Resources
    {
        get
        {
            return resources;
        }
    }

    public Team MyTeam { get; private set; }

    public ColorSetter MyColorSetter { get; private set; }

    public UnitMovement Movement { get; private set; }

    public UnitCombat CombatHandler { get; private set; }

    public bool IsSelected
    {
        get
        {
            return isSelected;
        }
        set
        {
            isSelected = value;

            //Movement.RefreshPath(); // shows that the unit is selected or not

            if (value) Movement.ShowMovementRange();
            else Movement.HideMovementRange();
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
        CombatHandler = GetComponentInChildren<UnitCombat>();

        if (!MyTeam || !MyColorSetter || !Movement || !CombatHandler)
            Debug.LogError($"unit {name} is missing an essential component");

        HexGrid.Units.Add(this); // HACK: should this be an event?
        name = $"unit {UnityEngine.Random.Range(0, 100000)}";
    }

    private void OnDestroy()
    {
        HexGrid.Units.Remove(this); // HACK: should this be an event?
    }

    #endregion
    /************************************************************/
    #region Server Functions

    public override void OnStartServer()
    {
        OnUnitSpawned?.Invoke(this);

        ValidateLocation();
    }

    public override void OnStopServer()
    {
        OnUnitDepawned?.Invoke(this);
    }

    #endregion
    /************************************************************/
    #region Client Functions

    public override void OnStartClient()
    {
        if (!isServer) OnUnitSpawned?.Invoke(this);
    }

    public override void OnStopClient()
    {
        if (!isServer) OnUnitDepawned?.Invoke(this);
    }

    #endregion
    /************************************************************/
    #region Class Functions

    public void ValidateLocation()
    {
        //UnitPathfinding.IncreaseVisibility(MyCell, Movement.VisionRange); // FIXME: vision no work
        if (isServer || !GameSession.IsOnline) transform.localPosition = MyCell.Position;
    }

    public void Die(bool isPlayingAnimation = true)
    {
        MyTeam.SetTeam(9); // black team
        GetComponent<UnitDeath>().Die(isPlayingAnimation);
    }

    #endregion
    /************************************************************/
    #region Save/Load Functions

    public void Save(BinaryWriter writer)
    {
        MyCell.coordinates.Save(writer);
        writer.Write((byte)Id);
        writer.Write((byte)MyTeam.Id);
        writer.Write(Movement.Orientation);
    }

    public static void Load(BinaryReader reader, int header)
    {
        HexCoordinates coordinates = HexCoordinates.Load(reader);

        Unit unit = Instantiate(Prefabs[reader.ReadByte()]);

        unit.MyTeam.SetTeam(reader.ReadByte());

        unit.Movement.MyCell = HexGrid.Singleton.GetCell(coordinates);
        unit.Movement.Orientation = reader.ReadSingle();

        // HACK: figure out to do with ParentTransformToGrid line (Unit.cs)
        //HexGrid.Singleton.ParentTransformToGrid(unit.transform);

        //NetworkServer.Spawn(unit.gameObject);
    }

    #endregion
}


