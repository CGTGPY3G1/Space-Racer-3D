using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

[System.Serializable]
public class Drone : MonoBehaviour {
	public float wpUpdateDistance;
	public SensorData sensorData;
	
	// sensor transforms
	[Header("Transforms")]
	public Transform backLeft;
	public Transform backRight;
	public Transform front;
	
	//removed (no longer needed)
//	public Transform frontLeft;
//	public Transform frontRight;
	
	// checkpoint tracking data
	[Header("Checkpoint Data")]
	public int newCheckpoint, targetCheckpoint, newSegment, noOfCollisions;
	public Vector3 targetWaypoint;
	int pointCrossed, targetPoint;
	
	// lap start and end times
	public float lastLapTime;
	private float lapStartTime;
	public float LapTime {
		get{return Time.time-lapStartTime; }
	}
	
	Vector3 trackForward;
	public bool IsFacingForward {
		get { return (Vector3.Dot(transform.forward, trackForward) > -0.3f); }
	}
	
	// Cached transform component
	private Transform droneTransform;
	// UI text components
	public Text currentLapText, lapTimer;
	// a reference to the tracks PathBuilder
	public PathBuilder pathBuilder;
	// the tracking data for the current lap
	public LapData currentLapData;
	// boundary collision layer
	int collisionLayer = 1<<10;
	// the amount to increment the lap by
	int lapIncrement;
	// the highest lap reached
	int lapReached;
	public int LapReached {
		get { return lapReached; }
	}
	// Raycast intersection data
	RaycastHit hit;
	
	public GameObject CrashPrefab;
	// reference to the vehicles HoverScript 
	HoverScript hoverScript;
	public HoverScript HoverScript {
		get { return hoverScript; }
	}
	
	// time untill the vehicle is destroyed
	public float deathTimer;
	// used to disable/enable the player/AI controls
	bool isEnabled;
	public bool IsEnabled {
		get { return isEnabled; }
		set { isEnabled = value; }
	}
	
	public bool incrementingLap;
	public PositionInfo positionInfo;
	List<LapData> completedLapData;
	public bool aiControlled;
	public List<LapData> GetLapData() {
		return completedLapData;
	}

	// Use this for initialization
	void Start () {

	}
	
	// set initial drone values
	public void Init() {
		lapStartTime = -1000;
		isEnabled = false;
		lapReached = 0;
		completedLapData = new List<LapData>();
		hoverScript = GetComponent<HoverScript>();
		droneTransform = transform;
		noOfCollisions = 0;
		pathBuilder = GameObject.Find("PathBuilder").GetComponent<PathBuilder>();
		positionInfo = new PositionInfo();
		positionInfo.currentSegment = pathBuilder.segments.Count-1;
		positionInfo.currentCheckpoint = 0;
		positionInfo.droneName = droneTransform.name;
		targetCheckpoint = pathBuilder.segments[0].start;
		targetPoint = 0;
		CheckPointScript newTarget = pathBuilder.waypoints[targetCheckpoint].GetComponent<CheckPointScript>();
		targetWaypoint = newTarget.getPoint(targetPoint);
		trackForward = newTarget.checkPointTransform.forward;
		SetUpSensors();
		// set default death timer to -10000
		deathTimer = -10000;
	}
	
	// Update is called once per frame
	void Update () {
		// if the drone id enable, respond to the relevant controls
		if(isEnabled) {
			if(aiControlled) {
				UpdateAI();
			}
			else {
				hoverScript.Torque = Input.GetAxis("Horizontal");
				hoverScript.Thrust = Input.GetAxis("Vertical");
			}
		}
		// else if the death timer is not set to the default value
		else if (deathTimer != -10000) {
			// count down to death
			deathTimer -= Time.deltaTime;
			if(deathTimer <= 0)  {
				// destroy the vehicle
				Destroy(this.gameObject);
			}
		}
	}
	
	void LateUpdate () {
		// update the lap timer if the referece to it has
		// beem assigned and the lapReached is at least 1
		if(lapTimer && lapReached > 0) {
			lapTimer.text = LapTime.ToString("0.##");
		}
	}
	void UpdateAI() {
		// the angle to turn by (in radians)
		float angle = 0;
		// will the vehicle, travelling in its current forward direction
		bool hitCentre = false; 
//		float frontLength = Mathf.Abs(1+hoverScript.moveSpeed*sensorData.frontRayScale);
		
		// get angle to target
		if (targetWaypoint != Vector3.zero) {
			Vector3 p = targetWaypoint;
			p.y = droneTransform.position.y;
			Debug.DrawLine(droneTransform.position, p, Color.white);
			p -= droneTransform.position;
			angle += Mathf.Clamp(TurnToPoint(droneTransform.forward, droneTransform.right, p, Vector3.up), -1, 1);
		}
		
		// caches the vehicles velocity to be used as the forward rays direction and length 
		Vector3 rayDirection = hoverScript.HCRigidBody.velocity;
		// front: fires a ray in the rayDirection scaled by the sensorDatas front ray scale against the boundary layer
		if(Physics.Raycast(front.position, rayDirection, out hit, rayDirection.magnitude*sensorData.frontRayScale, collisionLayer)) {
			float dotP = Vector3.Dot(droneTransform.right, rayDirection.normalized);
			if(dotP > -0.8f && dotP < 0.8f) {
				angle -= dotP;
				float frontRange = 0.2f;
				if(dotP > -frontRange && dotP < frontRange) {
					if(hoverScript.Thrust > -frontRange)
						hoverScript.Thrust = -frontRange;
					hoverScript.Thrust = Mathf.Lerp(hoverScript.Thrust, -1, Time.fixedTime*2);
					hitCentre = true;
				}
			}
			Debug.DrawLine(front.position, front.position+(rayDirection*sensorData.frontRayScale),Color.red);
		}
		else
			Debug.DrawLine(front.position, front.position+(rayDirection*sensorData.frontRayScale),Color.yellow);

//		old setup (now irrelevant)
//		///front Sensors			
//		if(Physics.Raycast(front.position, front.forward, out hit, sensorData.frontRayLength, collisionLayer)) {
//			hoverScript.Thrust = Mathf.Lerp(hoverScript.Thrust, -1, (sensorData.frontRayLength/hit.distance));
//			hitCentre = true;
//			Debug.DrawLine(front.position, front.position+(front.forward*sensorData.frontRayLength),Color.red);
//			Time.timeScale = 0;
//		}
//		else
//			Debug.DrawLine(front.position, front.position+(front.forward*sensorData.frontRayLength),Color.yellow);
//		///front Side Sensors
//		///front left
//		if(Physics.Raycast(frontLeft.position, frontLeft.forward, out hit, sensorData.steerRayLength, collisionLayer)) {
//			angle += Mathf.Lerp (0, sensorData.frontRedirectForce, (hit.distance/sensorData.steerRayLength));
//			Debug.DrawLine(frontLeft.position, frontLeft.position+(frontLeft.forward*sensorData.steerRayLength),Color.red);
//		}
//		else
//			Debug.DrawLine(frontLeft.position, frontLeft.position+(frontLeft.forward*sensorData.steerRayLength),Color.yellow);
//		
//		///front right
//		if(Physics.Raycast(frontRight.position, frontRight.forward, out hit, sensorData.steerRayLength, collisionLayer)) {
//			angle += Mathf.Lerp (-sensorData.frontRedirectForce, 0, (hit.distance / sensorData.steerRayLength));
//			Debug.DrawLine(frontRight.position, frontRight.position+(frontRight.forward*sensorData.steerRayLength),Color.red);
//		}
//		else
//			Debug.DrawLine(frontRight.position, frontRight.position+(frontRight.forward*sensorData.steerRayLength),Color.yellow);
		
		// fires rays int opposite directions from the back of the vehicle along the vehicles X axis
		// back left
		if(Physics.Raycast(backLeft.position, backLeft.forward, out hit, sensorData.backRayLength, collisionLayer)) {
			angle += Mathf.Lerp(0, sensorData.backRedirectForce, (hit.distance / sensorData.backRayLength));
			Debug.DrawLine(backLeft.position, backLeft.position+(backLeft.forward*sensorData.backRayLength),Color.red);
		}
		else
			Debug.DrawLine(backLeft.position, backLeft.position+(backLeft.forward*sensorData.backRayLength),Color.yellow);
		
		// back right
		if(Physics.Raycast(backRight.position, backRight.forward, out hit, sensorData.backRayLength, collisionLayer)) {
			angle += Mathf.Lerp (-sensorData.backRedirectForce, 0, (hit.distance / sensorData.backRayLength));
			Debug.DrawLine(backRight.position, backRight.position+(backRight.forward*sensorData.backRayLength),Color.red);
		}
		else
			Debug.DrawLine(backRight.position, backRight.position+(backRight.forward*sensorData.backRayLength),Color.yellow);
		
		
		if(!hitCentre && hoverScript.Thrust != 1)
			hoverScript.Thrust = 1;
		
		if(angle < -1) { angle = -1; }
		if(angle > 1) { angle = 1; }
		hoverScript.Torque = angle;
	}
	
	// returns the angle in radians*2 to the target direction along the given axis
	// forward & right are relative vehicle directions and target is the point to turn towards
	// the axis parameter will always Vector3.up unless a fix is found for the problem where  
	// blender discards local orientation data
	float TurnToPoint(Vector3 forward, Vector3 right, Vector3 target, Vector3 axis){
		return Vector3.Angle(forward, target)*Mathf.Sign(Vector3.Dot(axis, Vector3.Cross(forward, target)))*Mathf.Deg2Rad*2;			
	}
	
	void OnTriggerEnter(Collider other) {
		if(isEnabled && other.tag == "Waypoint") {
			CheckPointScript passed = other.gameObject.GetComponent<CheckPointScript>();
			newCheckpoint =  passed.Number;
			trackForward = passed.checkPointTransform.forward;
			newSegment = passed.Segment;
			if(positionInfo.currentCheckpoint != newCheckpoint) {
				positionInfo.checkpointTime = GetCurrentTimeInMilliseconds();
				pointCrossed = passed.GetClosestPoint(droneTransform.position);
				positionInfo.currentCheckpoint = newCheckpoint;

				if(Vector3.SqrMagnitude(targetWaypoint-droneTransform.position) < wpUpdateDistance*wpUpdateDistance) {
					targetCheckpoint = pathBuilder.segments[pathBuilder.getNextSegment(positionInfo.currentSegment)].end;
					CheckPointScript newTarget = pathBuilder.waypoints[targetCheckpoint].GetComponent<CheckPointScript>();
					targetPoint = pathBuilder.segments[pathBuilder.getNextSegment(positionInfo.currentSegment)].targetPoint;
					
					targetWaypoint = newTarget.getPoint(targetPoint);
				}
					
				CheckPointType ct = CrossedCheckpoint();
				UpdateData(ct);
			}
		}
	}
	void OnCollisionEnter(Collision collision) {
		GameObject crashSound = Instantiate(CrashPrefab, collision.contacts[0].point, Quaternion.identity) as GameObject;
		crashSound.GetComponent<CrashSoundScript>().PlaySound(positionInfo.droneName);
	}
//	only relevant to Dave (only for use in development)
	// Used to report on number of collisions
//	void OnCollisionStay(Collision collision) {
//		if(collision.gameObject.tag == "Boundary") {
//			Debug.Log("Hit Wall");
//			noOfCollisions++;
//		}
//	}
	
	// Used to update race tracking/drone reporting data
	void UpdateData(CheckPointType type) {
		WaypointData wd = ScriptableObject.CreateInstance<WaypointData>();
		wd.Initialize(newCheckpoint, targetCheckpoint, positionInfo.checkpointTime, hoverScript.moveSpeed, pointCrossed, targetPoint);
		if(type == CheckPointType.LapLine) {
			IncrementLap(wd);
		}
//		only relevant to Dave (only for use in development)
//		else if(type == CheckPointType.SegmentLine) {
//			if(positionInfo.lapNumber > 0) {
//				currentLapData.segmentData[positionInfo.currentSegment] = NewSegment();
//				if(positionInfo.currentSegment> 0) {
//					currentLapData.segmentData[positionInfo.currentSegment-1].SetEndValues(noOfCollisions, Time.time);
//					noOfCollisions = 0;
//				}
//				currentLapData.waypointsData[positionInfo.currentCheckpoint] = wd;
//			}
//		}
//		else{
//			if(positionInfo.lapNumber > 0) {
//				currentLapData.waypointsData[positionInfo.currentCheckpoint] = wd;
//			}
//		}
	}
	
	void IncrementLap(WaypointData wd) {
		positionInfo.lapNumber+=lapIncrement;
		// increment the lapReached value (used to track the current lap)
		// only if positionInfo.lapNumber (used for race position tracking)
		// is higher
		if(positionInfo.lapNumber > lapReached) {
			lapReached = positionInfo.lapNumber;
			// if a lap has been completed save its time
			if(lapReached > 1)
				lastLapTime = LapTime;
			lapStartTime = Time.time;
//			only relevant to Dave (only for use in development)
//			SegmentData sd = NewSegment();
//			currentLapData = ScriptableObject.CreateInstance<LapData>();
//			currentLapData.Initialize(droneTransform.name+ "(Lap " + positionInfo.lapNumber + ")", pathBuilder.segments.Count, sd, pathBuilder.waypoints.Count, wd, Time.time);
//			currentLapData.segmentData[positionInfo.currentSegment] = sd;
//			if (positionInfo.lapNumber > 1) {
//				currentLapData.Finalize(Time.time);
//				Dave.SaveReport(currentLapData, positionInfo.droneName);
//				completedLapData.Add(currentLapData);
//			}
		}
		if(currentLapText) {
			if(lapReached < 4) {
				currentLapText.text = "Lap "+lapReached.ToString();
			}
			else {
				currentLapText.fontSize = 40;
				currentLapText.text = "Race Over!";
			}
		}
		lapIncrement = 0;
	}
	
//  only relevant to Dave (only for use in development)
//	SegmentData NewSegment() {
//		SegmentData sd = ScriptableObject.CreateInstance<SegmentData>();
//		sd.Initialize(newCheckpoint, pathBuilder.segments[positionInfo.currentSegment].end, Time.time, hoverScript.moveSpeed, sensorData);
//		return sd;
//	}
	
	// updates position info based on the crossed checkpoint and returns
	// a checpoint type defining the type of checkpoint crossed
	public CheckPointType CrossedCheckpoint() {
		// check if the drone has crossed a lap line in either direction
		bool increaseLap = (newSegment == 0 && positionInfo.currentSegment == pathBuilder.segments.Count-1);
		bool decreaseLap = (positionInfo.currentSegment == 0 && newSegment == pathBuilder.segments.Count-1);
		if(increaseLap) {
			lapIncrement = 1;
			positionInfo.currentSegment = newSegment;
			return CheckPointType.LapLine;
		}
		else if (decreaseLap) {
			lapIncrement = -1;
			positionInfo.currentSegment = newSegment;
			return CheckPointType.LapLine;
		}
		else if (newSegment == positionInfo.currentSegment+1 || newSegment == positionInfo.currentSegment-1) {
			positionInfo.currentSegment = newSegment;
			return CheckPointType.SegmentLine;
		}
		else {
			return CheckPointType.None;
		}
	}
	
	// realign the sensors based on defined angles
	// for use by Editor Script
	public void SetUpSensors() {
//		frontLeft.localRotation = Quaternion.Euler(0, -sensorData.frontAngle, 0);
//		frontRight.localRotation = Quaternion.Euler(0, sensorData.frontAngle, 0);
		backLeft.localRotation = Quaternion.Euler(0, -sensorData.backAngle, 0);
		backRight.localRotation = Quaternion.Euler(0, sensorData.backAngle, 0);
	}
	
	// Deactivates the drone. reseting thrust and torque
	// the death timer defines how long will pass before the vehicle is destroyed
	public void DeactivateDrone(float deathTimer) {
		hoverScript.Torque = 0;
		hoverScript.Thrust = 0;
		isEnabled = false;
		this.deathTimer = deathTimer;
	}
	
	// DateTime reference to the year 2000 (time will be measure fron this point)
	// will be used to get more accurate checkpoint times than unitys Time class is capable of providing
	// results are unpredctable 
	private static readonly DateTime Y2K = new DateTime(2000, 1, 1, 0, 0, 0, DateTimeKind.Utc);
	// returns the TimeSpan since 00:00AM on 1/1/2000 in milliseconds.  Will return 
	// negative and therefor invalid numbers if the system time is set before this time
	public static double GetCurrentTimeInMilliseconds() {
		return (DateTime.UtcNow - Y2K).TotalMilliseconds;
	}
}