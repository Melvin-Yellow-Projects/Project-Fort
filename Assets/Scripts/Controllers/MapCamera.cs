/**
 * File Name: HexMapCamera.cs
 * Description: Camera controller for the Hex Map
 * 
 * Authors: Catlike Coding, Will Lacey
 * Date Created: September 10, 2020
 * 
 * Additional Comments: 
 *      The original version of this file can be found here:
 *      https://catlikecoding.com/unity/tutorials/hex-map/ within Catlike Coding's tutorial series:
 *      Hex Map; this file has been updated it to better fit this project
 *
 *      TODO: Comment this script
 **/

using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
///     Operable hex map camera class
/// </summary>
public class MapCamera : MonoBehaviour
{
	/********** MARK: Variables **********/
	#region Variables

	Transform swivel, stick;

	Vector2 moveDelta;
	//float rotateDelta;
	//float zoomDelta;

	float zoom = 1f;

	public float stickMinZoom, stickMaxZoom;

	public float swivelMinZoom, swivelMaxZoom;

	public float moveSpeedMinZoom, moveSpeedMaxZoom;

	public float rotationSpeed;

	float rotationAngle;

	Controls controls;

	#endregion

	/********** MARK: Properties **********/
	#region Properties

	public static MapCamera Singleton { get; set; }

	public static bool Locked
	{
		set
		{
            if (Singleton) Singleton.enabled = !value;
        }
	}

	#endregion

	/********** MARK: Unity Functions **********/
	#region Unity Functions

	void Awake()
	{
		swivel = transform.GetChild(0);
		stick = swivel.GetChild(0);

		controls = new Controls();

		controls.Camera.Move.performed += AdjustPosition;
		controls.Camera.Move.canceled += AdjustPosition;

		controls.Camera.Rotate.performed += AdjustRotation;
		controls.Camera.Rotate.canceled += AdjustRotation;

		controls.Camera.Zoom.performed += AdjustZoom;
		controls.Camera.Zoom.canceled += AdjustZoom;

		controls.Enable();
	}

	/// <summary>
	/// Unity Method; This function is called when the object becomes enabled and active
	/// </summary>
	protected void OnEnable()
	{
		Singleton = this;
	}

	void Update()
	{
		float zoomDelta = Input.GetAxis("Mouse ScrollWheel");
		if (zoomDelta != 0f) AdjustZoom(zoomDelta);

		float rotationDelta = Input.GetAxis("Rotation");
		if (rotationDelta != 0f) AdjustRotation(rotationDelta);

		float xDelta = Input.GetAxis("Horizontal");
        float zDelta = Input.GetAxis("Vertical");
        if (xDelta != 0f || zDelta != 0f) AdjustPosition(xDelta, zDelta);
		//if (moveDelta.x != 0f || moveDelta.y != 0f) AdjustPosition(moveDelta.x, moveDelta.y);
	}

	#endregion

	/********** MARK: Class Functions **********/
	#region Class Functions

	public static void ValidatePosition()
	{
		Singleton.AdjustPosition(0f, 0f);
	}

	void AdjustPosition(InputAction.CallbackContext ctx)
    {
		moveDelta = ctx.ReadValue<Vector2>();
		//AdjustPosition(moveDelta.x, moveDelta.y);
	}

	void AdjustPosition(float xDelta, float zDelta)
	{
		// movement is relative to the camera's point of view
		Vector3 direction = transform.localRotation * new Vector3(xDelta, 0f, zDelta).normalized;

		float damping = Mathf.Max(Mathf.Abs(xDelta), Mathf.Abs(zDelta));
		float distance = Mathf.Lerp(moveSpeedMinZoom, moveSpeedMaxZoom, zoom) *
			damping * Time.deltaTime;

		Vector3 position = transform.localPosition;
		position += direction * distance;
		transform.localPosition = position;

		transform.localPosition = ClampPosition(position);
	}

	Vector3 ClampPosition(Vector3 position)
	{
        float xMax = (HexGrid.Singleton.cellCountX * HexMetrics.chunkSizeX - 0.5f) *
            (2f * HexMetrics.innerRadius);
        position.x = Mathf.Clamp(position.x, 0f, xMax);

        float zMax = (HexGrid.Singleton.cellCountZ * HexMetrics.chunkSizeZ - 1) *
            (1.5f * HexMetrics.outerRadius);
        position.z = Mathf.Clamp(position.z, 0f, zMax);

        return position;
	}

	void AdjustRotation(InputAction.CallbackContext ctx)
	{
		Debug.Log("Rotating Camera");
	}

	void AdjustRotation(float delta)
	{
		rotationAngle += delta * rotationSpeed * Time.deltaTime;
		if (rotationAngle < 0f)
		{
			rotationAngle += 360f;
		}
		else if (rotationAngle >= 360f)
		{
			rotationAngle -= 360f;
		}
		transform.localRotation = Quaternion.Euler(0f, rotationAngle, 0f);
	}

	void AdjustZoom(InputAction.CallbackContext ctx)
	{
		Debug.Log("Zooming Camera");
	}

	void AdjustZoom(float delta)
	{
		zoom = Mathf.Clamp01(zoom + delta);

		float distance = Mathf.Lerp(stickMinZoom, stickMaxZoom, zoom);
		stick.localPosition = new Vector3(0f, 0f, distance);

		float angle = Mathf.Lerp(swivelMinZoom, swivelMaxZoom, zoom);
		swivel.localRotation = Quaternion.Euler(angle, 0f, 0f);
	}

    #endregion
}