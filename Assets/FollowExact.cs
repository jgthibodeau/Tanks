using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowExact : MonoBehaviour {
	public Vector3 rotation;
	public Vector3 position;
	public Transform target;
	
	// Update is called once per frame
	void Update () {
		transform.position = target.position + position;
		transform.rotation = Quaternion.Euler (rotation);
	}
}
