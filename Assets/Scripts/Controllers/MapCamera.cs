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
 *      HACK: this class is just a complete jank mess
 *				this class uses a singleton which won't work if there are ever multiple cameras
 **/

using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;
using Cinemachine;

/// <summary>
///     Operable hex map camera class
/// </summary>
public class MapCamera : MonoBehaviour
{
	/************************************************************/
	#region Variables

	[Header("Cached References")]
	[SerializeField] Transform swivel = null;
	[SerializeField] Transform stick = null;
	[SerializeField] CinemachineVirtualCamera virtualCamera = null;
	//[SerializeField] CinemachineVirtualCamera nextVirtualCamera = null;

	[Header("Settings")]
	[SerializeField] float stickMinZoom;
	[SerializeField] float stickMaxZoom;

	[SerializeField] float swivelMinZoom;
	[SerializeField] float swivelMaxZoom;

	[SerializeField] float moveSpeedMinZoom;
	[SerializeField] float moveSpeedMaxZoom;

	[SerializeField] float rotationSpeed;

	Vector2 moveDelta;
	//float rotateDelta;
	//float zoomDelta;

	float zoom = 1f;

	float rotationAngle;

	Controls controls;
	bool isMovePressed = false;
	bool isRotatePressed = false;

	static int rotationIndex;
	static Quaternion currentQuaternion;

	#endregion
	/************************************************************/
	#region Properties

	public static MapCamera Singleton { get; set; }

	public static bool Locked
	{
		set
		{
            if (Singleton) Singleton.enabled = !value;
        }
	}

	static int nextIndex;
	private static int NextIndex
    {
		get
        {
			return nextIndex;
		}
		set
        {
			nextIndex = value;

			// HACK i really dont like fetching the player through PlayerMenu
			if (GameManager.IsEconomyPhase) 
            {
				if (nextIndex >= PlayerDisplay.MyPlayer.MyForts.Count) nextIndex = 0;
			}
			else
            {
				if (nextIndex >= PlayerDisplay.MyPlayer.MyPieces.Count) nextIndex = 0;
			}
		}
    }

	#endregion
	/************************************************************/
	#region Unity Functions

    private void OnEnable()
    {
		Singleton = this;

		virtualCamera.enabled = true;

		controls = new Controls();

		controls.Camera.Move.performed += AdjustPosition;
		controls.Camera.Move.canceled += AdjustPosition;

		controls.Camera.Rotate.performed += AdjustRotation;
		//controls.Camera.Rotate.canceled += AdjustRotation;

		controls.Camera.Zoom.performed += AdjustZoom;
		controls.Camera.Zoom.canceled += AdjustZoom;

		controls.Camera.Next.performed += FocusOnNextEntity;

		controls.Enable();

		Subscribe();
	}

    private void OnDisable()
    {
		virtualCamera.enabled = false;

		controls.Dispose();

		Unsubscribe();
	}

    void Update()
	{
		float zoomDelta = Input.GetAxis("Mouse ScrollWheel");
		if (zoomDelta != 0f) AdjustZoom(zoomDelta);

		//float rotationDelta = Input.GetAxis("Rotation");
		//if (rotationDelta != 0f) AdjustRotation(rotationDelta);

		float xDelta = Input.GetAxis("Horizontal");
        float zDelta = Input.GetAxis("Vertical");
        if (xDelta != 0f || zDelta != 0f) AdjustPosition(xDelta, zDelta);
		//if (moveDelta.x != 0f || moveDelta.y != 0f) AdjustPosition(moveDelta.x, moveDelta.y);
	}

    private void OnDestroy()
    {
		Singleton = null;
	}

    #endregion
    /************************************************************/
    #region Input Functions

    void AdjustRotation(InputAction.CallbackContext ctx)
	{
		rotationIndex += (int) ctx.ReadValue<float>();

		StopAllCoroutines();
		StartCoroutine(AdjustRotation(rotationIndex * 45f));

		if (rotationIndex == 8) rotationIndex = 0;
	}

	private IEnumerator AdjustRotation(float targetAngle)
    {
		currentQuaternion = transform.rotation;
		for (float interpolator = 0; interpolator < 1f; interpolator += Time.deltaTime * rotationSpeed)
        {
			transform.localRotation = Quaternion.Lerp(
				transform.rotation,
				Quaternion.Euler(0f, targetAngle, 0f),
				interpolator
			);
			yield return null;
        }

		transform.localRotation = Quaternion.Euler(0f, targetAngle, 0f);
	}

	private void FocusOnNextEntity(InputAction.CallbackContext ctx)
    {
		// HACK attacc but it a snacc
		Player player = PlayerDisplay.MyPlayer;

		if (!player) return;

		Vector3 pos;
		if (GameManager.IsEconomyPhase) pos = player.MyForts[NextIndex].transform.position;
		else pos = player.MyPieces[NextIndex].transform.position;
		transform.position = new Vector3(pos.x, transform.position.y, pos.z);

		NextIndex++;
	}

	#endregion
	/************************************************************/
	#region Class Functions

	public static void ValidatePosition()
	{
		Singleton.AdjustPosition(0f, 0f);
	}

	void AdjustPosition(InputAction.CallbackContext ctx)
    {
		//moveDelta = ctx.ReadValue<Vector2>();
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
		//Debug.Log($"{HexGrid.Singleton.cellCountX}, {HexMetrics.chunkSizeX}, {2f * HexMetrics.innerRadius}");
		float xMax = (HexGrid.Singleton.cellCountX - 0.5f) * (2f * HexMetrics.Configuration.InnerRadius);
		//float xMax = (HexGrid.Singleton.cellCountX * HexMetrics.chunkSizeX - 0.5f) *
  //          (2f * HexMetrics.innerRadius);
        position.x = Mathf.Clamp(position.x, 0f, xMax);

		float zMax = (HexGrid.Singleton.cellCountZ - 1) * (1.5f * HexMetrics.Configuration.OuterRadius);
		//float zMax = (HexGrid.Singleton.cellCountZ * HexMetrics.chunkSizeZ - 1) *
  //          (1.5f * HexMetrics.outerRadius);
        position.z = Mathf.Clamp(position.z, 0f, zMax);

        return position;
	}

	//void AdjustRotation(float delta)
	//{
	//	rotationAngle += delta * rotationSpeed * Time.deltaTime;
	//	if (rotationAngle < 0f)
	//	{
	//		rotationAngle += 360f;
	//	}
	//	else if (rotationAngle >= 360f)
	//	{
	//		rotationAngle -= 360f;
	//	}
	//	transform.localRotation = Quaternion.Euler(0f, rotationAngle, 0f);
	//}

	void AdjustZoom(InputAction.CallbackContext ctx)
	{
		//Debug.Log("Zooming Camera");
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
	/************************************************************/
	#region Event Handler Functions

	private void Subscribe()
    {
		GameManager.ClientOnStartRound += HandleClientOnStartRound;
		GameManager.ClientOnStartTurn += HandleClientOnStartTurn;
	}

	private void Unsubscribe()
    {
		GameManager.ClientOnStartRound -= HandleClientOnStartRound;
		GameManager.ClientOnStartTurn -= HandleClientOnStartTurn;
	}

	private void HandleClientOnStartRound()
    {
		NextIndex = 0;

		if (GameManager.RoundCount == 1)
        {
			// HACK this is really rushed
			Player player = PlayerDisplay.MyPlayer;
			if (!player) return;

			Vector3 pos = player.MyForts[0].transform.position;
			Singleton.transform.position =
				new Vector3(pos.x, Singleton.transform.position.y, pos.z);
		}
    }

	private void HandleClientOnStartTurn()
    {
		NextIndex = 0;
	}

	#endregion
}