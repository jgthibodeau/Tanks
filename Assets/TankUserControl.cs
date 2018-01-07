using System;
using UnityStandardAssets.CrossPlatformInput;
using UnityEngine;
using DaburuTools;
using UnityEngine.UI;
using UnityEngine.Networking;

[RequireComponent(typeof (TankController))]
public class TankUserControl : NetworkBehaviour{
	private TankController m_Tank;
	public GameObject UI;
	public CameraShake cameraShake;
	public float fireCameraShakeAmount;

	public Joystick leftJoystick;
	public Joystick rightJoystick;

	public GameObject bullet;
	public ParticleSystem barrelSmoke;
	public float bulletForce;
	public Transform missileOrigin;
	private bool fired;

	public float maxFireCoolDown = 1f;
	public float fireCoolDown;
	public Image fireCoolDownImage;
	public AudioSource GunSource;

	public Text debugText;

	private void Awake(){
		UI.SetActive (isLocalPlayer);
		m_Tank = GetComponent<TankController>();
	}

	private void FixedUpdate(){
		if (!isLocalPlayer) {
			return;
		}

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
    }

	private void Fire() {
		if (fireCoolDown <= 0) {
			CmdFire ();
			barrelSmoke.Play (true);
			cameraShake.shake = fireCameraShakeAmount;
			fireCoolDown = maxFireCoolDown;
		}
	}

	[Command]
	private void CmdFire() {
		GunSource.Play ();
		GameObject bulletInst = Instantiate (bullet, missileOrigin.position, missileOrigin.rotation);
		Collider bulletCollider = bulletInst.GetComponent<Collider> ();
		foreach (Collider collider in GetComponentsInChildren<Collider> ()) {
			Physics.IgnoreCollision (bulletCollider, collider);
		}
		NetworkServer.Spawn (bulletInst);
		bulletInst.GetComponent<Rigidbody> ().AddForce (bulletInst.transform.forward * bulletForce);
	}
}
