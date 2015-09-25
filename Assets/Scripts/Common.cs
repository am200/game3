using UnityEngine;
using System.Collections;

public class Common{
	// check if the float is greater than zero
	public static bool FloatGreaterThanZero (float val)
	{
		return val > 0;
	}
	
	
	// are the shift keys being hold down?
	public static bool ShiftKeysDown ()
	{
		if (Input.GetKey (KeyCode.LeftShift) || Input.GetKey (KeyCode.RightShift)) {
			return true;
		} else {
			return false;
		}
	}
}
