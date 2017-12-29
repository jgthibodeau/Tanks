using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TankTurret : MonoBehaviour {

	[SerializeField] private Transform m_Turret;
	[SerializeField] private Transform m_Camera;

	private bool gyroEnabled;
	private Gyroscope gyro;
	private Quaternion origin = Quaternion.identity;

	// Use this for initialization
	void Start () {
		gyroEnabled = EnableGyro ();
	}
	
	// Update is called once per frame
	void Update () {
		if (gyroEnabled) {
			Quaternion unityGyro = new Quaternion (gyro.attitude.x, gyro.attitude.z, gyro.attitude.y, -gyro.attitude.w);
			Vector3 gyroRotation = (unityGyro * origin).eulerAngles;

//			Vector3 turretRotation = new Vector3 (gyroRotation.x, gyroRotation.y, gyroRotation.z);
//			m_Turret.localEulerAngles = turretRotation;
//
//			Vector3 cameraRotation = new Vector3 (gyroRotation.x, 90f, 90f);
//			m_Camera.localEulerAngles = cameraRotation;

//			m_Turret.local = gyroRotation;
//			m_Turret.localEulerAngles = gyroRotation;


			//gyro x is forward/back
			//gyro y is rotate cw/ccw
			//gyro z is tilt left right
			m_Turret.localRotation = Quaternion.Euler (-gyroRotation.x, gyroRotation.z, gyroRotation.y);

		} else {
			m_Turret.localRotation = origin;
		}
	}

	private bool EnableGyro() {
		if (SystemInfo.supportsGyroscope) {
			gyro = Input.gyro;
			gyro.enabled = true;
			return true;
		}
		return false;
	}

	private void Recenter() {
		origin = Quaternion.Inverse (transform.localRotation);
	}
}
