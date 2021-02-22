/**
 * File Name: PiecePath.cs
 * Description: TODO: write this
 * 
 * Authors: Will Lacey
 * Date Created: October 18, 2020
 * 
 * Additional Comments:
 *      TODO: Update Path to better track the pathing of the piece, in particular the tail of the
 *              path does a poor job in showing where the piece is moving during its move
 *      
 *      Previously known as UnitPath.cs & HexPath.cs
 **/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 
/// </summary>
public class PiecePath : MonoBehaviour
{
    /************************************************************/
    #region Private Variables

    Piece piece;
    PieceMovement movement;

    //List<HexCell> cells = ListPool<HexCell>.Get();
    List<HexCell> cells = new List<HexCell>();

    PieceCursor cursor;

    #endregion
    /************************************************************/
    #region Public Properties

    public int Length
    {
        get
        {
            return cells.Count;
        }
    }

    public bool HasPath
    {
        get
        {
            return (cells.Count > 1);
        }
    }

    public List<HexCell> Cells // HACK: this could be simplified with the variable cells
    {
        get
        {
            return cells;
        }
        set
        {
            // HACK: does this work?
            cells = value;
        }
    }

    public HexCell StartCell
    {
        get
        {
            return piece.MyCell;
            //return cells[0]; // this is the same thing
        }
    }

    public HexCell EndCell
    {
        get
        {
            return cells[cells.Count - 1];
        }
    }

    public HexCell PenultimateCell
    {
        get
        {
            if (Length > 1) return cells[cells.Count - 2];
            return null;
        }
    }

    public int MovementCost
    {
        get
        {
            return PiecePathfinding.GetMoveCostCalculation(cells);
        }
    }

    public HexCell this[int i]
    {
        get
        {
            return cells[i];
        }
        set
        {
            cells[i] = value;
        }
    }

    #endregion
    /************************************************************/
    #region Unity Functions

    private void Awake()
    {
        piece = GetComponent<Piece>();
        movement = GetComponent<PieceMovement>();
    }

    #endregion
    /************************************************************/
    #region Class Functions

    /// <summary>
    /// 
    /// </summary>
    /// <param name="cell"></param>
    /// <param name="canBackTrack"></param>
    public void AddCellToPath(HexCell cell, bool canBackTrack)
    {
        /* pseudocode */
        // initialize if new path
        // if the path doesn't have the cell or if you can back track
            // if we can add the cell
                // add it
                // if the path is now too long, remove the last cell
            // (we can't add cell) check a* route for potential new path

        // else (we have the cell and we can't backtrack)
            // if this is the penultimate cell, remove it (makes it easy for user to play w/ paths)
            // else A* to create new path

        if (cells.Count == 0) cells.Add(piece.MyCell);

        if (!cells.Contains(cell) || canBackTrack)
        {
            if (PiecePathfinding.CanAddCellToPath(piece, cell))
            {
                cells.Add(cell);

                if (MovementCost <= movement.CurrentMovement) return; // exit function

                cells.Remove(EndCell);
            }

            CheckForBetterPath(cell);
        }
        else
        {
            if (PenultimateCell == cell) cells.Remove(EndCell);
            else cells = PiecePathfinding.FindPath(piece, StartCell, cell);
        }
    }

    private void CheckForBetterPath(HexCell cell)
    {
        List<HexCell> potentialPath = PiecePathfinding.FindPath(piece, StartCell, cell);

        if (PiecePathfinding.GetMoveCostCalculation(potentialPath) <= movement.CurrentMovement)
            cells = potentialPath;
    }

    public void RemoveTailCells(int numberToRemove)
    {
        if (numberToRemove > cells.Count) Debug.LogError("Removing more cells than in Path!");

        for (int i = 0; i < numberToRemove; i++)
        {
            //cells[0].DisableHighlight();
            cells.RemoveAt(0);
        }

        if (piece.isServer && piece.MyCell != cells[0])
            Debug.LogWarning("Tail cell is not piece's cell!");
    }

    /// <summary>
    /// TODO: comment ShowPath
    /// HACK: show path and clear path can be compressed into one function
    /// </summary>
    /// <param name="speed"></param>
    public void Show()
    {
        //StopAllCoroutines();

        if (!HasPath)
        {
            if (cursor != null) cursor.DestroyCursor();
            movement.Display.RefreshMovementDisplay(movement.CurrentMovement);
            return;
        }

        List<Vector3> points = new List<Vector3>();

        for (int i = 0; i < cells.Count; i++)
        {
            //int turn = (cells[i].Distance - 1) / piece.Speed;
            //cells[i].SetLabel(turn.ToString(), FontStyle.Bold, fontSize: 8);
            //cells[i].EnableHighlight(Color.white);

            points.Add(cells[i].Position);
        }
        //StartCell.EnableHighlight(Color.blue);
        //endCell.EnableHighlight(Color.red);

        if (cursor) cursor.Redraw(points);
        else cursor = PieceCursor.Initialize(points);

        //cursor.IsSelected = piece.IsSelected;
        //cursor.HasError =
        //    (PiecePathfinding.GetMoveCostCalculation(cells) > movement.CurrentMovement);

        //StartCoroutine(AnimatePath());

        movement.Display.RefreshMovementDisplay(movement.CurrentMovement - cells.Count + 1);
    }

    public void Hide()
    {
        if (cursor != null) cursor.DestroyCursor();
    }

    private IEnumerator AnimatePath()
    {
        Image highlight;
        ColorSetter setter;
        int i = 0;
        while (HasPath && piece.IsSelected)
        {
            highlight = cells[i].uiRectTransform.GetChild(0).GetComponent<Image>();
            setter = cells[i].uiRectTransform.GetChild(0).GetComponent<ColorSetter>();

            yield return setter.SetColor(highlight, Color.red, cutoff:0.3f);
            Debug.Log("Helo");
            StartCoroutine(setter.SetColor(highlight, Color.white));

            i++;
            if (i >= cells.Count) i = 0;
        }
    }

    ///// <summary>
    ///// TODO: comment ClearPath
    ///// </summary>
    //public void Hide()
    //{
    //    // TODO: this is just wrong boy
    //    for (int i = 0; i < cells.Count; i++)
    //    {
    //        //cells[i].SetLabel(null);
    //        cells[i].DisableHighlight();
    //    }
    //}

    public void Clear(bool clearCursor = true)
    {
        //Hide(); // TODO: i think there needs to be a hide function for the cursor
        if (clearCursor && cursor != null) cursor.DestroyCursor();

        movement.Display.RefreshMovementDisplay(movement.CurrentMovement);

        //for (int i = 0; i < cells.Count; i++)
        //{
        //    cells[i].DisableHighlight();
        //}

        cells.Clear();
    }

    #endregion
    /************************************************************/
    #region Debug

    public void LogPath()
    {
        string str = "Path: ";

        for (int i = 0; i < cells.Count - 1; i++)
        {
            str += cells[i].Index + " -> ";
        }

        str += cells[cells.Count - 1].Index;

        Debug.LogWarning(str);
    }

    #endregion
}
