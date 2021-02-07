using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DebugHand : MonoBehaviour
{
    /************************************************************/
    #region Variables

    [Tooltip("layers to ignore when raycasting")]
    [SerializeField] LayerMask layersToIgnore;

    Unit grabbedUnit = null;

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

        if (!Physics.Raycast(ray, out hit, 1000, ~layersToIgnore)) return;

        Debug.DrawLine(ray.origin, hit.point, Color.red, 3f);

        Debug.Log(hit.rigidbody.transform.parent.name);

        grabbedUnit = hit.rigidbody.GetComponentInParent<Unit>();
    }

    private void LetGoOfPiece()
    {
        //grabbedUnit.transform.position = grabbedUnit.MyCell.Position;
        grabbedUnit.ValidateLocation();
        grabbedUnit = null;
    }

    private void UpdateGrabbedPiecePosition()
    {
        grabbedUnit.transform.position = PlayerMenu.MyPlayer.currentCell.Position;

        //grabbedUnit.transform.position = new Vector3(
        //    PlayerMenu.MyPlayer.currentCell.Position.x,
        //    PlayerMenu.MyPlayer.currentCell.Position.y,
        //    PlayerMenu.MyPlayer.currentCell.Position.z
        //);
    }

    #endregion
}
