using UnityEngine;
using System.Collections;
using System.Xml.Serialization;

[System.Serializable]
[XmlRoot]
public class SensorData {

	[Header("Angles")]
	[XmlAttribute]
	public float backAngle;
//	public float frontAngle;
	
	[Header("Forces + Scales")]
	[XmlAttribute]
	public float backRedirectForce;
//	public float frontRedirectForce; 
//	public float speedScaler;
	public float frontRayScale;
	public float backRayLength;
//	public float steerRayLength;
//	public float steerScaler;
	
}
