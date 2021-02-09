/**
 * File Name: DebugHand.cs
 * Description: 
 * 
 * Authors: Will Lacey
 * Date Created: February 5, 2021
 * 
 * Additional Comments: 
 **/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DebugHand : MonoBehaviour
{
    /************************************************************/
    #region Variables

    [Tooltip("layer(s) affected by the hand")] // HACK: is this the name we want
    [SerializeField] LayerMask grabLayers;

    [Tooltip("layer(s) for the HexGrid Map")]
    [SerializeField] LayerMask mapLayers;

    [Tooltip("speed of the piece when it changes position")]
    [SerializeField, Range(0, 10)] float moveSpeed;

    Unit grabbedUnit = null;
    Vector3 currentPosition;

    #endregion
    /************************************************************/
    #region Unity Functions

    // Update is called once per frame
    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            if (grabbedUnit) LetGoOfPiece();
            else TryToPickupPiece(); 
        }

        if (!grabbedUnit) return;
        UpdateGrabbedPiecePosition();
    }

    #endregion

    /************************************************************/
    #region Class Functions

    private void TryToPickupPiece()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (!Physics.Raycast(ray, out hit, 1000, grabLayers)) return;

        Debug.DrawLine(ray.origin, hit.point, Color.red, 3f);

        Debug.Log(hit.rigidbody.transform.parent.name);

        grabbedUnit = hit.rigidbody.GetComponentInParent<Unit>();
    }

    private void LetGoOfPiece()
    {
        StopAllCoroutines();
        //grabbedUnit.transform.position = grabbedUnit.MyCell.Position;
        grabbedUnit.ValidateLocation();
        grabbedUnit = null;
    }

    private void UpdateGrabbedPiecePosition()
    {
        //if (currentPosition == PlayerMenu.MyPlayer.currentCell.Position) return;

        //grabbedUnit.transform.position = PlayerMenu.MyPlayer.currentCell.Position;

        //StopAllCoroutines();
        //StartCoroutine(ChangePiecePosition());

        // instantly sets the position of the piece
        grabbedUnit.transform.position = PlayerMenu.MyPlayer.currentCell.Position;


        // piece follows the raycast hit of the mouse on the HexGrid
        //Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        //if (!Physics.Raycast(ray, out RaycastHit hit, 1000, terrainLayers)) return;

        //HexCell cell = HexGrid.Singleton.GetCell(hit.point);
        //if (!cell || !cell.IsExplored) return;

        //grabbedUnit.transform.position = new Vector3(
        //    hit.point.x,
        //    (PlayerMenu.MyPlayer.currentCell.Position.y + hit.point.y) / 2,
        //    hit.point.z
        //);
    }

    private IEnumerator ChangePiecePosition()
    {
        currentPosition = grabbedUnit.transform.position;
        for (float interpolator = 0; interpolator < 1; interpolator += Time.deltaTime * moveSpeed)
        {
            grabbedUnit.transform.position = Vector3.Lerp(
                currentPosition,
                PlayerMenu.MyPlayer.currentCell.Position,
                interpolator);

            yield return null;
        }

        currentPosition = PlayerMenu.MyPlayer.currentCell.Position;
    }

    #endregion
}
