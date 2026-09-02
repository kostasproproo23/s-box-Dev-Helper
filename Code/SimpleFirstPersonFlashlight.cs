using System;
using Sandbox;

public sealed class SimpleFirstPersonFlashlight : Component
{
	//In this example we will create a first person flashlight
	//Make sure this script in inside the player prefab, and there is a gameobject with a spot light component

	//Quick note: You can use the same stuff to make a first person viewmodel. Everything is the same, except for the light part...

	[Property] public SpotLight LightSource {get; set;}
	[Property] public float LerpSpeed {get; set;}

	protected override void OnPreRender()
	{
		//Only the owner of this player object is allowed here!
		if (IsProxy) return;
		//We use this function so we dont have any visual errors when lerping our spotlight

		//Simple put the flashlight to the position of the camera
		LightSource.WorldPosition = new Vector3(Scene.Camera.WorldPosition);

		//And now lerp the rotation
		//We first input the start of the lerp, then the end, then the speed (We do Time.Delta so the lerp is consistent among all framerates)
		// and we put the last thing on true so it does not overshoot the target
		LightSource.WorldRotation = Rotation.Lerp(LightSource.WorldRotation, Scene.Camera.WorldRotation, LerpSpeed * Time.Delta, true);
	}

	protected override void OnStart()
	{
		if (IsProxy)
		{
			//destroy the light for other players (this method of making the flashlight work would not work in multiplayer, as the other players dont know where each client's camera is)
			//Quick tip, you can make a [Sync] variables holding the positions and rotations and each client will update those and the other clients will read and do their thing
			LightSource.Destroy();
		}
	}

	//Now lets make it turn on and off
	protected override void OnUpdate()
	{
		if (IsProxy) return;

		//This is the default name for the left click in Settings --> Input
		if (Input.Pressed("Attack1"))
		{
			//What this does, is it just flips the Enabled the opposite way. Lets say it is Enabled = false, it would make it the oposite of Enabled = false, which is true
			LightSource.Enabled = !LightSource.Enabled;
		}
	}
}
