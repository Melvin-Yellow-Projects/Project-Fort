using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DebugHand : MonoBehaviour
{
    /************************************************************/
    #region Variables

    [Tooltip("layers to ignore when raycasting")]
    [SerializeField] LayerMask layersToIgnore; 

    #endregion
    /************************************************************/
    #region Unity Functions

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0)) TryToPickupPiece();
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
    }

    #endregion
}
