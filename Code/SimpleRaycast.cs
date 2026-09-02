using Sandbox;

public sealed class SimpleRaycast : Component
{

	//In this simple example you will learn how to use a raycast on for a first person game, to determent what items or objects are in front of the player

	private CameraComponent Camera;

	protected override void OnStart()
	{
		//Here we just get the scene camera, the one that the player has
		Camera = Scene.Camera;
	}

	protected override void OnUpdate()
	{
		//We want this code to run only on the client that owns the player
		if (IsProxy) return;
		//If the camera does not exist, then dont run anything
		if (Camera == null) return; 

		//here is our raycast (Starting Position, EndPosition). We get the starting position by just getting the position of our camera.
		//Now the end position is a little tricky for a begginer, but it is simple. We first start from the camera position again, and 
		//on the forward direction of the camera (where the camera is pointing), we go 100 units in that direction. And thats our end position
		//Now for the collisionRules you just need to create a tag with whatever name you want (here it is raycast), and inside settings --> physics add 
		//wich objects the ray will interact with. Just make sure to have player to not collide so it does not collide with your own player
		var Raycast = Scene.Trace.Ray(Camera.WorldPosition, Camera.WorldPosition + Camera.WorldRotation.Forward * 100).WithCollisionRules("raycast").Run();
	
		//There are a lot of other stuff you can get from the raycast but here are some of the basics
		if (Raycast.GameObject != null)
		{
			//Log.Info(Raycast.GameObject.Name);
			//Log.Info(Raycast.HitPosition);
		}
	}
}
