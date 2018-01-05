using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraShake : MonoBehaviour {
	Transform camera;
	public float shake = 0f;
	public float shakeAmount = 0.4f;
	public float shakeRotateAmount = 1f;
	public float decreaseFactor = 1f;

	// Use this for initialization
	void Start () {
		camera = Camera.main.transform;
	}
	
	void Update() {
		if (shake > 0) {
			camera.localPosition = Random.insideUnitSphere * shakeAmount;
			camera.localRotation = Quaternion.Euler (Random.insideUnitSphere * shakeRotateAmount) * camera.localRotation;
			shake -= Time.deltaTime * decreaseFactor;

		} else {
			shake = 0f;
			camera.localPosition = Vector3.zero;
		}
	}

	public void Shake(float amount) {
		if (shake > 0) {
			camera.localPosition = Random.insideUnitSphere * shakeAmount;
			shake -= Time.deltaTime * decreaseFactor;

		} else {
			shake = 0f;
			camera.localPosition = Vector3.zero;
		}
	}
}
