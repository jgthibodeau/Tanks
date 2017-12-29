using System;
using CnControls;
using UnityEngine;
using DaburuTools;

[RequireComponent(typeof (TankController))]
public class TankUserControl : MonoBehaviour{
	private TankController m_Car; // the car controller we want to use
	public DaburuTools.Input.GyroControl gyroControl;

	public SimpleJoystick leftJoystick;
	public SimpleJoystick rightJoystick;

	public GameObject bullet;
	public float bulletForce;
	public float bulletOutDistance;
	public float bulletUpDistance;
	private bool fired;

	private void Awake(){
	    // get the car controller
	    m_Car = GetComponent<TankController>();
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

		m_Car.Move (left, right, (brake ? 1 : 0));

		bool recalibrate = CnInputManager.GetButton ("Recalibrate");
		if (recalibrate) {
			gyroControl.SnapToPoint ();
		}
			
		bool fire = CnInputManager.GetButton ("Fire");
		if (fire && !fired) {
			fired = true;
			GameObject bulletInst = GameObject.Instantiate (bullet);
			bulletInst.transform.position = gyroControl.transform.position + gyroControl.transform.forward * bulletOutDistance + gyroControl.transform.up * bulletUpDistance;
			bulletInst.transform.rotation = gyroControl.transform.rotation;
			bulletInst.GetComponent<Rigidbody> ().AddForce (gyroControl.transform.forward * bulletForce);
		} else if (!fire) {
			fired = false;
		}
    }
}
