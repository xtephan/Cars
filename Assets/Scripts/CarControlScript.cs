using UnityEngine;
using System.Collections;

public class CarControlScript : MonoBehaviour {
	
	//variables
	//Wheels
	public WheelCollider wheelFL, wheelFR, wheelRL, wheelRR; 
	
	float maxTorque = 50f;
	
	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
		
		// Apply Torque to move the car
		wheelRL.motorTorque = maxTorque * Input.GetAxis("Vertical");
		wheelRR.motorTorque = maxTorque * Input.GetAxis("Vertical");
		
		// Apply Steering
		wheelFL.steerAngle = 10 * Input.GetAxis("Horizontal");
		wheelFR.steerAngle = 10 * Input.GetAxis("Horizontal");
	}
}
