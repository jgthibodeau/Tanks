using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

[RequireComponent(typeof (NetworkTransform))]
public class Missile : NetworkBehaviour {
	public const int damage = 10;
	private NetworkTransform networkTransform;

//	void Start() {
//		networkTransform = GetComponent<NetworkTransform> ();
//		StartCoroutine (ModelFollowServerPos());
//	}
//
//	IEnumerator ModelFollowServerPos() {
//		//used to store difference between client model and server position
//		Vector3 delta;
//		float dist;
//		while (!isServer) {
//			dist = Vector3.Distance(networkTransform.targetSyncPosition, transform.position);
////			if (dist > snapThreshold) SnapModel();
//			transform.position += networkTransform.targetSyncVelocity * Time.deltaTime;
//			delta = networkTransform.targetSyncVelocity - transform.position;
////			transform.position += delta * Time.deltaTime * correctionInfluence;
//			yield return 0;
//		}
//	}

	void OnCollisionEnter(Collision collision) {
		Destroy (gameObject);

		GameObject hit = collision.gameObject;
		Health health = hit.GetComponent<Health> ();

		if (health != null) {
			health.TakeDamage (damage);
		}
	}
}
