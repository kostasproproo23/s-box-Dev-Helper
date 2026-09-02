using Sandbox;

public sealed class SimpleObbySystem : Component
{
	//To look for the other checkpoint code, go to the simple checkpoint system! (These 2 are connected)

	//This is going to be a simple obby system.
	//It will have checkpoints, and "death" if the player falls too low in world height

	//Here we set up some initial properties
	[Property] [Sync] public int CurrentCheckpoint {get; set;}
	[Property] private int StartingCheckpoint {get; set;} = 1;

	[Property] private int DeathHeight = -400;
	[Property] public Vector3 RespawnPositon;

	//Quick note: you have to input the values in the editor

	//We will first make a simple death function wich is going to be public, so implementing kill bricks or other death related stuff will be easier
	public void Death()
	{
		if (IsProxy) return;

		GameObject.WorldPosition = RespawnPositon;
	}

	protected override void OnStart()
	{
		if (IsProxy) return;

		//Some Simple Setup
		CurrentCheckpoint = StartingCheckpoint;
	}

	protected override void OnFixedUpdate()
	{
		if (IsProxy) return;

		//In here we will check player height, and if it falls below a spesific threshold, we move the player to the wanted checkpoint

		if( GameObject.WorldPosition.z <= DeathHeight)
		{
			Death();
		}
	}
}
