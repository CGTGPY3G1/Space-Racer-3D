using UnityEngine;
using System.Collections;

/// <summary>
/// Accelerates any vehicle that mves over it while also
/// activating deactivating the boost points particle effect
/// </summary>

[System.Serializable]
public class BoostScript : MonoBehaviour {
	// the force to be applied to passing vehicles
	private static int BOOST_FORCE = 2000;
	// particles will not emit while this is <= 0
	float effectTime;
	// used to track state of particle emissions (on/off)
	bool isActive;
	// Boost point visual effect
	public ParticleSystem particles;
	 
	// Use this for initialization
	void Start () {
		SetEmission(false);
	}
	
	// Update is called once per frame
	void Update () {
		// if particle emissions are active
		if(isActive) {
			if(effectTime > 0) {
				effectTime -= Time.deltaTime;
			} 
			else {
				// disable emissions
				SetEmission(false);
			}
		}
	}
	
	// when a vehicle enter the boost points effect timer will be set to 2 
	// particle emissions will also be enabled (assuming they aren't already)
	void OnTriggerEnter(Collider other) {
		if (!isActive) {
			SetEmission(true);
		}
		effectTime = 2; 
	}
	
	// the vehicles rigidbody will be pushed in the boost points forward direction
	void OnTriggerStay(Collider other) {
		if (other.attachedRigidbody)
			other.attachedRigidbody.AddForce(transform.forward * BOOST_FORCE);        
	}
	
	// Enable/Disable Particle Emission
	void SetEmission(bool isEmitting) {
		isActive = isEmitting;
		particles.enableEmission = isEmitting;  
	}
}
