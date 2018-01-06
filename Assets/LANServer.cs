using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class LANServer : MonoBehaviour {
	public NetworkManager networkManager;

	public void StartServer() {
		networkManager.StartServer ();
	}
}
