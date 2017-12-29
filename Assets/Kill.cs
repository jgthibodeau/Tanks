﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Kill : MonoBehaviour {
	public float lifeTimeInSeconds;
	public float startTime;

	// Use this for initialization
	void Start () {
		startTime = Time.time;
	}
	
	// Update is called once per frame
	void Update () {
		if (Time.time >= startTime + lifeTimeInSeconds) {
			GameObject.Destroy (this.gameObject);
		}
	}
}
