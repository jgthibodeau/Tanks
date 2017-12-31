using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using DaburuTools;

public class AttachCamera : MonoBehaviour {
	public GameObject turretModel;
	public Vector3 turretRotationOffset;

	// Use this for initialization
	void Start () {
		NetworkIdentity parentIdentity = transform.parent.GetComponentInParent<NetworkIdentity> ();
		if (parentIdentity.isLocalPlayer) {
			GameObject camera = GameObject.FindGameObjectWithTag ("MainCamera").gameObject;
			camera.transform.parent = transform;
			camera.transform.localPosition = Vector3.zero;
			camera.transform.localEulerAngles = new Vector3 (0, 90, 90);

			camera.GetComponent<DaburuTools.Input.GyroControl> ().isLocalPlayer = true;
			camera.GetComponent<DaburuTools.Input.GyroControl> ().Init ();

			RotateWith rotateWith = turretModel.AddComponent<RotateWith> ();
			rotateWith.other = camera.transform;
			rotateWith.offset = turretRotationOffset;
		}
	}
}
