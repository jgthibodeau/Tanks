using System;
using UnityEngine;
using UnityStandardAssets.Vehicles.Car;

internal enum SpeedType
{
    MPH,
    KPH
}

public class TankController : MonoBehaviour
{
//    [SerializeField] private WheelCollider[] m_WheelColliders = new WheelCollider[4];
//    [SerializeField] private GameObject[] m_WheelMeshes = new GameObject[4];
    [SerializeField] private WheelEffects[] m_WheelEffects = new WheelEffects[4];

	[SerializeField] private WheelCollider[] m_WheelCollidersLeft = new WheelCollider[2];
	[SerializeField] private GameObject[] m_WheelMeshesLeft = new GameObject[2];
	[SerializeField] private WheelEffects[] m_WheelEffectsLeft = new WheelEffects[2];
	[SerializeField] private WheelCollider[] m_WheelCollidersRight = new WheelCollider[2];
	[SerializeField] private GameObject[] m_WheelMeshesRight = new GameObject[2];
	[SerializeField] private WheelEffects[] m_WheelEffectsRight = new WheelEffects[2];

	[SerializeField] private Vector3 m_CentreOfMassOffset;
	[SerializeField] private float m_WheelDamping;
    [Range(0, 1)] [SerializeField] private float m_TractionControl; // 0 is no traction control, 1 is full interference
    [SerializeField] private float m_FullTorqueOverAllWheels;
    [SerializeField] private float m_ReverseTorque;
    [SerializeField] private float m_MaxHandbrakeTorque;
    [SerializeField] private float m_Downforce = 100f;
    [SerializeField] private SpeedType m_SpeedType;
    [SerializeField] private float m_Topspeed = 200;
    [SerializeField] private static int NoOfGears = 5;
    [SerializeField] private float m_RevRangeBoundary = 1f;
    [SerializeField] private float m_SlipLimit;
    [SerializeField] private float m_BrakeTorque;

//    private Quaternion[] m_WheelMeshLocalRotations;
    private Vector3 m_Prevpos, m_Pos;
    private int m_GearNum;
    private float m_GearFactor;
    private float m_OldRotation;
    private float m_CurrentTorque;
    private Rigidbody m_Rigidbody;
    private const float k_ReversingThreshold = 0.01f;

    public bool Skidding { get; private set; }
    public float BrakeInput { get; private set; }
    public float CurrentSpeed{ get { return m_Rigidbody.velocity.magnitude*2.23693629f; }}
    public float MaxSpeed{get { return m_Topspeed; }}
    public float Revs { get; private set; }
	public float AccelInput { get; private set; }
	public float AccelLeftInput { get; private set; }
	public float AccelRightInput { get; private set; }

    // Use this for initialization
    private void Start()
    {
		foreach (Collider collider1 in transform.GetComponentsInChildren<Collider> ()) {
			foreach (Collider collider2 in transform.GetComponentsInChildren<Collider> ()) {
				Physics.IgnoreCollision(collider1, collider2);
			}
		}

//        m_WheelMeshLocalRotations = new Quaternion[4];
//        for (int i = 0; i < 4; i++)
//        {
//            m_WheelMeshLocalRotations[i] = m_WheelMeshes[i].transform.localRotation;
//        }
		foreach (WheelCollider wheel in m_WheelCollidersRight) {
			wheel.wheelDampingRate = m_WheelDamping;
		}
		foreach (WheelCollider wheel in m_WheelCollidersLeft) {
			wheel.wheelDampingRate = m_WheelDamping;
		}
        m_WheelCollidersLeft[0].attachedRigidbody.centerOfMass = m_CentreOfMassOffset;

        m_MaxHandbrakeTorque = float.MaxValue;

        m_Rigidbody = GetComponent<Rigidbody>();
        m_CurrentTorque = m_FullTorqueOverAllWheels - (m_TractionControl*m_FullTorqueOverAllWheels);
    }


    private void GearChanging()
    {
        float f = Mathf.Abs(CurrentSpeed/MaxSpeed);
        float upgearlimit = (1/(float) NoOfGears)*(m_GearNum + 1);
//        float downgearlimit = (1/(float) NoOfGears)*m_GearNum;
		float downgearlimit = -upgearlimit;

        if (m_GearNum > 0 && f < downgearlimit)
        {
            m_GearNum--;
        }

        if (f > upgearlimit && (m_GearNum < (NoOfGears - 1)))
        {
            m_GearNum++;
        }
    }


    // simple function to add a curved bias towards 1 for a value in the 0-1 range
    private static float CurveFactor(float factor)
    {
        return 1 - (1 - factor)*(1 - factor);
    }


    // unclamped version of Lerp, to allow value to exceed the from-to range
    private static float ULerp(float from, float to, float value)
    {
        return (1.0f - value)*from + value*to;
    }


    private void CalculateGearFactor()
    {
        float f = (1/(float) NoOfGears);
        // gear factor is a normalised representation of the current speed within the current gear's range of speeds.
        // We smooth towards the 'target' gear factor, so that revs don't instantly snap up or down when changing gear.
        var targetGearFactor = Mathf.InverseLerp(f*m_GearNum, f*(m_GearNum + 1), Mathf.Abs(CurrentSpeed/MaxSpeed));
        m_GearFactor = Mathf.Lerp(m_GearFactor, targetGearFactor, Time.deltaTime*5f);
    }


    private void CalculateRevs()
    {
        // calculate engine revs (for display / sound)
        // (this is done in retrospect - revs are not used in force/power calculations)
        CalculateGearFactor();
        var gearNumFactor = m_GearNum/(float) NoOfGears;
        var revsRangeMin = ULerp(0f, m_RevRangeBoundary, CurveFactor(gearNumFactor));
        var revsRangeMax = ULerp(m_RevRangeBoundary, 1f, gearNumFactor);
        Revs = ULerp(revsRangeMin, revsRangeMax, m_GearFactor);
    }

	public void Move(float accelLeft, float accelRight, float footbrake)
	{
		Quaternion quat;
		Vector3 position;
		int index = 0;
		foreach (WheelCollider wheelCollider in m_WheelCollidersLeft) {
			wheelCollider.GetWorldPose(out position, out quat);

			m_WheelMeshesLeft[index].transform.position = position;
			m_WheelMeshesLeft[index].transform.Rotate(0, 0, wheelCollider.rpm / 60 * 360 * Time.deltaTime);

			index++;
		}
		index = 0;
		foreach (WheelCollider wheelCollider in m_WheelCollidersRight) {
			wheelCollider.GetWorldPose(out position, out quat);

			m_WheelMeshesRight[index].transform.position = position;
			m_WheelMeshesRight[index].transform.Rotate(0, 0, wheelCollider.rpm / 60 * 360 * Time.deltaTime);

			index++;
		}

		//clamp input values
		AccelLeftInput = accelLeft = Mathf.Clamp(accelLeft, -1, 1);
		AccelRightInput = accelRight = Mathf.Clamp(accelRight, -1, 1);
//		BrakeInput = footbrake = -1*Mathf.Clamp(footbrake, -1, 0);
		BrakeInput = footbrake = Mathf.Clamp(footbrake, 0, 1);

		ApplyDrive(accelLeft, accelRight, footbrake);
//		CapSpeed();


//		CalculateRevs();
//		GearChanging();

		AddDownForce();
//		CheckForWheelSpin();
//		TractionControl();
	}


    private void CapSpeed()
    {
        float speed = m_Rigidbody.velocity.magnitude;
        switch (m_SpeedType)
        {
            case SpeedType.MPH:

                speed *= 2.23693629f;
                if (speed > m_Topspeed)
                    m_Rigidbody.velocity = (m_Topspeed/2.23693629f) * m_Rigidbody.velocity.normalized;
                break;

            case SpeedType.KPH:
                speed *= 3.6f;
                if (speed > m_Topspeed)
                    m_Rigidbody.velocity = (m_Topspeed/3.6f) * m_Rigidbody.velocity.normalized;
                break;
        }
    }
		
	private void ApplyDrive(float accelLeft, float accelRight, float footbrake)
	{
//		float thrustTorqueLeft = accelLeft * (m_CurrentTorque / m_WheelCollidersLeft.Length);
		float thrustTorqueLeft = accelLeft * (m_FullTorqueOverAllWheels / m_WheelCollidersLeft.Length);
		foreach (WheelCollider wheel in m_WheelCollidersLeft) {
			wheel.motorTorque = thrustTorqueLeft;
			wheel.brakeTorque = m_BrakeTorque*footbrake;
		}

//		float thrustTorqueRight = accelRight * (m_CurrentTorque / m_WheelCollidersRight.Length);
		float thrustTorqueRight = accelRight * (m_FullTorqueOverAllWheels / m_WheelCollidersRight.Length);
		foreach (WheelCollider wheel in m_WheelCollidersRight) {
			wheel.motorTorque = thrustTorqueRight;
			wheel.brakeTorque = m_BrakeTorque*footbrake;
		}


//		for (int i = 0; i < 4; i++)
//		{
//			if (CurrentSpeed > 5 && Vector3.Angle(transform.forward, m_Rigidbody.velocity) < 50f)
//			{
//				m_WheelColliders[i].brakeTorque = m_BrakeTorque*footbrake;
//			}
//			else if (footbrake > 0)
//			{
//				m_WheelColliders[i].brakeTorque = 0f;
//				m_WheelColliders[i].motorTorque = -m_ReverseTorque*footbrake;
//			}
//		}
	}


    // this is used to add more grip in relation to speed
    private void AddDownForce()
    {
        m_WheelCollidersLeft[0].attachedRigidbody.AddForce(-transform.up*m_Downforce*
			m_WheelCollidersLeft[0].attachedRigidbody.velocity.magnitude);
    }


    // checks if the wheels are spinning and is so does three things
    // 1) emits particles
    // 2) plays tiure skidding sounds
    // 3) leaves skidmarks on the ground
    // these effects are controlled through the WheelEffects class
//    private void CheckForWheelSpin()
//    {
//        // loop through all wheels
//        for (int i = 0; i < 4; i++)
//        {
//            WheelHit wheelHit;
//            m_WheelColliders[i].GetGroundHit(out wheelHit);
//
//            // is the tire slipping above the given threshhold
//            if (Mathf.Abs(wheelHit.forwardSlip) >= m_SlipLimit || Mathf.Abs(wheelHit.sidewaysSlip) >= m_SlipLimit)
//            {
//                m_WheelEffects[i].EmitTyreSmoke();
//
//                // avoiding all four tires screeching at the same time
//                // if they do it can lead to some strange audio artefacts
//                if (!AnySkidSoundPlaying())
//                {
//                    m_WheelEffects[i].PlayAudio();
//                }
//                continue;
//            }
//
//            // if it wasnt slipping stop all the audio
//            if (m_WheelEffects[i].PlayingAudio)
//            {
//                m_WheelEffects[i].StopAudio();
//            }
//            // end the trail generation
//            m_WheelEffects[i].EndSkidTrail();
//        }
//    }

    // crude traction control that reduces the power to wheel if the car is wheel spinning too much
    private void TractionControl()
    {
		WheelHit wheelHit;
		foreach (WheelCollider wheel in m_WheelCollidersLeft) {
			wheel.GetGroundHit (out wheelHit);
			AdjustTorque (wheelHit.forwardSlip);
		}
		foreach (WheelCollider wheel in m_WheelCollidersRight) {
			wheel.GetGroundHit (out wheelHit);
			AdjustTorque (wheelHit.forwardSlip);
		}
    }


    private void AdjustTorque(float forwardSlip)
    {
        if (forwardSlip >= m_SlipLimit && m_CurrentTorque >= 0)
        {
            m_CurrentTorque -= 10 * m_TractionControl;
        }
        else
        {
            m_CurrentTorque += 10 * m_TractionControl;
            if (m_CurrentTorque > m_FullTorqueOverAllWheels)
            {
                m_CurrentTorque = m_FullTorqueOverAllWheels;
            }
        }
    }


    private bool AnySkidSoundPlaying()
    {
        for (int i = 0; i < 4; i++)
        {
            if (m_WheelEffects[i].PlayingAudio)
            {
                return true;
            }
        }
        return false;
    }

//	private void OnCollisionEnter (Collision collision) {
//		if (collision.gameObject.tag == gameObject.tag) {
//			Physics.IgnoreCollision(collision.collider, GetComponent<Collider>());
//		}
//	}
}
