using UnityEngine;
using System.Collections;

public class DestroyParentGameObject : MonoBehaviour
{

	public void DestroyObject ()
	{
		Destroy (this.gameObject.transform.parent.gameObject);

	}
}

