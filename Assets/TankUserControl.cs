using System;
using UnityStandardAssets.CrossPlatformInput;
using UnityEngine;
using DaburuTools;
using UnityEngine.UI;
using UnityEngine.Networking;

[RequireComponent(typeof (TankController))]
public class TankUserControl : NetworkBehaviour{
	private TankController m_Tank; // the car controller we want to use
	public CameraShake cameraShake;
	public float fireCameraShakeAmount;

	public Joystick leftJoystick;
	public Joystick rightJoystick;

	public GameObject bullet;
	public float bulletForce;
	public Transform missileOrigin;
	private bool fired;

	public float maxFireCoolDown = 1f;
	public float fireCoolDown;
	public Image fireCoolDownImage;

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

//		float left = CnInputManager.GetAxis("Vertical");
//		float right = CnInputManager.GetAxis("Vertical Right");
//		bool brake = CnInputManager.GetButton ("Brake");
		float left = MyInputManager.GetAxis("Vertical");
		float right = MyInputManager.GetAxisRaw("Vertical Right");
		bool brake = MyInputManager.GetButton ("Brake");

		if (brake) {
			leftJoystick.Reset ();
			rightJoystick.Reset ();
		}

		m_Tank.Move (left, right, (brake ? 1 : 0));

		bool recalibrate = MyInputManager.GetButton ("Recalibrate");
		if (recalibrate) {
			Camera.main.GetComponent<DaburuTools.Input.GyroControl> ().SnapToPoint ();
		}

		if (fireCoolDown > 0) {
			fireCoolDown -= Time.deltaTime;
		}
		fireCoolDownImage.fillAmount = fireCoolDown / maxFireCoolDown;
		bool fire = MyInputManager.GetButton ("Fire");
		if (fire) {
			Fire ();
		}
//		if (fire && !fired) {
//			cameraShake.shake = fireCameraShakeAmount;
//			fired = true;
//			CmdFire ();
//		} else if (!fire) {
//			fired = false;
//		}
    }

	private void Fire() {
		if (fireCoolDown <= 0) {
			cameraShake.shake = fireCameraShakeAmount;
			fireCoolDown = maxFireCoolDown;
			CmdFire ();
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
