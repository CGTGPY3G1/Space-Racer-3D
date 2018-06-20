using UnityEngine;
using System.Collections;

public class RaceStartsScript : MonoBehaviour {
	AudioSource audioSource;
	public AudioClip[] beeps;
	public void PlayClip(int clipNumber){
		audioSource.PlayOneShot(beeps[clipNumber]);
	}
	// Use this for initialization
	void Start () {
		audioSource = GetComponent<AudioSource>();
	}
}
