#pragma strict

var car: Transform;
var distance : float = 6.4;
var height : float = 1.4;
var rotationDamping : float = 3.0;
var heightDamping : float = 2.0;
var zoomRatio : float = 0.5;
var defaultFOV : float = 60;

private var rotationVector : Vector3;

function Start () {

}

function LateUpdate () {

	var wantedAngle = rotationVector.y;
	var wantedHeight = car.position.y + height;
	
	var myAngle = transform.eulerAngles.y;
	var myHeight = transform.position.y;
		
	//make a smooth transition
	myAngle = Mathf.LerpAngle(myAngle, wantedAngle, rotationDamping * Time.deltaTime);
	myHeight = Mathf.Lerp(myHeight, wantedHeight, heightDamping * Time.deltaTime);
	
	var currentRotation = Quaternion.Euler(0, myAngle, 0);
	
	transform.position = car.position;
	transform.position -= currentRotation * Vector3.forward * distance;
	transform.position.y = myHeight;
	
	transform.LookAt(car);

}

function FixedUpdate() {

	//revert camera when going backworth
	var localVelocity = car.InverseTransformDirection(car.rigidbody.velocity);
	
	if( localVelocity.z < -0.5 ) 
		rotationVector.y = car.eulerAngles.y + 180;
	else 
		rotationVector.y = car.eulerAngles.y;

	// create zoom effect
	var acceleration = car.rigidbody.velocity.magnitude;
	camera.fieldOfView = defaultFOV + acceleration * zoomRatio * Time.deltaTime;

}