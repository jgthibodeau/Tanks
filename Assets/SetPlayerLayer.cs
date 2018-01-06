using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class SetPlayerLayer : NetworkBehaviour {

	// Use this for initialization
	void Start () {
		if (isLocalPlayer) {
			foreach (Transform t in gameObject.GetComponentsInChildren<Transform> ()) {
				if (t.gameObject.layer != LayerMask.NameToLayer ("DrawablePlayer")) {
					t.gameObject.layer = LayerMask.NameToLayer ("NonDrawablePlayer");
				}
			}
		}
	}
}
