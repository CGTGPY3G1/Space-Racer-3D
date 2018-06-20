//MachineGun Rotation Script (By Reikan - 2014)

public var RotationSpeedMax : float = 400;					//RotationSpeed Max Value					
private var RotationSpeed : float = 0;						//Actual RotationSpeed
public var RotationSpeedIncreaseValue : float = 10;			//RotationSpeed increase value
public var RotationObject : GameObject;						//Object to rotate (Z Axis)


//Script init
function Start () 
{
	//Actual rotation speed to 0
	RotationSpeed = 0.0;
}

//On update
function Update () 
{
	//If SpaceBar Key is down
	if (Input.GetKey(KeyCode.Space))
	{
		//If actual rotationspeed < max rotationspeed value
		if (RotationSpeed < RotationSpeedMax)
		{
			RotationSpeed = RotationSpeed + RotationSpeedIncreaseValue + Time.deltaTime;
		}
		else
		{
			RotationSpeed = RotationSpeedMax;
		}
	}
	else
	{
		if (RotationSpeed > 0)
		{
			RotationSpeed = RotationSpeed - RotationSpeedIncreaseValue + Time.deltaTime;
		}
		else
		{
			RotationSpeed = 0;
		}
	}
	
	//We rotate object on the Z axis
	RotationObject.transform.Rotate(Vector3(0, 0, Time.deltaTime * RotationSpeed));
}