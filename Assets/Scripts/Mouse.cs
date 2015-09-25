using UnityEngine;
using System.Collections;

public class Mouse: MonoBehaviour
{

	#region structs

	public struct ClipPlanePoints
	{
		public Vector3 upperLeft;
		public Vector3 upperRight;
		public Vector3 lowerLeft;
		public Vector3 lowerRight;
	}

	#endregion


	RaycastHit hit;
	public static ArrayList currentlySelectedUnits = new ArrayList (); // of GameObjects
	public static ArrayList unitsOnScreen = new ArrayList (); // of GameObjects
	public static ArrayList unitsInDrag = new ArrayList (); // of GameObjects
	private bool finishedDragOnThisFrame;
	public GUIStyle mouseDragSkin;
	private static Vector3 mouseDownPoint;
	private static Vector3 currentMousePoint; // in world space

	private bool startedDrag = true;
	public GameObject target;
	private static bool userIsDragging = false;
	private static float TimeLimitBeforeDeclareDrag = 1f;
	private static float TimeLeftBeforeDeclareDrag;
	private static Vector2 MouseDragStart;
	private static float clickDragZone = 1.3f;
	public static Vector3 rightClickPoint;
	public LayerMask selectMeshLayerMask;

	// GUI

	private float boxWidth;
	private float boxHeight;
	private float boxTop;
	private float boxLeft;
	private static Vector2 boxStart;
	private static Vector2 boxFinish;
	public GameObject dragSelectMesh;
	public float distanceToGround;
	public LayerMask terrainOnly;
	public GameObject pointer;

	void Awake ()
	{
	}

	void Start ()
	{
		pointer = new GameObject ();

			CreateDragBoxMesh ();
	}
	
	// Update is called once per frame
	void Update ()
	{

		ClipPlanePoints nearPlanePoints = CameraClipPlanePoints (Camera.main.nearClipPlane);

		Ray ray = Camera.main.ScreenPointToRay (Input.mousePosition);

		if (Physics.Raycast (ray, out hit, Mathf.Infinity)) {

			currentMousePoint = hit.point;

			if (Input.GetMouseButtonDown (0)) {
				mouseDownPoint = hit.point;
				TimeLeftBeforeDeclareDrag = TimeLimitBeforeDeclareDrag;
				MouseDragStart = Input.mousePosition;
				startedDrag = true;
			

			} else if (Input.GetMouseButton (0)) {
				// if the user is not dragging, lets do the tests

				if (!userIsDragging) {
					TimeLeftBeforeDeclareDrag = Time.deltaTime;
					if (TimeLeftBeforeDeclareDrag <= 0f || UserDraggingByPosition (MouseDragStart, Input.mousePosition)) {
						userIsDragging = true;
					}
				}
			} else if (Input.GetMouseButtonUp (0)) {

				if (userIsDragging) {
					finishedDragOnThisFrame = true;
				}

				userIsDragging = false;
			}


			// Mouse Click
			if (!userIsDragging) {

				if (hit.collider.name == "TerrainMain") {

					if (Input.GetMouseButtonDown (1) && !Input.GetKey (KeyCode.LeftControl)) {

						GameObject targetObj = Instantiate (target, hit.point, Quaternion.identity) as GameObject;
						targetObj.name = "TargetInstatnicated";
						rightClickPoint = hit.point;
					} else if (Input.GetMouseButtonUp (0) && DidUserClickLeftMouse (mouseDownPoint)) {

						if (!Common.ShiftKeysDown ()) {
							DeselectGameObjectsIfSelected ();
						}
					}
					 
					// end of the terrain
				} else {


					// hitting other objects
					if (Input.GetMouseButtonUp (0) && DidUserClickLeftMouse (mouseDownPoint)) {


						// is the user hitting a unit?
						if (hit.collider.gameObject.GetComponent<Unit> () || hit.collider.gameObject.layer == LayerMask.NameToLayer ("SelectMesh")) {

							Transform unitGameObject;
					
							if (hit.collider.gameObject.layer == LayerMask.NameToLayer ("SelectMesh")) {
								unitGameObject = hit.collider.transform.parent.transform;
							} else {
								unitGameObject = hit.collider.transform;
							}

							// are we a selecting a diffrernt object?
							if (!UnitAlreadyInCurrentylSelectedUnits (unitGameObject.gameObject)) {

								// if the shift key is down, ad it tot the arry list
								if (!Common.ShiftKeysDown ()) {
									DeselectGameObjectsIfSelected ();
								}

								// activate the selector
								GameObject selectedObj = unitGameObject.FindChild ("Selected").gameObject;
							
								selectedObj.SetActive (true);

								// add the unit to the array list
								currentlySelectedUnits.Add (unitGameObject.gameObject);

								unitGameObject.gameObject.GetComponent<Unit> ().selected = true;

							} else {

								
								if (Common.ShiftKeysDown ()) {
									// remove the unit if in array list						
									RemoveUnitFromCurrentlySelectedUnits (unitGameObject.gameObject);
								} else {
									DeselectGameObjectsIfSelected ();
									GameObject selectedObj = unitGameObject.transform.FindChild ("Selected").gameObject;
									selectedObj.SetActive (true);
									currentlySelectedUnits.Add (unitGameObject.gameObject);
								}
							}

						} else {
							

							// if this object is not a unit
							if (!Common.ShiftKeysDown ()) {
								DeselectGameObjectsIfSelected ();
							}
						}

					}
				}
			}
		// end of the terrain
		else {
				// hitting other objects
				if (Input.GetMouseButtonUp (0) && DidUserClickLeftMouse (mouseDownPoint)) {
			
					if (!Common.ShiftKeysDown ()) {
						DeselectGameObjectsIfSelected ();
					}
				}
			} // end of ray cast
		}// end of is dragging

		if (!Common.ShiftKeysDown () && startedDrag && userIsDragging) {
			DeselectGameObjectsIfSelected ();
			startedDrag = false;
		}
		
		Debug.DrawRay (ray.origin, ray.direction * 1000, Color.blue);
	
		if (userIsDragging) {
			boxWidth = Camera.main.WorldToScreenPoint (mouseDownPoint).x - Camera.main.WorldToScreenPoint (currentMousePoint).x;
			boxHeight = Camera.main.WorldToScreenPoint (mouseDownPoint).y - Camera.main.WorldToScreenPoint (currentMousePoint).y;
			// box width, height, top, left
			boxLeft = Input.mousePosition.x;
			boxTop = (Screen.height - Input.mousePosition.y) - boxHeight;

			if (Common.FloatGreaterThanZero (boxWidth)) {
				if (Common.FloatGreaterThanZero (boxHeight)) {
					boxStart = new Vector2 (Input.mousePosition.x, Input.mousePosition.y + boxHeight);	
				} else {
					boxStart = new Vector2 (Input.mousePosition.x, Input.mousePosition.y);
				}
			} else if (!Common.FloatGreaterThanZero (boxWidth)) {
				if (Common.FloatGreaterThanZero (boxHeight)) {
					boxStart = new Vector2 (Input.mousePosition.x + boxWidth, Input.mousePosition.y + boxHeight);	
				} else {
					boxStart = new Vector2 (Input.mousePosition.x + boxWidth, Input.mousePosition.y);
				}
			}
			boxFinish = new Vector2 (
				boxStart.x + Mathf.Abs (boxWidth),
				boxStart.y - Mathf.Abs (boxHeight)
			);
		}	
	}

	void LateUpdate ()
	{
		unitsInDrag.Clear ();
		if(userIsDragging){
			UpdateDragBoxMesh();
		}

		// if user is dragging, or finished on this frame, AND there are units to select on the screen
		if ((userIsDragging || finishedDragOnThisFrame) && unitsOnScreen.Count > 0) {

			//loop through those units on screen
			foreach (GameObject unitObj in unitsOnScreen) {
				Unit unitScript = unitObj.GetComponent<Unit> ();
				GameObject selectedObj = unitObj.transform.FindChild ("Selected").gameObject;

				//if not already in dragged units
				if (!UnitAlreadyInDraggedUnits (unitObj)) {

					if (UnitInsideDrag (unitScript.screenPos)) {
						selectedObj.SetActive (true);
						unitsInDrag.Add (unitObj);
					} else {
						if (!UnitAlreadyInCurrentylSelectedUnits (unitObj)) {
							selectedObj.SetActive (false);
						}
					}
				}
			}


		}

		if (finishedDragOnThisFrame) {
			finishedDragOnThisFrame = false;
			PutDraggedUnitsInCurrentlySelectedUnits ();
		}

	}
	// update the mesh, based on our 2D drag!
	public void UpdateDragBoxMesh(){
		// p0 ration (Top Right)
		Vector2 p0Ratio = new Vector2 (
			(boxFinish.x / (Screen.width * 0.01f) * 0.01f),
			((boxFinish.y + Mathf.Abs(boxHeight))/(Screen.height * 0.01f) * 0.01f)
			);

		// p1 ration (Top Left)
		Vector2 p1Ratio = new Vector2 (
			(boxStart.x / (Screen.width * 0.01f) * 0.01f),
			(boxStart.y /(Screen.height * 0.01f) * 0.01f)
			);

		// p2 ration (Lower Left)
		Vector2 p2Ratio = new Vector2 (
			(boxStart.x / (Screen.width * 0.01f) * 0.01f),
			((boxStart.y -Mathf.Abs (boxHeight)) /(Screen.height * 0.01f) * 0.01f)
			);

		
		// p2 ration (Lower Right)
		Vector2 p3Ratio = new Vector2 (
			(boxFinish.x / (Screen.width * 0.01f) * 0.01f),
			(boxFinish.y / (Screen.height * 0.01f) * 0.01f)
			);

		ClipPlanePoints nearClipPlanePoints = CameraClipPlanePoints (Camera.main.GetComponent<Camera>().nearClipPlane);
		ClipPlanePoints farClipPlanePoints = CameraClipPlanePoints (DistanceFromCameraToGround ());

		float nearPlaneWidth = Vector3.Distance (nearClipPlanePoints.lowerLeft, nearClipPlanePoints.lowerRight);
		float nearPlaneHeight = Vector3.Distance (nearClipPlanePoints.upperRight, nearClipPlanePoints.lowerRight);
		
		float farPlaneWidth = Vector3.Distance (farClipPlanePoints.lowerLeft, farClipPlanePoints.lowerRight);
		float farPlaneHeight = Vector3.Distance (farClipPlanePoints.upperRight, farClipPlanePoints.lowerRight);

		pointer.transform.position = nearClipPlanePoints.lowerLeft;
		pointer.transform.eulerAngles = Camera.main.transform.eulerAngles;
		pointer.transform.Translate (nearPlaneWidth * p0Ratio.x, nearPlaneHeight * p0Ratio.y, 0f);

		Vector3 p0 = pointer.transform.position;

		pointer.transform.position = nearClipPlanePoints.lowerLeft;
		pointer.transform.Translate (nearPlaneWidth * p1Ratio.x, nearPlaneHeight * p1Ratio.y, 0f);

		Vector3 p1 = pointer.transform.position;
		
		pointer.transform.position = nearClipPlanePoints.lowerLeft;
		pointer.transform.Translate (nearPlaneWidth * p2Ratio.x, nearPlaneHeight * p2Ratio.y, 0f);

		Vector3 p2 = pointer.transform.position;
		
		pointer.transform.position = nearClipPlanePoints.lowerLeft;
		pointer.transform.Translate (nearPlaneWidth * p3Ratio.x, nearPlaneHeight * p3Ratio.y, 0f);

		Vector3 p3 = pointer.transform.position;

		pointer.transform.position = farClipPlanePoints.lowerLeft;
		pointer.transform.Translate (farPlaneWidth * p0Ratio.x, farPlaneHeight * p0Ratio.y, 0f);

		Vector3 p4 = pointer.transform.position;
		
		pointer.transform.position = farClipPlanePoints.lowerLeft;
		pointer.transform.Translate (farPlaneWidth * p1Ratio.x, farPlaneHeight * p1Ratio.y, 0f);
		
		Vector3 p5 = pointer.transform.position;
		
		pointer.transform.position = farClipPlanePoints.lowerLeft;
		pointer.transform.Translate (farPlaneWidth * p2Ratio.x, farPlaneHeight * p2Ratio.y, 0f);
		
		Vector3 p6 = pointer.transform.position;
		
		pointer.transform.position = farClipPlanePoints.lowerLeft;
		pointer.transform.Translate (farPlaneWidth * p3Ratio.x, farPlaneHeight * p3Ratio.y, 0f);
		
		Vector3 p7 = pointer.transform.position;

		Mesh mesh = dragSelectMesh.GetComponent<MeshFilter> ().mesh;

		Vector3[] vertices = new Vector3[]
		{
			// Bottom
			p0, p1, p2, p3,
			
			// Left
			p7, p4, p0, p3,
			
			// Front
			p4, p5, p1, p0,
			
			// Back
			p6, p7, p3, p2,
			
			// Right
			p5, p6, p2, p1,
			
			// Top
			p7, p6, p5, p4
		};

		mesh.vertices = vertices;
	
	}

	void OnGUI ()
	{
		if (userIsDragging) {
				
			GUI.Box (new Rect (boxLeft, boxTop, boxWidth, boxHeight), "--", mouseDragSkin);

		}

	}


	#region Helper Functions
	
	// is the user dragging, relative to the mouse drag start point.

	public bool UserDraggingByPosition (Vector2 dragStartPoint, Vector2 newPoint)
	{
		if ((newPoint.x > dragStartPoint.x + clickDragZone || newPoint.x < dragStartPoint.x - clickDragZone) ||
			(newPoint.y > dragStartPoint.y + clickDragZone || newPoint.y < dragStartPoint.y - clickDragZone)
		   ) {
			return true;
		} else {
			return false;
		}
	}


	// did user perform a mouse click ?
	public bool DidUserClickLeftMouse (Vector3 hitPoint)
	{

		if (
			(mouseDownPoint.x < hitPoint.x + clickDragZone && mouseDownPoint.x > hitPoint.x - clickDragZone) &&
			(mouseDownPoint.y < hitPoint.y + clickDragZone && mouseDownPoint.y > hitPoint.y - clickDragZone) &&
			(mouseDownPoint.z < hitPoint.z + clickDragZone && mouseDownPoint.z > hitPoint.z - clickDragZone) 
		) {
			return true;
		} else {
			return false;
		}
	}

	// Deselects game object if selected
	public static void DeselectGameObjectsIfSelected ()
	{
		if (currentlySelectedUnits.Count > 0) {
			foreach (GameObject gameObj in currentlySelectedUnits) {
				gameObj.transform.FindChild ("Selected").gameObject.SetActive (false);
				gameObj.GetComponent<Unit> ().selected = false;
			}
			currentlySelectedUnits.Clear ();
		}
	}

	// if a unit is already in the currently selected units array list

	public static bool UnitAlreadyInCurrentylSelectedUnits (GameObject unit)
	{
		if (currentlySelectedUnits.Count > 0) {
			foreach (GameObject gameObj in currentlySelectedUnits) {
				if (gameObj == unit) {
					return true;
				}

			}
		}
		return false;
	}

	// remove a unit from the currently selecte unit arraylist

	public void RemoveUnitFromCurrentlySelectedUnits (GameObject unit)
	{
		if (currentlySelectedUnits.Count > 0) {
			foreach (GameObject gameObj in currentlySelectedUnits) {
				if (gameObj == unit) {
					gameObj.transform.FindChild ("Selected").gameObject.SetActive (false);
					currentlySelectedUnits.Remove (gameObj);
				}
			}
		}
		return;
	}


	// check if a unit is within the screen space to deal with mouse drag selecting
	public static bool UnitWithinScreenSpace (Vector2 unitScreenPosition)
	{
		if ((unitScreenPosition.x < Screen.width && unitScreenPosition.y < Screen.height) &&
			(unitScreenPosition.x > 0f && unitScreenPosition.y > 0f)) {
			return true;
		} else {
			return false;
		}
	}

	// Remove a unit from screen units (UnitsONScreen) arraylist
	public static void RemoveFromOnScreenUnit (GameObject unit)
	{
		foreach (GameObject unitObj in unitsOnScreen) {
			if (unit == unitObj) {
				unitsOnScreen.Remove (unitObj);
				unitObj.GetComponent<Unit> ().onScreen = false;
				return;
			}
		}
		return;
	}

	// is unit inside the drag?
	public static bool UnitInsideDrag (Vector2 unitScreenPos)
	{
		if (unitScreenPos.x > boxStart.x &&
			unitScreenPos.y < boxStart.y &&
			unitScreenPos.x < boxFinish.x &&
			unitScreenPos.y > boxFinish.y) {
			return true;
		} else {
			return false;
		}

	}

	// check if unit is unitsInDrag array list
	public static bool UnitAlreadyInDraggedUnits (GameObject unit)
	{
		if (unitsInDrag.Count > 0) {
			foreach (GameObject gameObj in unitsInDrag) {
				if (gameObj == unit) {
					return true;
				}
			}
		}
		return false;
	}

	// take all units from UnitsDrag, into currentlySelectedUnits
	public static void PutDraggedUnitsInCurrentlySelectedUnits ()
	{
		if (unitsInDrag.Count > 0) {
			foreach (GameObject gameObj in unitsInDrag) {
				if (!UnitAlreadyInCurrentylSelectedUnits (gameObj)) {
					currentlySelectedUnits.Add (gameObj);
					gameObj.GetComponent<Unit> ().selected = true;
				}
			}
			unitsInDrag.Clear ();
		}

	}



	#endregion

	#region drag box mesh


	private float DistanceFromCameraToGround ()
	{
		float extend = 50f;

		RaycastHit dist;
		float newFarPlaneValue = Camera.main.farClipPlane;

		if (Physics.Raycast (Camera.main.transform.position, Camera.main.transform.forward, out dist, 1000f, terrainOnly)) {
			distanceToGround = Vector3.Distance (Camera.main.transform.position, dist.point);
			Debug.DrawRay (Camera.main.transform.position, Camera.main.transform.forward * 1000f, Color.green);
			newFarPlaneValue = distanceToGround - Camera.main.nearClipPlane + extend;		
			Camera.main.farClipPlane = newFarPlaneValue;
		}
		return newFarPlaneValue;
	}

	// works out plane points at any distance inside a camera
	public ClipPlanePoints CameraClipPlanePoints (float distance)
	{
		ClipPlanePoints clipPlanePoints = new ClipPlanePoints ();

		Transform transform = Camera.main.transform;

		Vector3 position = transform.position;

		float halfFieldOfView = (Camera.main.fieldOfView * 0.5f) * Mathf.Deg2Rad;
		float aspect = Camera.main.aspect;

		float height = Mathf.Tan (halfFieldOfView) * distance;
		float width = height * aspect;


		// Lower Right
		clipPlanePoints.lowerRight = position + transform.forward * distance;
		clipPlanePoints.lowerRight += transform.right * width;
		clipPlanePoints.lowerRight -= transform.up * height;

		// Lower Left
		clipPlanePoints.lowerLeft = position + transform.forward * distance;
		clipPlanePoints.lowerLeft -= transform.right * width;
		clipPlanePoints.lowerLeft -= transform.up * height;

		// Upper Right
		clipPlanePoints.upperRight = position + transform.forward * distance;
		clipPlanePoints.upperRight += transform.right * width;
		clipPlanePoints.upperRight += transform.up * height;
		
		// Upper Left
		clipPlanePoints.upperLeft = position + transform.forward * distance;
		clipPlanePoints.upperLeft -= transform.right * width;
		clipPlanePoints.upperLeft += transform.up * height;

		return clipPlanePoints;
	}

	private void CreateDragBoxMesh ()
	{
		// You can change that line to provide another MeshFilter
		MeshFilter filter = dragSelectMesh.GetComponent<MeshFilter> ();
		Mesh mesh = filter.mesh;
		mesh.Clear ();
		
		float length = 50f;
		float width = 50f;
		float height = 50f;
		
		#region Vertices
		Vector3 p0 = new Vector3 (-length * .5f, -width * .5f, height * .5f);
		Vector3 p1 = new Vector3 (length * .5f, -width * .5f, height * .5f);
		Vector3 p2 = new Vector3 (length * .5f, -width * .5f, -height * .5f);
		Vector3 p3 = new Vector3 (-length * .5f, -width * .5f, -height * .5f);	
		
		Vector3 p4 = new Vector3 (-length * .5f, width * .5f, height * .5f);
		Vector3 p5 = new Vector3 (length * .5f, width * .5f, height * .5f);
		Vector3 p6 = new Vector3 (length * .5f, width * .5f, -height * .5f);
		Vector3 p7 = new Vector3 (-length * .5f, width * .5f, -height * .5f);
		
		Vector3[] vertices = new Vector3[]
		{
		// Bottom
			p0, p1, p2, p3,
			
		// Left
			p7, p4, p0, p3,
			
		// Front
			p4, p5, p1, p0,
			
		// Back
			p6, p7, p3, p2,
			
		// Right
			p5, p6, p2, p1,
			
		// Top
			p7, p6, p5, p4
		};
		#endregion
		
		#region Normales
		Vector3 up = Vector3.up;
		Vector3 down = Vector3.down;
		Vector3 front = Vector3.forward;
		Vector3 back = Vector3.back;
		Vector3 left = Vector3.left;
		Vector3 right = Vector3.right;
		
		Vector3[] normales = new Vector3[]
		{
		// Bottom
			down, down, down, down,
			
		// Left
			left, left, left, left,
			
		// Front
			front, front, front, front,
			
		// Back
			back, back, back, back,
			
		// Right
			right, right, right, right,
			
		// Top
			up, up, up, up
		};
		#endregion	
		
		#region UVs
		Vector2 _00 = new Vector2 (0f, 0f);
		Vector2 _10 = new Vector2 (1f, 0f);
		Vector2 _01 = new Vector2 (0f, 1f);
		Vector2 _11 = new Vector2 (1f, 1f);
		
		Vector2[] uvs = new Vector2[]
		{
		// Bottom
			_11, _01, _00, _10,
			
		// Left
			_11, _01, _00, _10,
			
		// Front
			_11, _01, _00, _10,
			
		// Back
			_11, _01, _00, _10,
			
		// Right
			_11, _01, _00, _10,
			
		// Top
			_11, _01, _00, _10,
		};
		#endregion
		
		#region Triangles
		int[] triangles = new int[]
		{
		// Bottom
			3, 1, 0,
			3, 2, 1,			
			
		// Left
			3 + 4 * 1, 1 + 4 * 1, 0 + 4 * 1,
			3 + 4 * 1, 2 + 4 * 1, 1 + 4 * 1,
			
		// Front
			3 + 4 * 2, 1 + 4 * 2, 0 + 4 * 2,
			3 + 4 * 2, 2 + 4 * 2, 1 + 4 * 2,
			
		// Back
			3 + 4 * 3, 1 + 4 * 3, 0 + 4 * 3,
			3 + 4 * 3, 2 + 4 * 3, 1 + 4 * 3,
			
		// Right
			3 + 4 * 4, 1 + 4 * 4, 0 + 4 * 4,
			3 + 4 * 4, 2 + 4 * 4, 1 + 4 * 4,
			
		// Top
			3 + 4 * 5, 1 + 4 * 5, 0 + 4 * 5,
			3 + 4 * 5, 2 + 4 * 5, 1 + 4 * 5,
			
		};
		#endregion
		
		mesh.vertices = vertices;
		mesh.normals = normales;
		mesh.uv = uvs;
		mesh.triangles = triangles;
		
		mesh.RecalculateBounds ();
		mesh.Optimize ();
	}

	#endregion
}
