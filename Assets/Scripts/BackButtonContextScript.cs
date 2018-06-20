using UnityEngine;
using System.Collections;

/// <summary>
/// Used yp Display info about the back buttons current function
/// only used on menu
/// </summary>
public class BackButtonContextScript : MonoBehaviour {
	// Will be false on Main Menu
	// and true everywhere else
	bool canReturn;
	// Gets/Sets canReturn variable.  Will
	// enable/disable relevant info text when Set
	public bool CanReturn {
		get { return canReturn; }
		set { 
			canReturn = value;
			exitImage.SetActive(!canReturn);
			returnImage.SetActive(canReturn);
		}
	}
	// Text info images 
	public GameObject exitImage, returnImage;
}
