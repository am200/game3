using UnityEngine;
using System.Collections;

/**
 * This script should be attached to all controllable units in the game, whether they can move or not
 */
public class Unit : MonoBehaviour
{
	// for Mouse.cs
	public Vector3 screenPos;
	public bool onScreen;
	public bool selected = false;
	public bool isWalkable = true;

	private GameObject dragSelect;

	void Awake(){
		Physics.IgnoreLayerCollision (8, 8, true);
		
		if (transform.FindChild ("Selected") != null) {
			transform.FindChild ("Selected").gameObject.SetActive(false);
		}
		if (transform.FindChild ("DragSelect") != null) {
			dragSelect = transform.FindChild ("DragSelect").gameObject;
		}
	}

	void Update(){
		// if unit is not selected, get screen space
		if (!selected) {

			if(dragSelect){
				// track the screen position
				screenPos = Camera.main.WorldToScreenPoint(dragSelect.transform.position);

			}else{
				// track the screen position
				screenPos = Camera.main.WorldToScreenPoint(this.transform.position);
			}
		
			//if within the screen space
			if(Mouse.UnitWithinScreenSpace(screenPos)){
				//and not already added to UnitsOnScreen, add it
				if(!onScreen){
					Mouse.unitsOnScreen.Add (this.gameObject);
					onScreen = true;
				}
			}else{
				// remove if previously on the screen
				if(onScreen){
					Mouse.RemoveFromOnScreenUnit(this.gameObject);
				}
			}

		}
	}

}

