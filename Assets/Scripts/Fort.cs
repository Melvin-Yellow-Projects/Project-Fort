/**
 * File Name: Fort.cs
 * Description: 
 * 
 * Authors: Will Lacey
 * Date Created: December 13, 2020
 * 
 * Additional Comments: 
 **/

using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using System;

/// <summary>
/// 
/// </summary>
[RequireComponent(typeof(Team), typeof(ColorSetter))]
public class Fort : MonoBehaviour
{
    /********** MARK: Variables **********/
    #region Variables

    HexCell myCell = null;
    float orientation;

    #endregion

    /********** MARK: Class Events **********/
    #region Class Events

    /// <summary>
    /// Event for when a fort is spawned, called in the Start Method
    /// </summary>
    /// <subscriber class="Grid">adds the fort to the list of forts on the grid</subscriber>
    public static event Action<Fort> OnFortSpawned;

    /// <summary>
    /// Event for when a fort is despawned, called in the OnDestroy Method
    /// </summary>
    /// <subscriber class="Grid">removes the fort to the list of forts on the grid</subscriber>
    public static event Action<Fort> OnFortDespawned;

    //public static event Action OnFortCaptured;

    #endregion

    /********** MARK: Properties **********/
    #region Properties

    public static Fort Prefab { get; set; }

    public Team MyTeam { get; private set; }

    public HexCell MyCell
    {
        get
        {
            return myCell;
        }
        set
        {
            myCell = value;
            myCell.MyFort = this;
            ValidateLocation();
        }
    }

    public float Orientation
    {
        get
        {
            return orientation;
        }
        set
        {
            orientation = value;
            transform.localRotation = Quaternion.Euler(0f, value, 0f);
        }
    }

    #endregion

    /********** MARK: Unity Functions **********/
    #region Unity Functions

    private void Awake()
    {
        MyTeam = GetComponent<Team>();

        Subscribe();
    }

    private void Start()
    {
        OnFortSpawned?.Invoke(this);
    }

    private void OnDestroy()
    {
        myCell.MyFort = null;
        OnFortDespawned?.Invoke(this);

        Unsubscribe();
    }

    #endregion

    /********** MARK: Class Functions **********/
    #region Class Functions

    public void ValidateLocation()
    {
        transform.localPosition = myCell.Position;
    }

    #endregion

    /********** MARK: Save/Load Functions **********/
    #region Save/Load Functions

    public void Save(BinaryWriter writer)
    {
        myCell.coordinates.Save(writer);
        writer.Write(orientation);
        writer.Write((byte)MyTeam.TeamIndex);
    }

    public static void Load(BinaryReader reader, int header)
    {
        HexCoordinates coordinates = HexCoordinates.Load(reader);
        float orientation = reader.ReadSingle();

        Fort fort = Instantiate(Prefab);
        if (header >= 4) fort.MyTeam.TeamIndex = reader.ReadByte();

        fort.MyCell = HexGrid.Singleton.GetCell(coordinates);
        fort.Orientation = orientation;

        HexGrid.Singleton.ParentTransformToGrid(fort.transform);
    }

    #endregion

    /********** MARK: Handle Functions **********/
    #region Handle Functions

    private void Subscribe()
    {
        GameManager.OnStopMoveUnits += HandleOnStopMoveUnits;
    }

    private void Unsubscribe()
    {
        GameManager.OnStopMoveUnits -= HandleOnStopMoveUnits;
    }

    public void HandleOnStopMoveUnits()
    {
        Unit unit = myCell.MyUnit;

        if (!unit) return;

        MyTeam.TeamIndex = unit.MyTeam.TeamIndex;
    }

    #endregion
}
