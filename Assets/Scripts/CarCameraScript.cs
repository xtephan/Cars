using UnityEngine;
using System.Collections;

public class CarCameraScript : MonoBehaviour {
	
	//vars
	public Transform car;
	
	public float distance = 6.4f,
				 height = 1.4f,
				 rotationDamping = 3.0f,
				 heightDamping = 2.0f,
				 zoomRatio = 0.5f;
	
	private Vector3 rotationVector;
	
	
	
	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void LateUpdate () {
		
		// what we want
		float wantedAngel = car.eulerAngles.y;
		float wantedHeight = car.position.y + height;
		
		// what we need
		float myAngel = transform.eulerAngles.y;
		float myHeight = transform.position.y;
		
		//make a smooth transition
		myAngel = Mathf.LerpAngle(myAngel, wantedAngel, rotationDamping * Time.deltaTime);
		myHeight = Mathf.Lerp(myHeight, wantedHeight, heightDamping * Time.deltaTime);
		
		//Quaternion tmp = Quaternion.Euler(0, myAngel, 0);
		Vector3 currentRotation = new Vector3( Quaternion.Euler(0, myAngel, 0).x, Quaternion.Euler(0, myAngel, 0).y, Quaternion.Euler(0, myAngel, 0).z);
		
		//change the possition
		transform.position = car.position;
		transform.position -= Vector3.Scale(currentRotation, Vector3.forward) * distance;
		
		transform.position = new Vector3(transform.position.x, myHeight ,transform.position.z);
		
		//Look at the car
		transform.LookAt(car);
		
	}
}
