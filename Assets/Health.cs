using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;

public class Health : NetworkBehaviour {
	[SyncVar(hook = "OnChangeHealth")]
	public int currentHealth = maxHealth;
	public const int maxHealth = 100;
	public Text healthText;
	public bool respawn = false;
	public bool destroyOnDeath = true;

	private NetworkStartPosition[] spawnPoints;
	private Vector3 originalSpawn;

	void Start() {
		if (isLocalPlayer) {
			OnChangeHealth (currentHealth);

			spawnPoints = FindObjectsOfType<NetworkStartPosition> ();

			originalSpawn = transform.position;
		}
	}

	public void TakeDamage(int amount) {
		if (!isServer) {
			return;
		}

		currentHealth -= amount;

		if (currentHealth <= 0) {
			Debug.Log ("Killed " + gameObject);
			Debug.Log ("Respawning");

			if (respawn) {
				currentHealth = maxHealth;
				RpcRespawn ();
			} else if (destroyOnDeath) {
				GameObject.Destroy (gameObject);
			}
		}
	}

	void OnChangeHealth(int currentHealth) {
		healthText.text = "Health: " + currentHealth;
	}

	[ClientRpc]
	void RpcRespawn() {
		if (isLocalPlayer) {
			Vector3 spawnPoint = Vector3.zero;
			if (originalSpawn != null) {
				spawnPoint = originalSpawn;
			} else if (spawnPoints != null && spawnPoints.Length > 0) {
				spawnPoint = spawnPoints [Random.Range (0, spawnPoints.Length)].transform.position;
			}
			transform.position = spawnPoint;
		}
	}
}
