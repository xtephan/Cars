using UnityEngine;
using System.Collections;

public class CarControlScript : MonoBehaviour {
	
	//variables
	//Wheels
	public WheelCollider wheelFL, wheelFR, wheelRL, wheelRR; 
	
	public Transform wheelFLTrans, wheelFRTrans, wheelRLTrans,wheelRRTrans;
	
	float maxTorque = 50f;
	float highestSpeed = 50;
	float lowSpeedSteerAngle = 10;
	float highSpeedSteerAngle = 1;
	float decelarationSpeed = 30;
	
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
		// Apply Torque to move the car
		wheelRL.motorTorque = maxTorque * Input.GetAxis("Vertical");
		wheelRR.motorTorque = maxTorque * Input.GetAxis("Vertical");
		
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
		
		currentSteerAngle *= Input.GetAxis("Horizontal");
		// Apply Steering
		wheelFL.steerAngle = currentSteerAngle;
		wheelFR.steerAngle = currentSteerAngle;
	}
}
