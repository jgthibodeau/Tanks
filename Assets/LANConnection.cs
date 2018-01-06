using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;

public class LANConnection : MonoBehaviour {
	public NetworkManager networkManager;
	public Menu menu;

	public void StartHost() {
		networkManager.StartHost ();
		menu.Hide ();
	}

	public void StartClient() {
		networkManager.StartClient ();
		menu.Hide ();
	}

	public void StartServer() {
		networkManager.StartServer ();
		menu.Hide ();
	}
}
