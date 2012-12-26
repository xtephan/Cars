using UnityEngine;
using System.Collections;

public class CarControlScript : MonoBehaviour {
	
	//variables
	//Wheels
	public WheelCollider wheelFL, wheelFR, wheelRL, wheelRR; 
	
	public Transform wheelFLTrans, wheelFRTrans, wheelRLTrans,wheelRRTrans;
	
	float maxTorque = 50f;
	
	float highestSpeed = 50;
	float highestReverse = 20;
	
	float lowSpeedSteerAngle = 10;
	float highSpeedSteerAngle = 1;
	float decelarationSpeed = 30;
	
	float topSpeed = 150;
	
	public GUITexture AcceleratePedal;
	public GUITexture BreakPedal;
	
	// Use this for initialization
	void Start () {
	
		//change mass center
		rigidbody.centerOfMass = new Vector3(rigidbody.centerOfMass.x, -0.9f, rigidbody.centerOfMass.z);
		
		
	}
	
	// Update is called once per frame
	void FixedUpdate () {
		ControlCar();
	}
	
	void Update() {
	
		// wheel spin
		wheelFLTrans.Rotate(wheelFL.rpm/60*360*Time.deltaTime, 0, 0);
		wheelFRTrans.Rotate(wheelFR.rpm/60*360*Time.deltaTime, 0, 0);
		wheelRLTrans.Rotate(wheelRL.rpm/60*360*Time.deltaTime, 0, 0);
		wheelRRTrans.Rotate(wheelRR.rpm/60*360*Time.deltaTime, 0, 0);
		
		// wheel steer
		wheelFLTrans.localEulerAngles = new Vector3(wheelFLTrans.localEulerAngles.x, wheelFL.steerAngle + 180 - wheelFLTrans.localEulerAngles.z, wheelFLTrans.localEulerAngles.z);
		wheelFRTrans.localEulerAngles = new Vector3(wheelFRTrans.localEulerAngles.x, wheelFR.steerAngle - wheelFRTrans.localEulerAngles.z, wheelFRTrans.localEulerAngles.z);
		
	}
	
	
	
	//Controls for the car
	private void ControlCar() {
		
		// Compute the speed based on RPM
		float currentSpeed = Mathf.Round( 2 * Mathf.PI * wheelRL.radius * wheelRL.rpm/60 * 100 );
		
		
		// Input for Android
		float multiplySpeedFactor = 0;
		
		if( Input.GetMouseButton(0) && AcceleratePedal.HitTest(Input.mousePosition) )
			multiplySpeedFactor = 1;
		
		if( Input.GetMouseButton(0) && BreakPedal.HitTest(Input.mousePosition) )
			multiplySpeedFactor = -1;
		
		
		if( currentSpeed < topSpeed ) {
			// Apply Torque to move the car
			//wheelRL.motorTorque = maxTorque * Input.GetAxis("Vertical");
			//wheelRR.motorTorque = maxTorque * Input.GetAxis("Vertical");
			wheelRL.motorTorque = maxTorque *  multiplySpeedFactor;
			wheelRR.motorTorque = maxTorque * multiplySpeedFactor;
		} else {
			// No more torque is over the speed limit
			wheelRL.motorTorque = 0;
			wheelRR.motorTorque = 0;
		}
		
		// Decelerate when not pressing any keys
		if( Input.GetButton("Vertical") == false ) {
			wheelRR.brakeTorque = decelarationSpeed;
			wheelRL.brakeTorque = decelarationSpeed;
		} else {
			wheelRR.brakeTorque = 0;
			wheelRL.brakeTorque = 0;
		}
		
		
		float speedFactor = rigidbody.velocity.magnitude/highestSpeed;
		float currentSteerAngle = Mathf.Lerp(lowSpeedSteerAngle,highSpeedSteerAngle,speedFactor);
		
		
		currentSteerAngle *= Input.acceleration.x * 10;
		
		// Apply Steering
		wheelFL.steerAngle = currentSteerAngle;
		wheelFR.steerAngle = currentSteerAngle;
	}
}
