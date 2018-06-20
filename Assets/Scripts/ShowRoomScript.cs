using UnityEngine;
using System.Collections;

public class ShowRoomScript : MonoBehaviour {

	Vector3 target;
	float moveSpeed, rotationSpeedH = 90, rotationSpeedV = 90;
	Transform cachedTransform;
	bool isSelected;
	public bool IsSelected {
		get { return isSelected; }
		set { isSelected = value; }
	}
	
	// Use this for initialization
	void Start () {
		cachedTransform = transform;
	}
	
	// Update is called once per frame
	void Update () {
		float delta = Time.deltaTime;
		// move towards target position
		if (cachedTransform.position != target) {
			cachedTransform.position = Vector3.Lerp(cachedTransform.position, target, moveSpeed*delta);
		}
		if(isSelected) {
			// spin vehicle
			float verticalRotation = Input.GetAxis("Vertical")*rotationSpeedV*delta;
			cachedTransform.Rotate(Vector3.up * rotationSpeedH*delta, Space.World);
			cachedTransform.Rotate(Vector3.right * verticalRotation, Space.Self);
		}
	}
	
	/// <summary>
	/// Set the target position
	/// </summary>
	/// <param name="position"> The position to move to.</param>
	/// <param name="speed"> The speed to move at.</param>
	public void MoveToPosition(Vector3 position, float speed) {
		target = position; moveSpeed = speed;
	}
	
	/// <summary>
	/// Teleports to a position.
	/// </summary>
	/// <param name="position"> The position to teleport to.</param>
	public void TeleportToPosition(Vector3 position) {
		cachedTransform.position = position;
		target = position;
	}
	
	/// <summary>
	/// Teleports to a position, oriented to a rotation.
	/// </summary>
	/// <param name="position"> The position to teleport tothe position to teleport to.</param>
	/// <param name="rotation"> The rotation to be positioned in.</param>
	public void TeleportToPosition(Vector3 position, Quaternion rotation) {
		target = position;
		cachedTransform.position = position;
		cachedTransform.rotation  = rotation;
	}
}
