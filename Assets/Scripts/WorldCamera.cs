using UnityEngine;
using System.Collections;

/* 
Class to control the camera within the game world.
Camera will move up, down, left and right when the users mouse hits the side of the screen in 2D space.
Camera will check desired location, will stop if over limits.
Camera can also be controlled by W,A,S,D keys, will call the same movement as the mouse events.
*/


public class WorldCamera : MonoBehaviour
{
	
	#region structs
	
	//box limits Struct
	public struct BoxLimit
	{
		public float LeftLimit;
		public float RightLimit;
		public float TopLimit;
		public float BottomLimit;
		
	}
	
	#endregion
	
	
	#region class variables
	
	public static BoxLimit cameraLimits = new BoxLimit ();
	public static BoxLimit mouseScrollLimits = new BoxLimit ();
	public static WorldCamera Instance;
	public GameObject mainCamera;
	private GameObject scrollAngle;
	private float cameraMoveSpeed = 60f;
	private float shiftBonus = 45f;
	private float mouseBoundary = 10f;
	public Terrain WorldTerrain;
	public float WorldTerrainPadding = 25f;
	private float mouseX;
	private float mouseY;
	private bool verticalRotationEnabled = true;
	private float verticalRotationMin = 0f; // in decrees
	private float verticalRotationMax = 65f; // indegrees
	
	[HideInInspector]
	public float
		cameraHeight; // only for scrolling, or zooming.
	[HideInInspector]
	public float
		cameraY; // this will change relative to terrain
	private float maxCameraHeight = 85f;
	public LayerMask terrainOnly;
	private float minimumDistanceToObject = 40f;

	public GameObject dragMeshCamera;

	#endregion
	
	
	void Awake ()
	{
		Instance = this;

	}
	
	void Start ()
	{
		
		//Declare camera limits
		cameraLimits.LeftLimit = WorldTerrain.transform.position.x + WorldTerrainPadding;
		cameraLimits.RightLimit = WorldTerrain.terrainData.size.x - WorldTerrainPadding;
		cameraLimits.TopLimit = WorldTerrain.terrainData.size.z - WorldTerrainPadding;
		cameraLimits.BottomLimit = WorldTerrain.transform.position.z + WorldTerrainPadding;


		// Declare Mouse scroll limits
		mouseScrollLimits.LeftLimit = mouseBoundary;
		mouseScrollLimits.RightLimit = mouseBoundary;
		mouseScrollLimits.TopLimit = mouseBoundary;
		mouseScrollLimits.BottomLimit = mouseBoundary;
	
		cameraHeight = transform.position.y;
		scrollAngle = new GameObject ();

		dragMeshCamera.transform.position = Camera.main.transform.position;
		dragMeshCamera.transform.eulerAngles = new Vector3 (Camera.main.transform.eulerAngles.x, transform.eulerAngles.y, dragMeshCamera.transform.eulerAngles.z);
	}

	void LateUpdate ()
	{

		handleMouseRotation ();

		ApplyScroll ();

		if (CheckIfUserCameraInput ()) {

			Vector3 desiredTranslation = GetDesiredTranslation ();
			
			if (!isDesiredPositionOverBoundaries (desiredTranslation)) {

				Vector3 desiredPosition = transform.position + desiredTranslation;

				UpdateCameraY (desiredPosition);

				this.transform.Translate (desiredTranslation);

			}
		}
		ApplyCameraY ();
		dragMeshCamera.transform.eulerAngles = new Vector3 (Camera.main.transform.eulerAngles.x, transform.eulerAngles.y, dragMeshCamera.transform.eulerAngles.z);

		
	}

	// calculate the minimum camera height
	public float MinCameraHeight ()
	{
		RaycastHit hit;

		float minCameraHeight = WorldTerrain.transform.position.y;
		if (Physics.Raycast (transform.position, Vector3.down, out hit, Mathf.Infinity, terrainOnly)) {
			minCameraHeight = hit.point.y + minimumDistanceToObject;
		}
		return minCameraHeight;

	}

	public void ApplyScroll ()
	{
		float deadZone = 0.01f;
		float easeFactor = 150f;

		if (Application.isWebPlayer) {
			easeFactor = 20f;
		}

		float scrollWheelValue = Input.GetAxis ("Mouse ScrollWheel") * easeFactor * Time.deltaTime;

		// check deadzone 
		if (scrollWheelValue > -deadZone && scrollWheelValue < deadZone) {
			return;
		}

		float eulerAngleX = mainCamera.transform.localEulerAngles.x;

		// Configure the scroll angle gameObject
		scrollAngle.transform.position = transform.position;
		scrollAngle.transform.eulerAngles = new Vector3 (eulerAngleX, this.transform.eulerAngles.y, this.transform.eulerAngles.z);
		scrollAngle.transform.Translate (Vector3.back * scrollWheelValue);
	
		Vector3 desiredScrollPosition = scrollAngle.transform.position;
		
		if (desiredScrollPosition.x < cameraLimits.LeftLimit || desiredScrollPosition.x > cameraLimits.RightLimit) {
			return;
		}
		if (desiredScrollPosition.z > cameraLimits.TopLimit || desiredScrollPosition.z < cameraLimits.BottomLimit) {
			return;
		}
		if (desiredScrollPosition.y > maxCameraHeight || desiredScrollPosition.y < MinCameraHeight ()) {
			return;
		}
		
		//update the cameraHeight and the cameraY

		float heightDifference = desiredScrollPosition.y - this.transform.position.y;
		cameraHeight += heightDifference;
		UpdateCameraY (desiredScrollPosition);

		// update the camera position
		this.transform.position = desiredScrollPosition;

	}

	public void UpdateCameraY (Vector3 desiredPosition)
	{
		RaycastHit hit;
		float deadZone = 0.1f;

		if (Physics.Raycast (desiredPosition, Vector3.down, out hit, Mathf.Infinity)) {
			
			float newHeight = cameraHeight + hit.point.y;

			float heightDifference = newHeight - cameraY;
			
			if (heightDifference > -deadZone && heightDifference < deadZone) {
				return;
			}
			
			if (newHeight > maxCameraHeight || newHeight < MinCameraHeight ()) {
				return;
			}
			
			cameraY = newHeight;
		}
		Debug.DrawRay (this.transform.position, Vector3.down * 1000, Color.cyan);

	}


	//apply the camera y to a smooth down,  and update camera y position
	public void ApplyCameraY ()
	{
		if (cameraY == transform.position.y || cameraY == 0) {
			return;
		}

		// smooth damp

		float smoothTime = 0.2f;
		float yVelocity = 0.0f;

		float newPositionY = Mathf.SmoothDamp (transform.position.y, cameraY, ref yVelocity, smoothTime);

		if (newPositionY < maxCameraHeight && newPositionY > 0) {
			transform.position = new Vector3 (transform.position.x, newPositionY, transform.position.z);
		}

		return;
		

	}


	// handles the mouse rotation vertically and horizontally
	public void handleMouseRotation ()
	{
		var easeFactor = 10f;

		if (Input.GetMouseButton (1) && Input.GetKey (KeyCode.LeftControl)) {
			// horizontal rotation
			if (Input.mousePosition.x != mouseX) {
				var cameraRotationY = (Input.mousePosition.x - mouseX) * easeFactor * Time.deltaTime;
				this.transform.Rotate (0, cameraRotationY, 0);
			}
			//vertical rotation
			if (verticalRotationEnabled && Input.mousePosition.y != mouseY) {
				var cameraRotationX = (mouseY - Input.mousePosition.y) * easeFactor * Time.deltaTime;
				var desiredRotationX = mainCamera.transform.eulerAngles.x + cameraRotationX;

				if (desiredRotationX >= verticalRotationMin && desiredRotationX <= verticalRotationMax) {
					mainCamera.transform.Rotate (cameraRotationX, 0, 0);
				}
			}
		}
		
		mouseX = Input.mousePosition.x;
		mouseY = Input.mousePosition.y;
	}

	//Check if the user is inputting commands for the camera to move
	public bool CheckIfUserCameraInput ()
	{
	
		bool keyboardMove;
		bool mouseMove;
		bool canMove;

		// check keyboard
		if (WorldCamera.AreCameraKeyboardButtonsPressed ()) {
			keyboardMove = true;
		} else {
			keyboardMove = false;
		}

		// check mouse position
		if (WorldCamera.IsMousePositionWithinBoundaries ()) {
			mouseMove = true;
		} else {
			mouseMove = false;
		}

		if (keyboardMove || mouseMove) {
			canMove = true;
		} else {
			canMove = false;
		}

		return canMove;
	}


	// Works out the cameras desired location depending on the players input
	public Vector3 GetDesiredTranslation ()
	{
		float moveSpeed = 0f;

		Vector3 desiredTranslation = Vector3.zero;

		if (Input.GetKey (KeyCode.LeftShift) || Input.GetKey (KeyCode.RightShift)) {
			moveSpeed = (cameraMoveSpeed + shiftBonus) * Time.deltaTime;
		} else {
			moveSpeed = cameraMoveSpeed * Time.deltaTime;
		}

		// move via keyboard
		if (Input.GetKey (KeyCode.W) || Input.mousePosition.y > (Screen.height - mouseScrollLimits.TopLimit)) {
			desiredTranslation += Vector3.forward * moveSpeed;
		}

		if (Input.GetKey (KeyCode.S) || Input.mousePosition.y < mouseScrollLimits.BottomLimit) {
			desiredTranslation += Vector3.back * moveSpeed;
		}

		if (Input.GetKey (KeyCode.A) || Input.mousePosition.x < mouseScrollLimits.LeftLimit) {
			desiredTranslation += Vector3.left * moveSpeed;
		}
		if (Input.GetKey (KeyCode.D) || Input.mousePosition.x > (Screen.width - mouseScrollLimits.RightLimit)) {
			desiredTranslation += Vector3.right * moveSpeed;
		}

		return desiredTranslation;
	}

	// checks if the desired position crosses boundaries
	public bool isDesiredPositionOverBoundaries (Vector3 desiredPosition)
	{

		Vector3 desiredWorldPosition = this.transform.TransformPoint (desiredPosition);

		bool overBoundaries = false;


		// check boundaries
		if ((this.transform.position.x + desiredPosition.x) < cameraLimits.LeftLimit) {
			overBoundaries = true;
		}
		if ((this.transform.position.x + desiredPosition.x) > cameraLimits.RightLimit) {
			overBoundaries = true;
		}
		if ((this.transform.position.z + desiredPosition.z) > cameraLimits.TopLimit) {
			overBoundaries = true;
		}
		if ((this.transform.position.z + desiredPosition.z) < cameraLimits.BottomLimit) {
			overBoundaries = true;
		}

		return overBoundaries;
	}

	#region helper functions

	public static bool AreCameraKeyboardButtonsPressed ()
	{
		if (Input.GetKeyDown (KeyCode.W) || Input.GetKeyDown (KeyCode.A) || Input.GetKeyDown (KeyCode.S) || Input.GetKeyDown (KeyCode.D)) {
			return true;
		} else {
			return false;
		}
		
	}

	public static bool IsMousePositionWithinBoundaries ()
	{
		if (
			(Input.mousePosition.x < mouseScrollLimits.LeftLimit && Input.mousePosition.x > -5) ||
			(Input.mousePosition.x > (Screen.width - mouseScrollLimits.RightLimit) && Input.mousePosition.x < (Screen.width + 5)) ||
			(Input.mousePosition.y < mouseScrollLimits.BottomLimit && Input.mousePosition.y > -5) ||
			(Input.mousePosition.y > (Screen.height - mouseScrollLimits.TopLimit) && Input.mousePosition.y < (Screen.height + 5))) {
			return true;
		} else {
			return false;
		}
	}

	#endregion
}
