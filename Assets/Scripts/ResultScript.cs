using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Used to Display the result of a race before moving on to player data screen
/// </summary>
public class ResultScript : MonoBehaviour {
	// Minimum and Maximum alpha values
	const float MIN_ALPHA = 0, MAX_IMAGE_ALPHA = 0.9f, MAX_TEXT_ALPHA = 1;
	// black background image
	public Image backgroundImage;
	// Text box used to print result
	public Text resultText;
	// colours of the image and text
	Color imageColour, textColour;
	// true if the result is being shown
	bool showingResult;
	
	// Use this for initialization
	void Start () {
		showingResult = false;
		imageColour = backgroundImage.color;
		textColour = resultText.color;
	}
	
	// Update is called once per frame
	void Update () {
		float delta = Time.deltaTime*0.25F;
		if(showingResult) {
			if(imageColour.a < MAX_IMAGE_ALPHA) {
				imageColour.a += delta;
				if(imageColour.a > MAX_IMAGE_ALPHA)
					imageColour.a = MAX_IMAGE_ALPHA;
				backgroundImage.color = imageColour;
			}
			if(textColour.a < MAX_TEXT_ALPHA) {
				textColour.a += delta;
				if(textColour.a > MAX_TEXT_ALPHA)
					textColour.a = MAX_TEXT_ALPHA;
				resultText.color = textColour;
			}
		}
		else {
			if(imageColour.a > MIN_ALPHA) {
				imageColour.a -= delta;
				if(imageColour.a < MIN_ALPHA)
					imageColour.a = MIN_ALPHA;
				backgroundImage.color = imageColour;
			}
			if(textColour.a > MIN_ALPHA) {
				textColour.a -= delta;
				if(textColour.a < MIN_ALPHA)
					textColour.a = MIN_ALPHA;
				resultText.color = textColour;
			}
		}
	}
	
	// Sets the result and beins to show it
	public void ShowResult(string result) {
		resultText.text = result;
		showingResult = true;
	}
}
