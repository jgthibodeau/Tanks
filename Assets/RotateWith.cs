using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotateWith : MonoBehaviour {
	public Transform other;
	public Vector3 offset;
	
	// Update is called once per frame
	void Update () {
		this.transform.rotation = other.rotation * Quaternion.Euler (offset);
	}
}
