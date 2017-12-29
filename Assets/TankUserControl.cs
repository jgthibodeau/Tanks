using System;
using CnControls;
using UnityEngine;
using DaburuTools;
using UnityEngine.UI;

[RequireComponent(typeof (TankController))]
public class TankUserControl : MonoBehaviour{
	private TankController m_Tank; // the car controller we want to use
	public DaburuTools.Input.GyroControl gyroControl;

	public SimpleJoystick leftJoystick;
	public SimpleJoystick rightJoystick;

	public GameObject bullet;
	public float bulletForce;
	public bool fireFromTurret;
	public float bulletOutDistance;
	public float bulletUpDistance;
	public Transform missileOrigin;
	private bool fired;

	public Text debugText;

	private void Awake(){
	    // get the car controller
		m_Tank = GetComponent<TankController>();
		gyroControl.SnapToPoint ();
	}


	private void FixedUpdate(){
	    // pass the input to the car!
//		float h = Util.GetAxis("Horizontal");
//		float v = Util.GetAxis("Vertical");

		float left = CnInputManager.GetAxis("Vertical");
		float right = CnInputManager.GetAxis("Vertical Right");
		bool brake = CnInputManager.GetButton ("Brake");

		if (brake) {
			leftJoystick.Reset ();
			rightJoystick.Reset ();
		}

		m_Tank.Move (left, right, (brake ? 1 : 0));

		bool recalibrate = CnInputManager.GetButton ("Recalibrate");
		if (recalibrate) {
			gyroControl.SnapToPoint ();
		}
			
		bool fire = CnInputManager.GetButton ("Fire");
		if (fire && !fired) {
			fired = true;
//			GameObject bulletInst = GameObject.Instantiate (bullet);
			Vector3 position = missileOrigin.position + missileOrigin.forward * bulletOutDistance + missileOrigin.up * bulletUpDistance;
			GameObject bulletInst = Instantiate (bullet, position, missileOrigin.rotation);

//			if (fireFromTurret) {
//				bulletInst.transform.position = missileOrigin.position;
//			}
//			bulletInst.tag = gameObject.tag;
//			debugText.text = "adding force";
			bulletInst.GetComponent<Rigidbody> ().AddForce (bulletInst.transform.forward * bulletForce);
//			debugText.text = "added force "+bulletInst.transform.forward * bulletForce;

//			bulletInst.GetComponent<Rigidbody> ().AddForce (Vector3.forward * bulletForce);
		} else if (!fire) {
			fired = false;
		}
    }
}
