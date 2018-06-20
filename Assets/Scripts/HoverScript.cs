using UnityEngine;
using System.Collections;

public class HoverScript : MonoBehaviour {
	// Minimum/Maximum vehicle speed
	public float MIN_SPEED = -5, MAX_SPEED = 300;
	// Minimum/Maximum acceleration (Driving Force) that can be applied in a single physics Time-Step 
	public float MIN_ACCELERATION = -50, MAX_ACCELERATION = 500;
	// The collision layer used by the track faces
	int trackLayer = 1 << 8;

	//public LapData currentLapData;
	
	// Used to detect the Track
	private RaycastHit hitInfo;
	// The distance to hover above the track
	public float hoverHeight = 2;
	// Basic variables for defining manouberability and speed.  
	// rollScaler defines how far the vehicle should rotate around the Z axis while turning corners
	// drivingForce it the amount of acceleration to be applied in the current Physics Time-Step  
	public float moveSpeed, rotationSpeed, acceleration, rollScaler, drivingForce;
	// Reference to the child transform that holds colliders/renderers
	private Transform body;
	// Reference to the main parent transform
	private Transform vehicleTransform;
	// Reference to the vehicles tigidbody
	public Rigidbody HCRigidBody;
	// Used to influence linear/angular velocity
	float thrust, torque; 
	// Used to play the engine sound
	AudioSource engineAudio;
	public float Thrust {
		get{ return thrust; }
		set{ thrust = value; }
	}
	
	public float Torque {
		get{ return torque; }
		set{ torque = value; }
	}
	
	bool usingFixedUpdate;
	// Use this for initialization
	void Start () {
		Init();
	}
	
	// Use this for re-initialization
	public void Init() {
		// Store references tro the relevant components
		usingFixedUpdate = true;
		vehicleTransform = transform;
		body = vehicleTransform.Find("Body");
		engineAudio = GetComponent<AudioSource>();
		HCRigidBody = GetComponent<Rigidbody>();
	}
	
	
	
	// Update is called once per frame
	void Update () {
		float delta = Time.deltaTime;
		if(!usingFixedUpdate)
			ReorientVehicle(delta, false);
		// applu a positive value relative to the vehicles speed as the engine audios pitch
		engineAudio.pitch = Mathf.Lerp(engineAudio.pitch, 0.5f+ (Mathf.Abs(moveSpeed)/MAX_SPEED), delta);
		
		if(Input.GetKeyDown(KeyCode.U))
			usingFixedUpdate = !usingFixedUpdate;
	}
	
	// Cached track rotation value
	Quaternion trackRotation;
	// reorient the vehicle to the track
	void ReorientVehicle(float delta, bool fixedUpdate) {
		// Fire a Ray in the parent transforms local down direction checking for an intersection with the track 
		if(Physics.Raycast(vehicleTransform.position, -vehicleTransform.up, out hitInfo, 30, trackLayer)) {
			// if the vehicle is not hovering at the correct height, reposition it.
			if(hitInfo.distance != hoverHeight) {
				if(fixedUpdate) {
					HCRigidBody.AddRelativeForce((hitInfo.point+(vehicleTransform.up*hoverHeight)-vehicleTransform.position)*1400);
				}
				else {
					vehicleTransform.position = hitInfo.point+vehicleTransform.up*hoverHeight;
				}
			}
			
			// Get the relevant rotation between the parent transforms local up direction and the hit normal (track faces local up)
			trackRotation = Quaternion.FromToRotation(vehicleTransform.up, hitInfo.normal);
			//trackRotation *= Quaternion.FromToRotation(vehicleTransform.forward, Vector3.Cross(hitInfo.normal, -vehicleTransform.right));
			// align the vehicle to the track along its X nad Z axes, while mainyaining the original Y axis rotation
			vehicleTransform.rotation = Quaternion.Slerp(vehicleTransform.rotation, trackRotation * Quaternion.Euler(0, vehicleTransform.rotation.eulerAngles.y, 0), 5*delta);
			// cache the new rotation values
			Vector3 newEuler = vehicleTransform.rotation.eulerAngles;
			// rotate the vehicles body around the Z axis (with colliders/renderers attached) based on the value of torque
			body.rotation = Quaternion.Slerp(body.rotation,  Quaternion.Euler(newEuler.x, newEuler.y, -torque*rollScaler), 5*delta);
		}
	}
	
	void FixedUpdate() {
//		Reorient Vehicle using Fixed Time-Step
		float fixedDelta = Time.fixedDeltaTime;
		if(usingFixedUpdate)
			ReorientVehicle(fixedDelta, true);
		// Calculates the vehicles velocity then adds an amount to it based on the vehicles thrust and acceleration.
		// the value is then clamped to nithin a min/max range of total acceleration for the Time-Step 
		drivingForce = Mathf.Clamp(HCRigidBody.velocity.magnitude+(thrust*acceleration*fixedDelta), MIN_ACCELERATION, MAX_ACCELERATION);
		// Rotate the vehicle
		HCRigidBody.AddRelativeTorque(Vector3.up*torque*rotationSpeed);
		// Accelerate Decelerate the vehicle
		HCRigidBody.AddRelativeForce(Vector3.forward*drivingForce, ForceMode.Acceleration);
		ClampVelocity();
	}
	
	// Camps the vehicles velocity vetween MIN_SPEED and MAX_SPEED
	void ClampVelocity(){
		moveSpeed = Mathf.Clamp(HCRigidBody.velocity.magnitude, MIN_SPEED, MAX_SPEED);
		HCRigidBody.velocity = HCRigidBody.velocity.normalized*moveSpeed;
	}
	
	// Spawns the vehucle at a given transforms location and orientation
	public void Respawn(Transform spawnPoint) {
		HCRigidBody.velocity = Vector3.zero;
		moveSpeed = 0;
		torque = 0;
		thrust = 0;
		vehicleTransform.position = spawnPoint.position;
		vehicleTransform.rotation = spawnPoint.rotation;
		body.position = vehicleTransform.position;
		body.rotation = vehicleTransform.rotation;
	}
}
