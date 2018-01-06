using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class LANClient : MonoBehaviour {
	public NetworkManager networkManager;

	public void StartClient() {
		networkManager.StartClient ();
	}
}
