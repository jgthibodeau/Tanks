using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class EnableUI : NetworkBehaviour {
	public GameObject UI;

	// Use this for initialization
	void Start () {
		if (isLocalPlayer) {
			UI.SetActive (true);
		} else {
			UI.SetActive (false);
		}
	}
}
