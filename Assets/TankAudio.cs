using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TankAudio : MonoBehaviour {
	public float minEnginePitch;
	public float maxEnginePitch;
	public float minEngineVolume;
	public float maxEngineVolume;
	public float maxRpm;
	public AudioSource engineSource;

	public float minTreadPitch;
	public float maxTreadPitch;
	public float minTreadVolume;
	public float maxTreadVolume;
//	public float maxRpm;
	public AudioSource treadSource;

	public TankController tankController;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		float rpm = Mathf.Abs (tankController.m_WheelCollidersLeft[0].rpm) + Mathf.Abs (tankController.m_WheelCollidersRight[0].rpm);
		float rpmPercent = Mathf.Clamp (rpm / maxRpm, 0, 1f);

		float enginePitch = minEnginePitch + (maxEnginePitch - minEnginePitch) * rpmPercent;
		engineSource.pitch = enginePitch;
		float engineVolume = minEngineVolume + (maxEngineVolume - minEngineVolume) * rpmPercent;
		engineSource.volume = engineVolume;

		float treadPitch = minTreadPitch + (maxTreadPitch - minTreadPitch) * rpmPercent;
		treadSource.pitch = treadPitch;
		float treadVolume = minTreadVolume + (maxTreadVolume - minTreadVolume) * rpmPercent;
		treadSource.volume = treadVolume;
	}
}
