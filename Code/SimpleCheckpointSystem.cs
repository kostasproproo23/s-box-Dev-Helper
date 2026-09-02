using Sandbox;

public sealed class SimpleCheckpointSystem : Component, Component.ITriggerListener
{
	//To look for the other checkpoint code, go to the simple obby system! (These 2 are connected)

	[Property] private int CheckpointNumber {get; set;}

	//This stuff is really primitive, but if you still dont know. Find the code called "simple trigger" for more details
	public void OnTriggerEnter(Collider other)
	{
		//Check if what we have collided with is a player, and we own the player
		if (other.Tags.Has("player") && other.IsProxy == false)
		{

			//See if we can find the script
			var SimpleObbySystem = other.GetComponentInParent<SimpleObbySystem>();
			if (SimpleObbySystem != null)
			{
				//If we have a lower checkpoint, then add this checkpoint as the current checkpoint
				if (SimpleObbySystem.CurrentCheckpoint < CheckpointNumber)
				{
					SimpleObbySystem.CurrentCheckpoint = CheckpointNumber;
					SimpleObbySystem.RespawnPositon = GameObject.WorldPosition;
				}
			}
			else
			{
				Log.Error("Something went wrong!");
			}
		}
	}
}
