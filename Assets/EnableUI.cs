using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class EnableUI : NetworkBehaviour {
	public GameObject UI;

	// Use this for initialization
	void Start () {
		UI.SetActive (isLocalPlayer);
	}
}
