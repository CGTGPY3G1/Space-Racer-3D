using UnityEngine;
using System.Collections;
using System.Collections.Generic;
public class CrashSoundScript : MonoBehaviour {
	// length of time before the object destroys itself
	float deathCounter = 1000;
	// Crash audio effects (should be named after vehicles they represent)
	public AudioClip[] crashSounds;
	
	// Play the sound relating to the vehicle
	// named in the vehicle parameter
	public void PlaySound(string vehicle) {
		foreach(AudioClip clip in crashSounds) {
			// if the clip name matches the vehicle name 
			if(clip.name == vehicle) {
				GetComponent<AudioSource>().PlayOneShot(clip);
				deathCounter = 1.5f;
				break;
			}//End if
		}//End for
	}//End PlaySound
	
	void Update() {
		if(deathCounter > 0)
			deathCounter -= Time.deltaTime;
		else 
			Destroy(this.gameObject);
		//End if	
	}//End Update
}
