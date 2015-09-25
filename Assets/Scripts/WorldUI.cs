using UnityEngine;
using System.Collections;

public class WorldUI : MonoBehaviour {

	[HideInInspector] public GameObject UICamera;

	private Resolution screenResolution;
	public LayerMask uiCameraLayerMask;
	//public SpriteManager spriteManager;
	public static WorldUI instance;

	void Start(){
		instance = this;

		// resolution
		Resolution currentResolution = Screen.currentResolution;
		if (Application.isEditor) {
			screenResolution = Screen.resolutions [0];
		} else {
			screenResolution = currentResolution;
		}
		//set the resolution
		Screen.SetResolution (screenResolution.width, screenResolution.height, true);

		// UI Camera setup

		UICamera = new GameObject ("UICamera");
		UICamera.AddComponent<Camera>();

		Camera uiCamera = UICamera.GetComponent<Camera>();

		uiCamera.cullingMask = uiCameraLayerMask;
		uiCamera.name = "UICamera";
		uiCamera.orthographicSize = screenResolution.height / 2;
		uiCamera.orthographic = true;
		uiCamera.nearClipPlane = 0.3f;
		uiCamera.farClipPlane = 50f;
		uiCamera.clearFlags = CameraClearFlags.Depth;
		uiCamera.depth = 1;
		uiCamera.rect = new Rect (0, 0, 1, 1);
		uiCamera.renderingPath = RenderingPath.UsePlayerSettings;
		uiCamera.targetTexture = null;
		uiCamera.hdr = false;
	}


}
