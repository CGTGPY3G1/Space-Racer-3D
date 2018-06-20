using UnityEngine;
using System.Collections;
using System.Xml.Serialization;

[System.Serializable]
[XmlRoot]
public class WaypointData : ScriptableObject {
	[XmlAttribute]
	public int number, targetWaypoint;
	[XmlAttribute]
	public int pointCrossed, targetPoint;
	[XmlAttribute]
	public float speed;
	[XmlAttribute]
	public double time;
	
	/// <summary>
	/// Initialize the specified number, targetWaypoint, time, speed, pointCrossed and targetPoint.
	/// </summary>
	/// <param name="number">this waypoints number.</param>
	/// <param name="targetWaypoint">the new target waypoint.</param>
	/// <param name="time">the time the waypoint was crossed.</param>
	/// <param name="speed">the speed the vehicle was travelling at.</param>
	/// <param name="pointCrossed">the point of the track segment the vehicle is closest to.</param>
	/// <param name="targetPoint">the new target point.</param>
	public void Initialize(int number, int targetWaypoint, double time, float speed, int pointCrossed, int targetPoint) {
		this.number = number; this.targetWaypoint = targetWaypoint; this.time = time; this.speed = speed; 
		this.pointCrossed = pointCrossed; this.targetPoint = targetPoint;
	}
}
