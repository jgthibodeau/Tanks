using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class LANHost : MonoBehaviour {
	public NetworkManager networkManager;

	public void StartHost() {
		networkManager.StartHost ();
	}
}
