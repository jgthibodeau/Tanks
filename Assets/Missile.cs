using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

[RequireComponent(typeof (NetworkTransform))]
[RequireComponent(typeof (Rigidbody))]
public class Missile : NetworkBehaviour {
	public int damage = 10;

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
		if (Time.time - startTime < accelerationTime) {
			rigidBody.AddForce (transform.forward * acceleration);
		}
	}

	void OnCollisionEnter(Collision collision) {
		Destroy (gameObject);
		GameObject.Instantiate (explosion, transform.position, transform.rotation);

		GameObject hit = collision.gameObject;
		Health health = hit.GetComponent<Health> ();

		if (health != null) {
			health.TakeDamage (damage);
		}
	}
}
