using UnityEngine;
using System.Collections;

public class CarControlScript : MonoBehaviour {
	
	//variables
	//Wheels
	public WheelCollider wheelFL, wheelFR, wheelRL, wheelRR; 
	
	float maxTorque = 50f;
	
	// Use this for initialization
	void Start () {
	
		//change mass center
		rigidbody.centerOfMass = new Vector3(rigidbody.centerOfMass.x, -0.9f, rigidbody.centerOfMass.z);
		
		
	}
	
	// Update is called once per frame
	void FixedUpdate () {
		
		// Apply Torque to move the car
		wheelRL.motorTorque = maxTorque * Input.GetAxis("Vertical");
		wheelRR.motorTorque = maxTorque * Input.GetAxis("Vertical");
		
		// Apply Steering
		wheelFL.steerAngle = 10 * Input.GetAxis("Horizontal");
		wheelFR.steerAngle = 10 * Input.GetAxis("Horizontal");
	}
}
