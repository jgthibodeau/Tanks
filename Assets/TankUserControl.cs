using System;
using CnControls;
using UnityEngine;
using DaburuTools;
using UnityEngine.UI;
using UnityEngine.Networking;

[RequireComponent(typeof (TankController))]
public class TankUserControl : NetworkBehaviour{
	private TankController m_Tank; // the car controller we want to use

	public SimpleJoystick leftJoystick;
	public SimpleJoystick rightJoystick;

	public GameObject bullet;
	public float bulletForce;
	public Transform missileOrigin;
	private bool fired;

	public Text debugText;

	private void Awake(){
	    // get the car controller
		m_Tank = GetComponent<TankController>();
	}

	private void FixedUpdate(){
		if (!isLocalPlayer) {
			return;
		}
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
			Camera.main.GetComponent<DaburuTools.Input.GyroControl> ().SnapToPoint ();
		}
			
		bool fire = CnInputManager.GetButton ("Fire");
		if (fire && !fired) {
			fired = true;
			CmdFire ();
		} else if (!fire) {
			fired = false;
		}
    }

	[Command]
	private void CmdFire() {
		GameObject bulletInst = Instantiate (bullet, missileOrigin.position, missileOrigin.rotation);

//		debugText.text = "fired - adding force";
//		bulletInst.GetComponent<Rigidbody> ().velocity = (bulletInst.transform.forward * bulletForce);
//		debugText.text = "fired - added force";

		NetworkServer.Spawn (bulletInst);
		bulletInst.GetComponent<Rigidbody> ().AddForce (bulletInst.transform.forward * bulletForce);
//		debugText.text = "fired - spawned in server";
	}
}
