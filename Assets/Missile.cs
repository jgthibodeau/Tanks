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
	public bool createExplosionParticles = true;
	public float explosionRadius = 1f;
	public float explosionForceScale = 1f;
	public ForceMode explosionForceMode = ForceMode.Impulse;

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
		if (createExplosionParticles) {
			Debug.Log ("creating explosion");
			GameObject explosionInst = GameObject.Instantiate (explosion, transform.position, transform.rotation);
			NetworkServer.Spawn (explosionInst);
			Debug.Log ("spawned explosion");
		}
		Destroy (gameObject);

		//find all rigidbodies in radius, apply force based on distance

		//find all colliders in radius
		Debug.Log ("finding rigidbodies");
		Collider[] colliders = Physics.OverlapSphere (transform.position, explosionRadius);
		HashSet<Rigidbody> rigidbodies = new HashSet<Rigidbody>();
		foreach (Collider collider in colliders) {
			DestroyableTree tree = collider.gameObject.GetComponent<DestroyableTree> ();
			if (tree != null) {
				tree.Delete ();
			} else {
				//for each unique rigidbody
				Rigidbody rigidBody = collider.GetComponentInParent<Rigidbody> ();
				if (rigidBody != null && !rigidbodies.Contains (rigidBody)) {
					rigidbodies.Add (rigidBody);
					//add force based on distance and away from explosion point
					Vector3 direction = rigidBody.position - transform.position;
					float explosionPercent = 1 - (Vector3.Magnitude (direction) / explosionRadius);
					if (explosionPercent >= 0) {
						Vector3 explosionForce = direction.normalized * explosionPercent * explosionForceScale;
						Debug.Log ("adding force to rigidbody " + rigidBody + " " + explosionForce);
						rigidBody.AddForce (explosionForce, explosionForceMode);
					}
				}
			}
		}
	}
}
