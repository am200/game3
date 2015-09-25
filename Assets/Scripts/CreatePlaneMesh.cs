using UnityEngine;
using System.Collections;

public class CreatePlaneMesh : MonoBehaviour {

	public float width = 50f;
	public float height = 50f;

	// Use this for initialization
	void Start () {
		MeshFilter mf = GetComponent<MeshFilter> ();
		Mesh mesh = new Mesh ();
		mf.mesh = mesh;

		// Vertices
		Vector3[] vertices = new Vector3[]{
			new Vector3 (0, 0, 0),
			new Vector3 (width, 0, 0),
			new Vector3 (0, height, 0),
			new Vector3 (width, height, 0)
		};

		// triangles
		int[] triangles =new int[6];
		triangles[0] = 0;
		triangles[1] = 2;
		triangles[2] = 1;
		
		triangles[3] = 2;
		triangles[4] = 3;
		triangles[5] = 1;

		// Normals (only if you want to display objects in the game);
	
		Vector3[] normals = new Vector3[4];

		normals[0] = -Vector3.forward;
		normals[1] = -Vector3.forward;
		normals[2] = -Vector3.forward;
		normals[3] = -Vector3.forward;

		// UVs (how textures are displayed)

		Vector2[] uv = new Vector2[4];
		uv[0] = new Vector2(0,0);
		uv[1] = new Vector2(1,0);
		uv[2] = new Vector2(0,1);
		uv[3] = new Vector2(1,1);


		// assigne arrays!
		mesh.vertices = vertices;
		mesh.triangles = triangles;
		mesh.normals = normals;
		mesh.uv = uv;

	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
