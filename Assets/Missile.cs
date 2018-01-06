using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

[RequireComponent(typeof (NetworkTransform))]
[RequireComponent(typeof (Rigidbody))]
public class Missile : NetworkBehaviour {
	public int damage = 10;

	public bool accelerate = false;
	public int acceleration = 10;
	public int accelerationTime = 10;
	private float startTime;

	public GameObject explosion;

	private NetworkTransform networkTransform;
	private Rigidbody rigidBody;

	void Start() {
		startTime = Time.time;
		rigidBody = GetComponent<Rigidbody> ();
	}

	void Update() {
		if (accelerate) {
			if (Time.time - startTime < accelerationTime) {
				rigidBody.AddForce (transform.forward * acceleration);
			}
		}
	}

	void OnCollisionEnter(Collision collision) {
		GameObject hit = collision.gameObject;
		Health health = hit.GetComponent<Health> ();

		if (health != null) {
			health.TakeDamage (damage);
		}

		Debug.Log ("calling explosion");
		CmdExplode ();
	}

	[Command]
	private void CmdExplode() {
		Debug.Log ("creating explosion");
		GameObject explosionInst = GameObject.Instantiate (explosion, transform.position, transform.rotation);
		NetworkServer.Spawn (explosionInst);
		Debug.Log ("spawned explosion");
		Destroy (gameObject);
	}
}
