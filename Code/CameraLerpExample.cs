using Sandbox;

public sealed class CameraLerpExample : Component
{
	//Here we will make a simple camera lerp example where the camera will smoothly follow the the target!

	//Set up some variables
	[Property] public GameObject CameraTarget {get; set;}
	[Property] public GameObject Camera {get; set;}

	//This determines how smoothly or choppy the camera will follow the target!
	[Property] public float CameraLerpSpeed {get; set;} = 5f;
	[Property] public float CameraRoationLerpSpeed {get; set;} = 5f;

	protected override void OnPreRender()
	{
		//Here we will move the camera! (We are using prerender cause it will be coppy and laggy if we do it inside OnUpdate. (Quick note: I am not entirly sure about that though))

		//Here is where the magic happens (You can find everywhere what lerp actually does, i will not be explaining lerp itself here, just how you can use it!)
		// The first parameter is where we are, the second is where we want to go and the last you can imagine it like speed (not kinda but it works if you think it like that)
		// Time.Delta is like a balancing float so it keeps stuff the same on every frame rate (Because this function runs every frame, if you did not have Time.Delta then the camera would move faster on bigger FPS)
		Camera.WorldPosition = Vector3.Lerp(Camera.WorldPosition, CameraTarget.WorldPosition, Time.Delta * CameraLerpSpeed);

		//We now need to rotate the camera so it looks at the player
		//We first need to get the rotation the camera needs to be to be looking at the player so we can lerp it!

		//We first get the direction. Look position - Current Position
		var direction = GameObject.WorldPosition - Camera.WorldPosition;

		//Now we get the rotation, and we make sure to add "Vector3.Forward" so it looks at the forward direction
		var LookRotation = Rotation.LookAt(direction, Vector3.Forward);

		Angles angles = LookRotation.Angles();
		angles.roll = 0;

		//Finally we need to lerp the rotation! (We use Slerp for rotation because its better :) )
		Camera.WorldRotation = Rotation.Slerp(Camera.WorldRotation, angles , Time.Delta * CameraRoationLerpSpeed);
	}
}
