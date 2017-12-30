using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class PinchZoom : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IDragHandler {

	public Camera mapCamera;
	public float minZoom = 10;
	public float maxZoom = 50;
	public float zoomSpeed = 0.5f;

	private int pointerCount = 0;

	public void OnPointerDown(PointerEventData data) {
		pointerCount++;
	}

	public void OnPointerUp(PointerEventData data) {
		pointerCount--;
	}
	
	// Update is called once per frame
//	void Update () {
	public void OnDrag(PointerEventData data) {
		if (pointerCount == 2 && Input.touchCount == 2) {
			Touch touchZero = Input.GetTouch (0);
			Touch touchOne = Input.GetTouch (1);

			Vector2 touchZeroPrevPos = touchZero.position - touchZero.deltaPosition;
			Vector2 touchOnePrevPos = touchOne.position - touchOne.deltaPosition;

			float prevTouchDeltaMag = (touchZeroPrevPos - touchOnePrevPos).magnitude;
			float touchDeltaMag = (touchZero.position - touchOne.position).magnitude;

			float deltaMagnitudeDiff = prevTouchDeltaMag - touchDeltaMag;

			mapCamera.orthographicSize += deltaMagnitudeDiff * zoomSpeed;
			mapCamera.orthographicSize = Mathf.Clamp (mapCamera.orthographicSize, minZoom, maxZoom);
		}
	}
}
