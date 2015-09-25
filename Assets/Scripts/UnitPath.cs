using UnityEngine;
using System.Collections;
using Pathfinding;

public class UnitPath : MonoBehaviour
{

	private Seeker seeker;
	private CharacterController controller;
	public Path path;
	public float speed;
	// The max distance from the AI to the waypoint for it to continue to the next waypoint
	// current waypoint (always start at index 0)
	private int currentWaypoint = 0;
	public float defaultNextWayPointDistance = 20f;
	private Unit unit;

	public void Start ()
	{	
		seeker = GetComponent<Seeker> ();
		controller = GetComponent<CharacterController> ();
		unit = GetComponent<Unit> ();
	}

	public void LateUpdate ()
	{
		if (unit.selected && unit.isWalkable) {
			if (Input.GetMouseButtonDown (1)) {
				seeker.StartPath (transform.position, Mouse.rightClickPoint, OnPathComplete);
			}
		}
	}


	// path finding logic
	public void OnPathComplete (Path p)
	{
		if (!p.error) {
			path = p;
			//Reset waypoint counter 
			currentWaypoint = 0;
		}
	}
	
	public void FixedUpdate ()
	{
		if (!unit.isWalkable) {
			return;
		}

		// return if no path exists
		if (path == null) {
			return;
		}
		// return if waypoint counter above or equal to path waypoint size
		if (currentWaypoint >= path.vectorPath.Count) {
			return;
		}
		
		// Calculate directon of unit
		Vector3 dir = (path.vectorPath [currentWaypoint] - transform.position).normalized;
		dir *= speed * Time.fixedDeltaTime;
		controller.SimpleMove (dir); // unit moves here!

		float nextWayPointDistance = defaultNextWayPointDistance;

		if (currentWaypoint == path.vectorPath.Count - 1) {
			nextWayPointDistance = 0.1f;
		} else {
			
			transform.LookAt (new Vector3 (path.vectorPath [currentWaypoint].x, transform.position.y, path.vectorPath [currentWaypoint].z));
		}

		// check if close enough to the current waypoint, if we are, proceed to next waypoint
		
		if (Vector3.Distance (transform.position, path.vectorPath [currentWaypoint]) < nextWayPointDistance) {
			currentWaypoint++;
			return;
		}
		
		
	}
}
