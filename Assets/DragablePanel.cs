using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class DragablePanel : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IDragHandler {

	public float maxHeight = 100;

	public float moveSpeed = 1;
	public float minDistance = 10;

	private Vector2 pointerOffset;
	private RectTransform canvasRectTransform;
	private RectTransform panelRectTransform;
	private int pointerCount = 0;

	void Awake() {
		Canvas canvas = GetComponentInParent<Canvas> ();
		if (canvas != null) {
			canvasRectTransform = canvas.transform as RectTransform;
			panelRectTransform = transform as RectTransform;

			Close ();
		}
	}

	public void OnPointerDown(PointerEventData data) {
		pointerCount++;

		RectTransformUtility.ScreenPointToLocalPointInRectangle (
			panelRectTransform,
			data.position,
			data.pressEventCamera,
			out pointerOffset
		);
	}

	public void OnPointerUp(PointerEventData data) {
		pointerCount--;

		//if data.delta.y is negative, dragging down
		if (data.delta.y < 0) {
			StartCoroutine ("OpenSmooth");
		}
		//if data.delta.y is positive, dragging up
		else if (data.delta.y > 0) {
			StartCoroutine ("CloseSmooth");
		}
		//if data.delta.y is 0, don't know direction so use position
		else {
			if (panelRectTransform.localPosition.y < canvasRectTransform.rect.height / 2) {
				StartCoroutine ("OpenSmooth");
			} else {
				StartCoroutine ("CloseSmooth");
			}
		}
	}

	public void OnDrag(PointerEventData data) {
		if (panelRectTransform == null || pointerCount != 1) {
			return;
		}

		Vector2 localPointerPosition;
		if (RectTransformUtility.ScreenPointToLocalPointInRectangle (
			canvasRectTransform,
			data.position,
			data.pressEventCamera,
			out localPointerPosition)
		) {
			Vector2 newPanelPosition = panelRectTransform.localPosition;
			float maxPanelY = canvasRectTransform.rect.height - maxHeight;
			newPanelPosition.y = Mathf.Clamp((localPointerPosition - pointerOffset).y, 0, maxPanelY);

			panelRectTransform.localPosition = newPanelPosition;
		}
	}

	public void Close() {
		Vector2 newPanelPosition = panelRectTransform.localPosition;
		newPanelPosition.y = canvasRectTransform.rect.height - maxHeight;
		panelRectTransform.localPosition = newPanelPosition;
	}

	public void Open() {
		Vector2 newPanelPosition = panelRectTransform.localPosition;
		newPanelPosition.y = 0;
		panelRectTransform.localPosition = newPanelPosition;
	}

	IEnumerator CloseSmooth() {
		Vector2 desiredPanelPosition = panelRectTransform.localPosition;
		desiredPanelPosition.y = canvasRectTransform.rect.height - maxHeight;

		while (Vector2.Distance (panelRectTransform.localPosition, desiredPanelPosition) > minDistance) {
			Vector2 newPanelPosition = Vector2.Lerp (panelRectTransform.localPosition, desiredPanelPosition, Time.deltaTime * moveSpeed);
			panelRectTransform.localPosition = newPanelPosition;
			yield return null;
		}
		panelRectTransform.localPosition = desiredPanelPosition;
	}

	IEnumerator OpenSmooth() {
		Vector2 desiredPanelPosition = panelRectTransform.localPosition;
		desiredPanelPosition.y = 0;

		while (Vector2.Distance (panelRectTransform.localPosition, desiredPanelPosition) > minDistance) {
			Vector2 newPanelPosition = Vector2.Lerp (panelRectTransform.localPosition, desiredPanelPosition, Time.deltaTime * moveSpeed);
			panelRectTransform.localPosition = newPanelPosition;
			yield return null;
		}
		panelRectTransform.localPosition = desiredPanelPosition;
	}
}
