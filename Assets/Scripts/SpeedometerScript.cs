using UnityEngine;
using UnityEngine.UI;
using System.Collections;

[System.Serializable]
public class SpeedometerScript : MonoBehaviour {

	private Image image;
	private Drone drone;
	HoverScript hoverScript;
	// Use this for initialization
	void Start () {
		image = GetComponent<Image>();
	}
	
	// sets the dron whose speed will be tracked
	public void SetDrone(Drone drone) {
		this.drone = drone;
		hoverScript = drone.HoverScript;
	}
	
	// Update is called once per frame
	void Update () {
		if(drone && !hoverScript) 
			hoverScript = drone.HoverScript;
		if(image && hoverScript) {
			image.fillAmount = Mathf.Clamp(hoverScript.moveSpeed/hoverScript.MAX_SPEED, 0, 1);
		}
	}
}
