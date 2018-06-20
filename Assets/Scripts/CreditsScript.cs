using UnityEngine;
using System.Collections;

public class CreditsScript : MonoBehaviour {
	// Available credits screens
	public GameObject[] screens;
	// The currently selected screen
	int selectedScreen = 0;	
	// Time until the screen switches automatically
	float switchTimer;
	
	// Used to show the next screen
	void ShowNextScreen() {
		// disable the old screen
		screens[selectedScreen].SetActive(false);
		selectedScreen++;
		// make sure the selected value does not 
		// exceed the range of the screens array
		if(selectedScreen >= screens.Length)
			selectedScreen = 0;
		// show the new screen
		screens[selectedScreen].SetActive(true);
		// reset the timer
		switchTimer = 8;
	} 
	
	// Use this for initialization
	void Start() {
		switchTimer = 8;
	}
	
	// Update is called once per frame
	void Update () {
		// count down to screen switch
		switchTimer -= Time.deltaTime;
		// show the next screen if the timer is 0 or the player 
		// hits the Submit button (Keyboard: enter | Xbox360 controller: A)
		if(switchTimer <= 0 || Input.GetButtonDown("Submit")) {
			ShowNextScreen();
		}
	}
}
