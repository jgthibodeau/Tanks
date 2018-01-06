using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UINetworkManagerHUD : MonoBehaviour {
	RectTransform mainScreen;
	RectTransform serverScreen;
	RectTransform clientScreen;
	RectTransform matchScreen;

	// Use this for initialization
	void Start () {
		mainScreen.gameObject.SetActive (true);
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
