using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DisableOnDesktop : MonoBehaviour {

	// Use this for initialization
	void Start () {
		if (SystemInfo.deviceType != DeviceType.Handheld) {
			this.gameObject.SetActive (false);
		}
	}
}
