/**
 * File Name: UnitMovement.cs
 * Description: 
 * 
 * Authors: Will Lacey
 * Date Created: December 17, 2020
 * 
 * Additional Comments: 
 **/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public abstract class UnitMovement : NetworkBehaviour
{
    /************************************************************/
    #region Variables

    /** Class Parameters **/
    [Header("Gameplay Settings")]
    [Tooltip("ID for this unit")]
    [SerializeField] protected int maxMovement = 4;

    [Tooltip("ID for this unit")]
    [SerializeField] protected int visionRange = 1;

    [Tooltip("ID for this unit")]
    [SerializeField] protected int movesPerStep = 1;

    [Header("Aesthetic Settings")]
    [Tooltip("ID for this unit")]
    [SerializeField] protected float travelSpeed = 6f;

    [Tooltip("ID for this unit")]
    [SerializeField] protected float rotationSpeed = 360f;

    /** Other Variables **/
    protected float orientation;
    protected int currentMovement;

    [SyncVar(hook = nameof(HookOnMyCell))]
    HexCell myCell;

    #endregion
    /************************************************************/
    #region Properties

    public Unit MyUnit { get; private set; }

    public UnitDisplay Display { get; private set; }

    public UnitPath Path { get; private set; }

    public HexCell MyCell
    {
        get
        {
            return myCell;
        }
        set
        {
            // if there is a previous cell...
            if (myCell && myCell.MyUnit == MyUnit)
            {
                //UnitPathfinding.DecreaseVisibility(myCell, visionRange);
                myCell.MyUnit = null;
            }

            // update for new location
            myCell = value;
            myCell.MyUnit = MyUnit; // sets this hex cell's unit to this one

            MyUnit.ValidateLocation(); // FIXME: Why doesn't this work?
            //MyUnit.ServerValidateLocation();
        }
    }

    /// <summary>
    /// A unit's rotation around the Y axis, in degrees
    /// </summary>
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

    // TODO: maybe one day you can get this from orientation and the look angle
    public HexDirection Direction { get; private set; }

    public int VisionRange
    {
        get
        {
            return visionRange;
        }
    }

    public int CurrentMovement
    {
        get
        {
            return currentMovement;
        }
        private set
        {
            currentMovement = Mathf.Clamp(value, 0, maxMovement);

            if (!hasAuthority) return;

            Display.RefreshMovementDisplay(currentMovement);

            // refreshes color given if the unit can move
            MyUnit.MyColorSetter.SetColor(MyUnit.MyTeam.TeamColor, isSaturating: !CanMove);

            if (CanMove) Display.ShowDisplay();
            else Display.HideDisplay();
        }
    }

    public int MaxMovement
    {
        get
        {
            return maxMovement;
        }
    }

    public HexCell EnRouteCell { get; private set; }

    public bool IsEnRoute { get; set; }

    public bool HasAction { get; set; }

    public bool HadActionCanceled { get; private set; } = false; // HACK: i dont like this name

    // HACK: might be better broken up into a property and function FreezeUnit()/CannotMove() 
    public bool CanMove
    {
        get
        {
            return (currentMovement > 0);
        }
        set
        {
            if (value)
            {
                CurrentMovement = maxMovement;
                if (hasAuthority) Display.ShowDisplay();
            }
            else
            {
                CurrentMovement = 0;
                if (hasAuthority) Display.HideDisplay();
            }
            Path.Clear();
        }
    }

    #endregion
    /************************************************************/
    #region Unity Functions

    private void Awake()
    {
        MyUnit = GetComponent<Unit>();
        Path = GetComponent<UnitPath>();

        Display = GetComponent<UnitDisplay>();
        Display.HideDisplay();

        currentMovement = maxMovement; // TODO: create sync var for variable
    }

    private void Start()
    {
        Display.RefreshMovementDisplay(currentMovement);
    }

    private void OnDestroy()
    {
        Unsubscribe();
    }

    #endregion
    /************************************************************/
    #region Server Functions

    public override void OnStartServer()
    {
        Subscribe();

        Orientation = Random.Range(0, 360f);

        // this is called by the LateUpdate's reset in HexCellShaderData
        UnitPathfinding.IncreaseVisibility(MyCell, VisionRange);
    }

    [Server]
    public void ServerDoAction()
    {
        if (!HasAction) return;

        // HACK: removing line causes errors, HasAction should better reflect action status of unit
        if (!Path.HasPath || CurrentMovement == 0) return;

        List<HexCell> cells = new List<HexCell>();
        cells.Add(Path[0]);
        cells.Add(Path[1]);

        StartCoroutine(Route(cells));
    }

    [Server]
    public void ServerCompleteAction()
    {
        if (GetComponent<UnitDeath>().IsDying)
        {
            // TODO: Brute Force repitition, this can be improved
            MyUnit.CombatHandler.gameObject.SetActive(false);
            Debug.Log("Disabling Combat Handler");
            return;
        }

        if (!EnRouteCell) return;

        //Direction = HexMetrics.GetDirection(MyCell, EnRouteCell);
        MyCell = EnRouteCell;
        EnRouteCell = null;
        HadActionCanceled = false;

        TargetCompleteAction(connectionToClient); // TODO: relay this message to allies too

        CurrentMovement--; // FIXME assumes all tiles have the same cost

        if (!Path.HasPath) return;

        Path.RemoveTailCells(numberToRemove: movesPerStep);
        if (hasAuthority) Path.Show();
    }

    [Server]
    public void CancelAction()
    {
        if (!EnRouteCell) return;

        CanMove = false;
        HadActionCanceled = true;

        RpcCancelAction(); // TODO: relay this message to allies too

        StopAllCoroutines();
        StartCoroutine(RouteCanceled());
    }

    /// <summary>
    /// TODO: comment this; apparently a unit's velocity will slow down when changing directions,
    /// why?
    /// </summary>
    /// <returns></returns>
    [Server]
    private IEnumerator Route(List<HexCell> cells)
    {
        IsEnRoute = true;
        Vector3 a, b, c = cells[0].Position;

        // perform lookat
        //yield return LookAt(cells[1].Position);

        // decrease vision HACK: this ? shenanigans is confusing
        UnitPathfinding.DecreaseVisibility(
            (EnRouteCell) ? EnRouteCell : cells[0],
            visionRange
        );

        float interpolator = Time.deltaTime * travelSpeed;
        for (int i = 1; i < cells.Count; i++)
        {
            EnRouteCell = cells[i]; // prevents teleportation

            a = c;
            b = cells[i - 1].Position;
            c = (b + EnRouteCell.Position) * 0.5f;

            UnitPathfinding.IncreaseVisibility(EnRouteCell, visionRange);

            for (; interpolator < 1f; interpolator += Time.deltaTime * travelSpeed)
            {
                //transform.localPosition = Vector3.Lerp(a, b, interpolator);
                transform.localPosition = Bezier.GetPoint(a, b, c, interpolator);
                //Vector3 d = Bezier.GetDerivative(a, b, c, interpolator);
                //d.y = 0f;
                //transform.localRotation = Quaternion.LookRotation(d);
                yield return null;
            }

            UnitPathfinding.DecreaseVisibility(EnRouteCell, visionRange);

            interpolator -= 1f;
        }
        EnRouteCell = cells[cells.Count - 1];

        // arriving at the center if the last cell
        a = c;
        b = EnRouteCell.Position; // We can simply use the destination here.
        c = b;

        UnitPathfinding.IncreaseVisibility(EnRouteCell, visionRange);

        for (; interpolator < 1f; interpolator += Time.deltaTime * travelSpeed)
        {
            //transform.localPosition = Vector3.Lerp(a, b, interpolator);
            transform.localPosition = Bezier.GetPoint(a, b, c, interpolator);
            //Vector3 d = Bezier.GetDerivative(a, b, c, interpolator);
            //d.y = 0f;
            //transform.localRotation = Quaternion.LookRotation(d);
            yield return null;
        }

        transform.localPosition = EnRouteCell.Position;
        //orientation = transform.localRotation.eulerAngles.y;
        IsEnRoute = false;
    }

    [Server]
    private IEnumerator RouteCanceled()
    {
        IsEnRoute = true;

        //yield return LookAt(myCell.Position);

        UnitPathfinding.DecreaseVisibility(EnRouteCell, visionRange);

        Vector3 a = myCell.Position;
        Vector3 b = EnRouteCell.Position;
        Vector3 c = b;
        Vector3 d; // HACK: all of this is so jank
        EnRouteCell = myCell;
        //for (; interpolator > 0; interpolator -= Time.deltaTime * travelSpeed / 10)
        for (float t = 0; t < 1f; t += Time.deltaTime * travelSpeed / 2)
        {
            transform.localPosition = Vector3.Lerp(transform.localPosition, a, t);
            //transform.localPosition = Bezier.GetPoint(a, b, c, interpolator);
            //Vector3 d = Bezier.GetDerivative(a, b, c, interpolator); // this will be backwards
            //d.y = 0f;
            //transform.localRotation = Quaternion.LookRotation(d);
            yield return null;
        }

        UnitPathfinding.IncreaseVisibility(EnRouteCell, visionRange);

        IsEnRoute = false;
    }

    [Server]
    public void LookAt(HexDirection direction)
    {
        // HACK: yea dis is weird, coroutine needs to probably be stopped first
        Vector3 localPoint = HexMetrics.GetBridge(direction);
        StartCoroutine(LookAt(myCell.Position + localPoint));
    }

    [Server]
    IEnumerator LookAt(Vector3 point)
    {
        // locks the y dimension
        point.y = transform.localPosition.y;

        Quaternion fromRotation = transform.localRotation;
        Quaternion toRotation = Quaternion.LookRotation(point - transform.localPosition);

        float angle = Quaternion.Angle(fromRotation, toRotation);

        if (angle > 0f)
        {
            // normalizes the speed so that it's always the same regardless of angle; "To ensure a
            // uniform angular speed, we have to slow down our interpolation as the rotation angle
            // increases."
            float speed = rotationSpeed / angle;

            for (float t = Time.deltaTime * speed; t < 1f; t += Time.deltaTime * speed)
            {
                transform.localRotation = Quaternion.Slerp(fromRotation, toRotation, t);
                yield return null;
            }
        }

        transform.LookAt(point);
        orientation = transform.localRotation.eulerAngles.y;

    }

    [Server]
    public bool ServerClearAction()
    {
        bool hadAction = HasAction;

        //Debug.Log($"Clearing Action for Unit, {name}");

        Path.Clear();
        HasAction = false;
        if (connectionToClient != null) TargetClearPath();

        return hadAction;
    }

    [Server] // HACK: not sure if this is correct
    public bool ServerSetAction(UnitData data)
    {
        if (!CanMove || data.pathCells.Count < 2) return false;

        Path.Cells = UnitPathfinding.GetValidCells(MyUnit, data.pathCells);

        HasAction = true;

        return true;
    }

    #endregion
    /************************************************************/
    #region Client Functions

    [TargetRpc]
    private void TargetClearPath()
    {
        if (!isClientOnly) return;

        HasAction = false;
        Path.Clear();
    }

    [TargetRpc]
    public void TargetCompleteAction(NetworkConnection conn)
    {
        if (!isClientOnly) return;

        CurrentMovement--; // TODO: This isn't needed, this can be a sync var

        if (!Path.HasPath) return;

        Path.RemoveTailCells(numberToRemove: 1);
        Path.Show();
    }

    [ClientRpc]
    public void RpcCancelAction()
    {
        if (!isClientOnly) return;

        CanMove = false; // This isn't needed, this can be a sync var
    }

    #endregion
    /************************************************************/
    #region Class Functions

    public virtual bool IsValidEdgeForPath(HexCell current, HexCell neighbor)
    {
        // invalid if there is a river inbetween
        //if (current.GetEdgeType(neighbor) == river) return false;

        // invalid if edge between cells is a cliff
        if (current.GetEdgeType(neighbor) == HexEdgeType.Cliff) return false;

        // neighbor is a valid cell
        return true;
    }

    public virtual bool IsValidCellForPath(HexCell current, HexCell neighbor)
    {
        // if a Unit exists on this cell
        //if (neighbor.MyUnit && neighbor.MyUnit.Team == Team) return false; // TODO: check unit type

        // invalid if cell is unexplored
        if (!neighbor.Explorable) return false;

        //isValid &= !cell.IsUnderwater; // cell is not underwater

        // neighbor is a valid cell
        return true;
    }

    public void RefreshPath()
    {
        // updates path HACK: kinda sloppy
        Path.Show();
    }

    #endregion
    /************************************************************/
    #region Event Handler Functions

    private void Subscribe()
    {
        GameManager.ServerOnStartRound += HandleServerOnStartRound;
        GameManager.ServerOnStopTurn += HandleServerOnStopTurn;
        UnitDeath.ServerOnUnitDeath += HandleServerOnUnitDeath;
    }

    private void Unsubscribe()
    {
        GameManager.ServerOnStartRound -= HandleServerOnStartRound;
        GameManager.ServerOnStopTurn -= HandleServerOnStopTurn;
        UnitDeath.ServerOnUnitDeath -= HandleServerOnUnitDeath;
    }

    [Server]
    private void HandleServerOnStartRound()
    {
        CanMove = true;
        HandleRpcOnStartRound();
    }

    [ClientRpc]
    private void HandleRpcOnStartRound()
    {
        if (!isClientOnly) return;

        CanMove = true;
    }

    [Server]
    protected virtual void HandleServerOnStopTurn()
    {
        MyUnit.CombatHandler.HasCaptured = false;
        HasAction = false;
        HandleRpcOnStopTurn();
    }

    [ClientRpc]
    protected virtual void HandleRpcOnStopTurn()
    {
        if (!isClientOnly) return;
        MyUnit.CombatHandler.HasCaptured = false;
        HasAction = false;
    }

    [Server]
    private void HandleServerOnUnitDeath(Unit unit)
    {
        if (unit != MyUnit) return;

        StopAllCoroutines();

        MyCell.MyUnit = null;

        IsEnRoute = false;

        CanMove = false;

        if (EnRouteCell) UnitPathfinding.DecreaseVisibility(EnRouteCell, VisionRange);
        else UnitPathfinding.DecreaseVisibility(MyCell, VisionRange);

        HandleRpcOnDeath();
    }

    [ClientRpc]
    private void HandleRpcOnDeath()
    {
        if (GeneralUtilities.IsRunningOnHost()) return;

        CanMove = false;

        UnitPathfinding.DecreaseVisibility(MyCell, VisionRange);
    }

    [Client]
    private void HookOnMyCell(HexCell oldValue, HexCell newValue)
    {
        if (GeneralUtilities.IsRunningOnHost()) return;

        if (myCell) MyCell = myCell;

        if (oldValue) UnitPathfinding.DecreaseVisibility(oldValue, VisionRange);
        UnitPathfinding.IncreaseVisibility(newValue, VisionRange);
    }

    #endregion
    /************************************************************/
    #region Debug

    ///// <summary>
    ///// Unity Method; Gizmos are drawn only when the object is selected; Gizmos are not pickable;
    ///// This is used to ease setup
    ///// </summary>
    //void OnDrawGizmos()
    //{
    //    if (pathToTravel == null || pathToTravel.Count == 0)
    //    {
    //        return;
    //    }

    //    Vector3 a, b, c = pathToTravel[0].Position;

    //    for (int i = 1; i < pathToTravel.Count; i++)
    //    {
    //        a = c;
    //        b = pathToTravel[i - 1].Position;
    //        c = (b + pathToTravel[i].Position) * 0.5f;
    //        for (float t = 0f; t < 1f; t += Time.deltaTime * travelSpeed)
    //        {
    //            Gizmos.DrawSphere(Bezier.GetPoint(a, b, c, t), 2f);
    //        }
    //    }

    //    a = c;
    //    b = pathToTravel[pathToTravel.Count - 1].Position;
    //    c = b;
    //    for (float t = 0f; t < 1f; t += 0.1f)
    //    {
    //        Gizmos.DrawSphere(Bezier.GetPoint(a, b, c, t), 2f);
    //    }
    //}

    #endregion
}
