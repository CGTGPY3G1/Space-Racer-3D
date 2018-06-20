using UnityEngine;
using System.Collections;

/// <summary>
/// Adapted version of Smooth Follow from http://wiki.dreamsteep.com/Unity_tools
/// </summary>


[System.Serializable]
public class CamScript : MonoBehaviour {
	
	// The target to follow
	public Transform target;
	// The distance in the x-z plane to the target
	public float distance = 10;
	// the height we want the camera to be above the target
	public float height = 5;
	// How much we 
	public float heightDamping = 2;
	public float rotationDamping = 3;
	
	public bool interp;
		
	void SetTarget(Transform target) {
		this.target = target;
	}
	
	void LateUpdate () {
		// Early out if we don't have a target
		if (!target)
			return;
		
		// Calculate the tequired rotation angle and height
		float wantedRotationAngle = target.rotation.eulerAngles.y;
		float wantedHeight = target.position.y + height;
		// Calculate the current rotation angle and height
		float currentRotationAngle = transform.rotation.eulerAngles.y;
		float currentHeight = transform.position.y;
		
		// Damp the rotation around the y-axis
		currentRotationAngle = Mathf.LerpAngle (currentRotationAngle, wantedRotationAngle, rotationDamping * Time.deltaTime);
		
		// Damp the height
		currentHeight = Mathf.Clamp(Mathf.Lerp (currentHeight, wantedHeight, Mathf.Abs(currentHeight-wantedHeight) * Time.deltaTime), target.position.y+(height*0.75f), target.position.y+height*1.25f) ;
		
		// Convert the angle into a rotation
		Quaternion currentRotation = Quaternion.Euler (0, currentRotationAngle, 0);
		
		// Set the position of the camera on the x-z plane to:
		// distance meters behind the target
		Vector3 newPosition = target.position;
		newPosition -= currentRotation * Vector3.forward * distance;
		
		// Set the height of the camera
		newPosition.y = currentHeight;
		if(interp) {
			// linearly interpolate towards the desired position
			transform.position = Vector3.Lerp(transform.position, newPosition, (Vector3.Distance(transform.position, newPosition)));
		}
		else {
			// set new position immediately
			transform.position = newPosition;
		}
		// look at the target
		transform.LookAt (target);
	}
}
