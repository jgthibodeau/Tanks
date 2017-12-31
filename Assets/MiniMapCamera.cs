using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof (Camera))]
public class MiniMapCamera : MonoBehaviour {
	public RawImage mapImage;
	private Camera mapCamera;

	// Use this for initialization
	void Start () {
		RenderTexture texture = new RenderTexture(256, 256, 16, RenderTextureFormat.ARGB32);
		mapCamera = GetComponent<Camera> ();
		mapCamera.targetTexture = texture;
		mapImage.texture = texture;
	}
}
